using Godot;
using Scripts.Networking.LANComponents;
using System;
using System.Collections.Generic;

namespace Scripts.UI
{
	public partial class ServerListUI : VBoxContainer
	{
		public event Action<string> ServerSelected; // emits IP

		private readonly Dictionary<string, Button> _rows = new();

		public void UpdateList(List<ServerInfo> servers)
		{
			// Remove stale
			var keys = new List<string>(_rows.Keys);
			foreach (var key in keys)
			{
				if (!servers.Exists(s => $"{s.Ip}:{s.Port}" == key))
				{
					_rows[key].QueueFree();
					_rows.Remove(key);
				}
			}

			// Add/update
			foreach (var server in servers)
			{
				string key = $"{server.Ip}:{server.Port}";
				if (_rows.ContainsKey(key)) continue;

				var btn = new Button();
				btn.Text = $"{server.Name}  —  {server.Ip}";
				btn.Alignment = HorizontalAlignment.Left;
				string ip = server.Ip;
				btn.Pressed += () => ServerSelected?.Invoke(ip);
				AddChild(btn);
				_rows[key] = btn;
			}
		}

		public void Clear()
		{
			foreach (var btn in _rows.Values)
				btn.QueueFree();
			_rows.Clear();
		}
	}
}
