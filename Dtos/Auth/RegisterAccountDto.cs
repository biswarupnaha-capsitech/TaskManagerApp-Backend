namespace TaskManager.Dtos.Auth
{
    public class RegisterAccountDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterResponseDto
    {
        public string Message { get; set; } = String.Empty;
    }
}
