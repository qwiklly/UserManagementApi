using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required, RegularExpression("^[a-zA-Z0-9]+$")]
        public string Login { get; set; } = default!;

        [Required, RegularExpression("^[a-zA-Z0-9]+$")]
        public string Password { get; set; } = default!;

        [Required, RegularExpression("^[a-zA-Zа-яА-ЯёЁ]+$")]
        public string Name { get; set; } = default!;

        [Range(0, 2)]
        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public bool Admin { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = default!;

        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = default!;

        public DateTime? RevokedOn { get; set; }
        public string? RevokedBy { get; set; }
    }
}
