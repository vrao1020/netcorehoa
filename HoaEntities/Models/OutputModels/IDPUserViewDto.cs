using HoaEntities.Models.ManipulationModels;

namespace HoaEntities.Models.OutputModels
{
    public class IDPUserViewDto : IDPUserManipulationDto
    {
        public string Email { get; set; }
        public string EmailConfirmed { get; set; }
        public string Name { get; set; }

        public IDPUserViewDto() : base()
        {

        }

        public IDPUserViewDto(string id, string role,
            string readOnly, string postCreation,
            string email, string emailConfirmed,
            string name) : base(id, role, readOnly, postCreation)
        {
            this.Email = email;
            this.EmailConfirmed = emailConfirmed;
            this.Name = name;
        }
    }
}
