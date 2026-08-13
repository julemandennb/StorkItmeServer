namespace StorkItmeServer.FromBody.User
{
    public class UserFromBody
    {
        public required string Email { get; init; }
        public required string UserName { get; init; }
        public required string Password { get; init; }

        public required string ConfirmPassword { get; init; }

        public required string Role { get; init; }
    }
}
