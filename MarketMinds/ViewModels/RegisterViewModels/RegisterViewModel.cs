using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.AuthService;

namespace ViewModelLayer.ViewModel
{
	public class RegisterViewModel
	{
		private readonly IAuthService _authService;

		public RegisterViewModel()
		{
			_authService = MarketMinds.App.AuthService;
		}
		
		public RegisterViewModel(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task<bool> IsUsernameTaken(string username)
		{
			try
			{
				return await _authService.IsUsernameTakenAsync(username);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking username: {ex.Message}");
				return true; // Assume username is taken if an error occurs
			}
		}

		public async Task<bool> CreateNewUser(User user)
		{
			try
			{
				return await _authService.RegisterUserAsync(user);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating user: {ex.Message}");
				return false;
			}
		}
	}
}