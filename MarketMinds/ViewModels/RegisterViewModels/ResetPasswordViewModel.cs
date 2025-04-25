using System.Threading.Tasks;
using MarketMinds.Repositories.UserRepository;
using MarketMinds.Services.ResetPasswordService;

namespace ViewModelLayer.ViewModel
{
    public class ResetPasswordViewModel
    {
        private readonly ResetPasswordService resetPasswordService;

        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;

        public ResetPasswordViewModel()
        {
            resetPasswordService = new ResetPasswordService();
        }

        public bool ResetPassword(string newPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "Passwords don't match!";
                return false;
            }
            return resetPasswordService.ResetPassword(Email, newPassword);
        }
    }
}
