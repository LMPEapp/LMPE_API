namespace LMPE_API.Models
{
    public class AuthModels
    {
        public class LoginRequest { 
            public string Email { get; set; } = ""; 
            public string Password { get; set; } = ""; 
        }
        public class LoginRequestOut
        {
            public string Token { get; set; } = "";
            public User? User { get; set; }
        }
        public class ResetPasswordRequest { 
            public string Email { get; set; } = ""; 
        }
        public class ChangePasswordRequest
        {
            public long? UserId { get; set; }
            public string? OldPassword { get; set; }
            public string NewPassword { get; set; } = "";
        }
    }
}
