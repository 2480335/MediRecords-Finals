using System.Net;
using MediRecords.Dto.UserDtos;
using MediRecords.Repository.UserRoleRepository;
using MediRecords.Services.UserServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRoleRepository _userRoleRepository;

        public UserController(IUserService userService, IUserRoleRepository userRoleRepository)
        {
            _userService = userService;
            _userRoleRepository = userRoleRepository;
        }

        /// <summary>
        /// Lists all roles available for assignment to a user.
        /// Used to populate the role dropdown on the Admin user-registration form.
        /// </summary>
        [HttpGet("roles")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RoleOptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _userRoleRepository.GetAllAsync();
                var result = roles.Select(r => new RoleOptionDto
                {
                    RoleId = r.RoleId,
                    Name   = r.Name
                });
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = Constant.InternalError });
            }
        }

        /// <summary>
        /// Registers a new user into the MediRecords system.
        /// </summary>
        /// <param name="requestDto">The user registration data transfer object containing credentials and profile info.</param>
        /// <returns>Return the Success or ErrorMessage</returns>
        [HttpPost("register")]
        [Authorize(Roles = Constant.Admin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser(UserRegisterRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _userService.RegisterUserAsync(requestDto);
                return Ok(Constant.RegisterSuccess);
            }
            catch (MediRecordsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, Constant.InternalError);
            }
        }

        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        /// <returns>A collection of UserViewDto objects.</returns>
        // [Authorize(Roles = Constant.Admin)]
        [HttpGet("GetAll")]
        [Authorize(Roles = Constant.Admin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserViewDto>>> GetAll()
        {
            try
            {
                var result = await _userService.GetAllUsersAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = Constant.InternalError });
            }
        }

        /// <summary>
        /// Retrieves all physicians (providers) for dropdown selection across forms.
        /// </summary>
        /// <returns>A list of ProviderLookupDto objects (ProviderId, Name, Email).</returns>
        [HttpGet("providers")]
        [Authorize(Roles = Constant.Physician + "," + Constant.FrontDesk + "," + Constant.Admin)]
        [ProducesResponseType(typeof(IEnumerable<ProviderLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProviderLookupDto>>> GetAllProviders()
        {
            try
            {
                var result = await _userService.GetAllProvidersAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = Constant.InternalError });
            }
        }

        /// <summary>
        /// Fetches a specific user's details by their unique ID.
        /// </summary>
        /// <param name="id">The numeric ID of the user.</param>
        [Authorize(Roles = Constant.Admin)]
        [HttpGet("GetById/{id}")]
        [Authorize(Roles = Constant.Admin)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserViewDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = Constant.InvalidUserId });
            }

            try
            {
                var result = await _userService.GetUserByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new { message = string.Format(Constant.UserNotFound, id) });
                }

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = Constant.InternalError });
            }
        }


        /// <summary>
        /// Updates user details by an administrator.
        /// </summary>
        /// <param name="user">User details to be updated by admin</param>
        /// <returns>Returns updated user information</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid request or validation error</response>
        /// <response code="500">Server error</response>
        // [Authorize(Roles = "Admin")]  
        [HttpPut("update")]
        [Authorize(Roles = Constant.Admin)]
        [ProducesResponseType(typeof(UserUpdateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserByAdmin(
            [FromBody] UserUpdateRequestDto user) // <-- request body comes from JSON
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {

                var response = await _userService.UpdateUser(user);
                return Ok(new {Message = "User updated successfully", Data = response});
            }
            catch (ArgumentNullException ex)
            {
                // Bad request if required data is missing
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Not found if invalid arguments (like user not existing)
                return BadRequest(new { error = ex.Message });
            }
            catch (MediRecordsException ex)
            {
                // Not found if operation is invalid (like role mismatch)
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                // Internal server error for unexpected issues
                return StatusCode(500, Constant.InternalError);
            }

        }

        /// <summary>
        /// Allows users to reset their password by providing their email and new password details. 
        /// Only accessible by administrators to ensure security and proper user management.
        /// </summary>
        [HttpPost("forgotpassword")]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UserForgotPassword([FromBody] UserForgotPasswordDto model)
        {
            if (model == null)
                return BadRequest(Constant.Messages.InvalidRequest);

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(new { message = firstError });
            }

            var (success, message, statusCode) = await _userService.ForgotPasswordAsync(model);

            return statusCode switch
            {
                200 => Ok(new { message }),
                404 => NotFound(new { message }),
                500 => StatusCode(500, new { message }),
                _ => BadRequest(new { message })
            };
        }


        /// <summary>
        /// Soft deletes a user by marking the user as inactive.
        /// This operation can only be performed by an authorized admin user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles ="Admin")]
        [HttpPost("delete/{id}")]
        [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.SoftDeleteUserByIdAsync(id);
                return Ok(Constant.DeleteSucess);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, Constant.InternalError);
            }
        }
    }
}
