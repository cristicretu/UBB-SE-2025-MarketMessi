using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.RegisterService;

namespace ViewModelLayer.ViewModel
{
	public class RegisterViewModel
	{
		private readonly RegisterService registerService;

		public RegisterViewModel()
		{
			registerService = new RegisterService();
		}

		public async Task<bool> IsUsernameTaken(string username)
		{
			return await registerService.IsUsernameTaken(username);
		}

		public async Task<bool> CreateNewUser(User user)
		{
			try
			{
				return await registerService.RegisterUser(user);
			}
			catch (Exception userCreationException)
			{
				Console.WriteLine($"Error creating user: {userCreationException.Message}");
				return false;
			}
		}
	}
}