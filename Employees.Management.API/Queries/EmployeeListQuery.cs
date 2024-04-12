using Employees.Management.API.Converters;
using System.Text.Json.Serialization;

namespace Employees.Management.API.Queries
{
    public class EmployeeListQuery
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly BirthDate { get; set; }

        public string Role { get; set; }
    }
}
