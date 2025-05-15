using System.ComponentModel.DataAnnotations;

namespace DevTools.Application.Utils
{
    public static class ValidationUtils
    {
        public static void ValidateEmail(string? email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email), "Email cannot be null");

            if (!new EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("Invalid email address", nameof(email));
        }
    }
}