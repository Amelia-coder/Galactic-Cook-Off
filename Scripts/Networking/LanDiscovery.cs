using Godot;
using System;
using System.Linq;

using System.Collections.Generic;
using Scripts.Networking.LANComponents;

namespace Scripts.Networking
{
	public partial class LanDiscovery : Node
	{
		[Export] public LanListener Listener;

		[Export] public LanBroadcaster Broadcaster;

		public event Action<List<ServerInfo>> ServersUpdated;

		private Dictionary<string, ServerInfo> _servers = new();
		private Dictionary<string, double> _lastSeen = new();
		private const double ServerTimeout = 3.0; // seconds

		public override void _Ready()
		{
			Listener.ServerDiscovered += OnServerDiscovered;
		}

		public override void _Process(double delta)
		{
			// Remove servers that haven't broadcast recently
            var stale = new List<string>();
            foreach (var kv in _lastSeen)
            {
                _lastSeen[kv.Key] -= delta;
                if (_lastSeen[kv.Key] <= 0)
                    stale.Add(kv.Key);
            }

            if (stale.Count > 0)
            {
                foreach (var key in stale)
                {
                    _servers.Remove(key);
                    _lastSeen.Remove(key);
                }
                ServersUpdated?.Invoke(_servers.Values.ToList());
            }
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

			//_servers.Clear();
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
			_servers.Clear();
		}

	   private void OnServerDiscovered(ServerInfo info)
		{
            string key = $"{info.Ip}:{info.Port}";
            bool isNew = !_servers.ContainsKey(key);
            _servers[key] = info;
            _lastSeen[key] = ServerTimeout;

            if (isNew)
                GD.Print($"[LAN] Registered server: {info.Name}");

            ServersUpdated?.Invoke(_servers.Values.ToList());
        }
	}
}
