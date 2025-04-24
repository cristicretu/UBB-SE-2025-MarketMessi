namespace Marketplace_SE.Services
{
    public class UserService : IUserService
    {
        private const string ValidUserId = "12345";

        public string RetrieveUserId()
        {
            // Return the hardcoded user ID
            return ValidUserId;
        }

        public bool ValidateUserId(string enteredId)
        {
            // Compare the entered ID with the valid user ID
            return enteredId == ValidUserId;
        }
    }
}
