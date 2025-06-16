using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace UserManagementApi.DTOs
{
    public class CreateUserDto
    {
        public string Login { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Gender { get; set; }

        [SwaggerSchema(Format = "date")]
        public DateTime? Birthday { get; set; }

        [DefaultValue(false)]
        public bool Admin { get; set; } = false;
    }
}
