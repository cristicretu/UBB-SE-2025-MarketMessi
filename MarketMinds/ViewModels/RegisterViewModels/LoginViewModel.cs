using DomainLayer.Domain;
using MarketMinds.Services.LoginService;

namespace ViewModelLayer.ViewModel
{
	public class LoginViewModel
	{
		private readonly LoginService loginService;

		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool LoginStatus { get; private set; }

		public User LoggedInUser { get; private set; }

		public LoginViewModel()
		{
			loginService = new LoginService();
		}

		public void AttemptLogin(string username, string password)
		{
			Username = username;
			Password = password;

			LoggedInUser = loginService.GetUserByCredentials(Username, Password);

			LoginStatus = LoggedInUser != null;
		}
	}
}