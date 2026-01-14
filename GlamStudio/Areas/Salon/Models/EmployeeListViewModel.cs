namespace GlamStudio.Models
{
    public class EmployeeListViewModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}