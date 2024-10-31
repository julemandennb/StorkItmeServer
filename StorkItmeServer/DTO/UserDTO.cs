

using StorkItmeServer.Model;

namespace StorkItmeServer.DTO
{
    public class UserDTO
    {

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<UserGroupDTO> UserGroups { get; set; }

        public UserDTO() { }

        public UserDTO(User user) {

            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;



        }
    }
}
