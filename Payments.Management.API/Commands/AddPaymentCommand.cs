namespace Payments.Management.API.Commands
{
    public class AddPaymentCommand
    {
        public Guid EmployeeId { get; set; }

        public string Role { get; set; }
    }
}
