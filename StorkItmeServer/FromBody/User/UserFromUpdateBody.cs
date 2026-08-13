namespace StorkItmeServer.FromBody.User
{
    public class UserFromUpdateBody
    {
        public string ?Email { get; set; }
        public string ?UserName { get; set; }

        public string ?Password { get; set; }

        public string ?ConfirmPassword { get; set; }

        public string ?Role { get; set; }
    }
}
