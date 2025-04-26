using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.UserService;

namespace ViewModelLayer.ViewModel
{
	public class RegisterViewModel
	{
		private readonly IUserService _userService;

		public RegisterViewModel()
		{
			_userService = MarketMinds.App.UserService;
		}
		
		public RegisterViewModel(IUserService userService)
		{
			_userService = userService;
		}

		public async Task<bool> IsUsernameTaken(string username)
		{
			try
			{
				return await _userService.IsUsernameTakenAsync(username);
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
				return await _userService.RegisterUserAsync(user);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating user: {ex.Message}");
				return false;
			}
		}
	}
}