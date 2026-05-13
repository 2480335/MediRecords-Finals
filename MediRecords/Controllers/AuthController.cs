using Microsoft.AspNetCore.Mvc;
using MediRecords.Services.AuthServices;
using MediRecords.Dto.UserDtos;
using Microsoft.AspNetCore.Http;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and provides a login response (e.g., JWT token).
    /// </summary>
    /// <param name="dto">The login request data transfer object containing user credentials.</param>
    /// <returns>Returns the authentication result or an unauthorized error.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        //Missing fields check
        if(string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "400-Missing Fields" });
        }
        try
        {
            var result = await _authService.LoginUser(dto);

            if (result == null)
                return Unauthorized(new {message = "401-Username or password doesn't match" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    // AuthController Implemented Successfully
}