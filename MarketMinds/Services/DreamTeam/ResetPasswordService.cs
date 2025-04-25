using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;

namespace MarketMinds.Services.ResetPasswordService
{
    public class ResetPasswordService
    {
        private readonly UserRepository userRepository;

        public ResetPasswordService()
        {
            this.userRepository = new UserRepository();
        }

        public bool ResetPassword(string email, string newPassword)
        {
            return userRepository.ResetUserPassword(email, newPassword);
        }
    }
}
