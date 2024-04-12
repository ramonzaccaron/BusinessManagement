namespace Employees.Management.API.Domain
{
    public class Employee
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateOnly BirthDate { get; set; }

        public string Role { get; set; }
    }
}
