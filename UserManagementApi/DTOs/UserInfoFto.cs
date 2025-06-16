using Swashbuckle.AspNetCore.Annotations;

namespace UserManagementApi.DTOs
{
    public class UserInfoDto
    {
        public string Login { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Gender { get; set; }

        [SwaggerSchema(Format = "date")]
        public DateTime? Birthday { get; set; }
        public bool IsActive { get; set; }
    }
}
