using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.UserService;

namespace ViewModelLayer.ViewModel
{
	public class LoginViewModel
	{
		private readonly IUserService userService;

		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool LoginStatus { get; private set; }

		public User LoggedInUser { get; private set; }

		public LoginViewModel()
		{
			userService = MarketMinds.App.UserService;
		}
		public LoginViewModel(IUserService userService)
		{
			this.userService = userService;
		}

		public async Task AttemptLogin(string username, string password)
		{
			try
			{
				Username = username;
				Password = password;

				LoggedInUser = await userService.GetUserByCredentialsAsync(Username, Password);
				LoginStatus = LoggedInUser != null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Login error: {ex.Message}");
				LoginStatus = false;
				LoggedInUser = null;
			}
		}
	}
}