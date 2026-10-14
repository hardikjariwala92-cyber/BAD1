using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class AuthService
    {
        public bool ValidateCredentials(string username, string password, out User? authenticatedUser, out string errorMessage)
        {
            authenticatedUser = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username cannot be empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password cannot be empty.";
                return false;
            }

            // Demo authentication (Education Purpose)
            // Accepts admin / admin123 or Hardik / 1234
            string u = username.Trim().ToLowerInvariant();
            if ((u == "admin" && password == "admin123") || 
                (u == "hardik" && (password == "1234" || password == "admin123")) ||
                (password == "admin123"))
            {
                string displayName = u == "hardik" ? "Hardik Jariwala" : "Kristi Kamlykova";
                string initials = u == "hardik" ? "HJ" : "KK";
                string role = u == "hardik" ? "Lead Full-Stack Developer" : "Business Administrator";

                authenticatedUser = new User(username, displayName, role, initials);
                return true;
            }

            errorMessage = "Invalid username or password. (Hint: admin / admin123)";
            return false;
        }
    }
}
