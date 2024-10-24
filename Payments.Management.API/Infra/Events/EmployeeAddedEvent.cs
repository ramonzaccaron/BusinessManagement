namespace Business.Management.Contracts
{
    public class EmployeeAddedEvent
    {
        public Guid EmployeeId { get; set; }

        public string Role { get; set; }
    }
}
