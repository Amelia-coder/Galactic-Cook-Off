using Godot;
using System;
using System.Collections.Generic;
using Scripts.Networking.LANComponents;

namespace Scripts.Networking
{
	public partial class LanDiscovery : Node
	{
		[Export] public LanListener Listener;

		[Export] public LanBroadcaster Broadcaster;

		public event Action<List<ServerInfo>> ServersUpdated;

		private Dictionary<string, ServerInfo> _servers =
			new();

		public override void _Ready()
		{
			Listener.ServerDiscovered +=
				OnServerDiscovered;
		}

		public void StartClientDiscovery()
		{
			GD.Print("[LAN] Starting client discovery");

			_servers.Clear();

			Listener.StartListening();
		}

		public void StopClientDiscovery()
		{
			GD.Print("[LAN] Stopping client discovery");

			Listener.StopListening();

			_servers.Clear();
		}

		public void StartHostBroadcast(string serverName, int gamePort)
		{
			GD.Print("[LAN] Starting host broadcast");

			Broadcaster.Init(serverName, gamePort);

			Broadcaster.StartBroadcasting();
		}

		public void StopHostBroadcast()
		{
			GD.Print("[LAN] Stopping host broadcast");

			Broadcaster.StopBroadcasting();
		}

	   private void OnServerDiscovered(
			ServerInfo info)
		{
			string key =
				$"{info.Ip}:{info.Port}";

			bool isNew =
				!_servers.ContainsKey(key);

			_servers[key] = info;

			if (isNew)
			{
				GD.Print(
					$"[LAN] Registered server: {info.Name}");
			}

			//ServersUpdated?.Invoke(
			//    _servers.Values.ToList());
		}
	}
}
