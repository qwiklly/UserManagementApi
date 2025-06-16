using Swashbuckle.AspNetCore.Annotations;

namespace UserManagementApi.DTOs
{
    public class UpdateUserDto
    {
        public string Name { get; set; } = default!;
        public int Gender { get; set; }

        [SwaggerSchema(Format = "date")]
        public DateTime? Birthday { get; set; }
    }
}
