namespace UserManagementApi.DTOs
{
    public class UpdatePasswordDto
    {
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
