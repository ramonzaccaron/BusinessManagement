namespace Payments.Management.API.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; private set; }

        public string Role { get; private set; }

        public decimal Salary { get; private set; }

        public Payment(Guid employeeId, string role)
        {
            EmployeeId = employeeId;
            Role = role;

            SetSalary();
        }

        public void SetSalary()
        {
            if (Role == "Developer" || Role == "Tester")
                Salary = 5000;
            
            if (Role == "Manager")
                Salary = 8000;
            
            if (Role == "Director")
                Salary = 10000;
        }
    }
}
