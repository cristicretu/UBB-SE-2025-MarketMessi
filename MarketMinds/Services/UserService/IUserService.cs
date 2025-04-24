namespace Marketplace_SE.Services
{
    public interface IUserService
    {
        string RetrieveUserId();
        bool ValidateUserId(string enteredId);
    }
}
