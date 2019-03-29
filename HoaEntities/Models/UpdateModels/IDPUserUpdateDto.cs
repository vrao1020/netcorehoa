using HoaEntities.Models.ManipulationModels;

namespace HoaEntities.Models.UpdateModels
{
    public class IDPUserUpdateDto : IDPUserManipulationDto
    {
        public IDPUserUpdateDto() : base()
        {

        }

        public IDPUserUpdateDto(string id, string role,
            string readOnly, string postCreation) : base(id, role, readOnly, postCreation)
        {
        }
    }
}
