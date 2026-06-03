using Godot;
using System;
using System.Net;

namespace Scripts.UI
{
	public partial class Menu : Control
	{
		public event Action HostRequested;
		public event Action<string> JoinRequested;
		public event Action ExitRequested;
		private LineEdit _ipInput;
		[Export] public ServerListUI ServerList;
		private Button _joinButton; 

		public override void _Ready()
		{
			_joinButton = GetNode<Button>("CenterContainer/VBoxContainer/VBoxContainer/JoinButton");
			_ipInput = GetNode<LineEdit>("CenterContainer/VBoxContainer/VBoxContainer/IPLineEdit");
			_ipInput.TextChanged += OnIpTextChanged;
			_joinButton.Disabled = true;


			ServerList.ServerSelected += ip => JoinRequested?.Invoke(ip);

		}

		void OnIpTextChanged(string text)
		{
			_joinButton.Disabled = !IsValidIPv4(text);
		}
		private void OnHostButtonPressed()
		{
			GD.Print("[Menu] Host requested");
			HostRequested?.Invoke();
		}

		private void OnJoinButtonPressed()
		{
			GD.Print("join was pressed");
			var ip = _ipInput?.Text;

			if (string.IsNullOrWhiteSpace(ip))
			{
#if DEBUG
				ip = "127.0.0.1";
				GD.Print("[Menu] DEBUG: defaulting to loopback");
#else
				GD.Print("[Menu] No IP entered");
				// Optionally show a warning label to the user here
				return;
#endif
			}
			else
			{
				if (!IsValidIPv4(ip))
				{ 
					return;
				}

			}



			GD.Print($"[Menu] Join requested: {ip}");
			JoinRequested?.Invoke(ip);
		}

		private void OnExitButtonPressed()
		{
			GD.Print("[Menu] Exit requested");
			ExitRequested?.Invoke();
		}

		//helper function for ip validation
		bool IsValidIPv4(string input)
		{
			//GD.Print($"{input}");
			//GD.Print($"is {input} valid ", IPAddress.TryParse(input, out var addr));
			//GD.Print($"is 127.0.0.1 valid ", IPAddress.TryParse("127.0.0.1", out var add));

			if (string.IsNullOrWhiteSpace(input))
				return false;

			return IPAddress.TryParse(input, out var address)  && address.ToString() == input;

		}

	}


}
