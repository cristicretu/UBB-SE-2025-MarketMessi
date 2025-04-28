using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DomainLayer.Domain;
using MarketMinds.Services.UserService;

namespace MarketMinds.ViewModels
{
	public class RegisterViewModel
	{
		private readonly IUserService userService;

		public RegisterViewModel()
		{
			userService = App.UserService;
		}
		public RegisterViewModel(IUserService userService)
		{
			this.userService = userService;
		}

		public async Task<bool> IsUsernameTaken(string username)
		{
			try
			{
				return await userService.IsUsernameTakenAsync(username);
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
				return await userService.RegisterUserAsync(user);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating user: {ex.Message}");
				return false;
			}
		}

		public bool IsValidUsername(string username)
		{
			return Regex.IsMatch(username, "^[A-Za-z0-9_]{5,20}$");
		}

		public bool IsValidPassword(string password)
		{
			return Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,15}$");
		}

		public string GetPasswordStrength(string password)
		{
			if (password.Length < 6)
			{
				return "Weak";
			}

			if (Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&]).{6,15}$"))
			{
				return "Strong";
			}

			if (Regex.IsMatch(password, "^(?=.*[A-Z])|(?=.*\\d)|(?=.*[@$!%*?&]).{6,15}$"))
			{
				return "Medium";
			}

			return "Weak";
		}

		public bool PasswordsMatch(string password, string confirmPassword)
		{
			return password == confirmPassword;
		}
	}
}