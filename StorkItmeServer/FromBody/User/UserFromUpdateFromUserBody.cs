namespace StorkItmeServer.FromBody.User
{
    public class UserFromUpdateFromUserBody
    {
        public required string  Password { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NewPassword { get; set; }

    }
}
