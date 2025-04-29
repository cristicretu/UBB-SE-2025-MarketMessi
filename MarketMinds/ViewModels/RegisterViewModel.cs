using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using MarketMinds.Shared.Models;
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
				bool result = await userService.IsUsernameTakenAsync(username);
				return result;
			}
			catch (Exception usernameCheckException)
			{
				return true; // Assume username is taken if an error occurs
			}
		}

		public async Task<bool> CreateNewUser(User user)
		{
			try
			{
				if (user == null)
				{
					return false;
				}

				if (string.IsNullOrEmpty(user.Username))
				{
					return false;
				}

				if (string.IsNullOrEmpty(user.Email))
				{
					return false;
				}

				if (string.IsNullOrEmpty(user.Password))
				{
					return false;
				}

				bool result = await userService.RegisterUserAsync(user);
				return result;
			}
			catch (Exception userCreationException)
			{
				if (userCreationException.InnerException != null)
				{
				}
				return false;
			}
		}

		public bool IsValidUsername(string username)
		{
			bool isValid = Regex.IsMatch(username, "^[A-Za-z0-9_]{5,20}$");
			return isValid;
		}

		public bool IsValidPassword(string password)
		{
			bool isValid = Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,15}$");
			return isValid;
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
			bool match = password == confirmPassword;
			return match;
		}
	}
}