using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MediRecords.Utility
{
	public class EmailHelper
    {
        /// <summary>
        /// Validates the format and length of an email address.
        /// </summary>
        /// <param name="email">The email address string to validate.</param>
        /// <returns>A tuple indicating if the email is valid and a descriptive message.</returns>
        public static (bool IsValid, string Message) ValidateEmail(string email)
        {
            // Check for null, empty, or whitespace strings
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email address cannot be empty.");

            // Standard maximum length for an email address
            if (email.Length > 254)
                return (false, "Email is too long.");

            try
            {
                // Use built-in MailAddress class for basic format verification
                var addr = new MailAddress(email);

                if (addr.Address != email)
                    return (false, "Invalid email format.");

                // Regex pattern to ensure the presence of an '@' and a domain dot
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(email, pattern))
                    return (false, "Email must have a valid domain.");

                return (true, "Email is valid.");
            }
            catch (FormatException)
            {
                // Handle cases where MailAddress constructor fails
                return (false, "The email format is incorrect.");
            }
        }
    }
}


