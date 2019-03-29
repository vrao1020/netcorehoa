namespace HoaEntities.Models.OutputModels
{
    public class UserViewDto
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Reminder { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
    }
}
