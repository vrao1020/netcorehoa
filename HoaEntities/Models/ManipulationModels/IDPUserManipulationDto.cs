namespace HoaEntities.Models.ManipulationModels
{
    public class IDPUserManipulationDto
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string ReadOnly { get; set; }
        public string PostCreation { get; set; }

        public IDPUserManipulationDto()
        {
        }

        public IDPUserManipulationDto(string id, string role,
            string readOnly, string postCreation)
        {
            this.Id = id;
            this.Role = role;
            this.ReadOnly = readOnly;
            this.PostCreation = postCreation;
        }
    }
}
