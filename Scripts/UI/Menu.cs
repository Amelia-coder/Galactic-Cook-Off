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

        public override void _Ready()
        {
            _ipInput = GetNode<LineEdit>("IPLineEdit");

        }
        private void OnHostButtonPressed()
        {
            GD.Print("[Menu] Host requested");
            HostRequested?.Invoke();
        }

        private void OnJoinButtonPressed()
        {
            var ip = _ipInput?.Text;

            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";

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
