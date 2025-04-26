using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.AuthService;

namespace ViewModelLayer.ViewModel
{
	public class LoginViewModel
	{
		private readonly IAuthService _authService;

		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool LoginStatus { get; private set; }

		public User LoggedInUser { get; private set; }

		public LoginViewModel()
		{
			_authService = MarketMinds.App.AuthService;
		}
		
		public LoginViewModel(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task AttemptLogin(string username, string password)
		{
			try 
			{
				Username = username;
				Password = password;

				LoggedInUser = await _authService.GetUserByCredentialsAsync(Username, Password);
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