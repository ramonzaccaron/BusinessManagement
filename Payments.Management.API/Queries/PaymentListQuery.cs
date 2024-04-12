namespace Payments.Management.API.Queries
{
    public class PaymentListQuery
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string Role { get; set; }

        public decimal Salary { get; set; }
    }
}
