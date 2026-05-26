using Godot;
using System;

namespace Scripts.UI
{
	public partial class Menu : Control
	{
		public event Action HostRequested;
		public event Action<string> JoinRequested;
		public event Action ExitRequested;
		private LineEdit _ipInput;
		[Export] public ServerListUI ServerList;

		public override void _Ready()
		{
			_ipInput = GetNode<LineEdit>("IPLineEdit");
			ServerList.ServerSelected += ip => JoinRequested?.Invoke(ip);

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

			GD.Print($"[Menu] Join requested: {ip}");
			JoinRequested?.Invoke(ip);
		}

		private void OnExitButtonPressed()
		{
			GD.Print("[Menu] Exit requested");
			ExitRequested?.Invoke();
		}
	}
}
