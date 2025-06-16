using Swashbuckle.AspNetCore.Annotations;

namespace UserManagementApi.DTOs
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = default!;

        [SwaggerSchema(Format = "date")]
        public DateTime CreatedOn { get; set; }
    }
}
