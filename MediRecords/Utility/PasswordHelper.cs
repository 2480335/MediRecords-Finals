using System;

namespace MediRecords.Utility;

public static class PasswordHelper
{
    /// <summary>
    /// Validates a password against length, casing, and character type requirements.
    /// </summary>
    /// <param name="password">The plain-text password string to validate.</param>
    /// <returns>A tuple containing a boolean success flag and a descriptive status message.</returns>
    public static (bool IsValid, string Message) ValidatePassword(string password)
    {
        // Ensure the password is not null or whitespace
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty.");

        // Enforce minimum character length requirement
        if (password.Length < 8)
            return (false, "Minimum 8 characters required.");

        // Check for at least one uppercase letter (A-Z)
        if (!password.Any(char.IsUpper))
            return (false, "At least one uppercase letter required.");

        // Check for at least one lowercase letter (a-z)
        if (!password.Any(char.IsLower))
            return (false, "At least one lowercase letter required.");

        // Check for at least one numeric digit (0-9)
        if (!password.Any(char.IsDigit))
            return (false, "At least one numeric digit required.");

        // Check for at least one special character or punctuation mark
        if (!password.Any(char.IsPunctuation))
            return (false, "At least one Special character is required.");

        // Return success if all security checks pass
        return (true, "Success");
    }
}
