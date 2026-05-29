using Godot;
using System;
using System.Collections.Generic;

namespace Scripts.UI
{
	public partial class LobbyUI : Control
	{
		[Export] public Label BroadcastIpLabel;
		[Export] public Label TitleLabel;
		[Export] public Label IpLabel;
		[Export] public Label PlayerCountLabel;
		[Export] public VBoxContainer PlayerList;
		[Export] public Button StartButton;
		[Export] public Button LeaveButton;

		[Export] public int MaxPlayers = 4;
		[Export] public int MinPlayers = 2;

		public event Action StartPressed;
		public event Action LeavePressed;

		private readonly Dictionary<long, HBoxContainer> _playerRows = new();

		public override void _Ready()
		{
			StartButton.Pressed += () => StartPressed?.Invoke();
			LeaveButton.Pressed += () => LeavePressed?.Invoke();
		}

		public void Setup(bool isHost, string localIp, string broadcastIp, string gameName)
		{
			TitleLabel.Text = $"{gameName} — Lobby";
			IpLabel.Text = $"IP: {localIp}";

			if (isHost && broadcastIp != localIp)
			{
				BroadcastIpLabel.Text = $"Join IP: {broadcastIp}";
				BroadcastIpLabel.Visible = true;
			}
			else
			{
				BroadcastIpLabel.Visible = false;
			}


			StartButton.Visible = isHost;
			StartButton.Disabled = true; // enable when 2+ players
			ClearPlayerList();
		}

		public void AddPlayer(long id, bool isHost)
		{
			if (_playerRows.ContainsKey(id)) return;

			var row = new HBoxContainer();

			var dot = new Label();
			dot.Text = "🟢  ";
			row.AddChild(dot);

			var nameLabel = new Label();
			string suffix = isHost ? " (Host)" : "";
			nameLabel.Text = $"Player {_playerRows.Count + 1}{suffix}";
			row.AddChild(nameLabel);

			PlayerList.AddChild(row);
			_playerRows[id] = row;

			UpdatePlayerCount();
		}

		public void RemovePlayer(long id)
		{
			if (!_playerRows.TryGetValue(id, out var row)) return;

			row.QueueFree();
			_playerRows.Remove(id);

			UpdatePlayerCount();
		}

		private void UpdatePlayerCount()
		{
			int count = _playerRows.Count;
			PlayerCountLabel.Text = $"Players ({count}/{MaxPlayers}):";
			StartButton.Disabled = count < MinPlayers;
		}

		private void ClearPlayerList()
		{
			foreach (var row in _playerRows.Values)
				row.QueueFree();
			_playerRows.Clear();
		}
	}
}
