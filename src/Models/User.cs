namespace PROJECT_SBV1.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = "KK";

        public User() { }

        public User(string username, string fullName, string role, string avatarInitials)
        {
            Username = username;
            FullName = fullName;
            Role = role;
            AvatarInitials = avatarInitials;
        }
    }
}
