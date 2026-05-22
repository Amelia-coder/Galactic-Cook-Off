using Godot;
using Scripts.Networking;
using Scripts.Networking.LANComponents;
using Scripts.UI;
using System;
using System.Collections.Generic;

// AppRoot.cs — simplified
public partial class AppRoot : Node
{
	private const int GamePort = 65000;

	[Export] public Menu MenuScene;
	[Export] public PackedScene ArenaScene;

	[Export] public NetworkManager NetworkManager;
	[Export] public LanDiscovery LanDiscovery;
	[Export] public LobbyUI LobbyUI;
	// In editor: add a Node called "Level" as child of AppRoot
	[Export] public Node LevelContainer;

	// In editor: add a MultiplayerSpawner, set SpawnPath = LevelContainer,
	// and add ArenaScene to Auto Spawn List

	private bool _isHost;


	public override void _Ready()
	{
		MenuScene.HostRequested += OnHostRequested;
		MenuScene.JoinRequested += OnJoinRequested;
		MenuScene.ExitRequested += () => GetTree().Quit();

		// Lobby signals
		LobbyUI.StartPressed += OnStartPressed;
		LobbyUI.LeavePressed += OnLeavePressed;

		// Network signals
		NetworkManager.Connected += OnClientConnected;
		NetworkManager.Disconnected += OnDisconnected;
		NetworkManager.PlayerJoined += OnPlayerJoined;
		NetworkManager.PlayerLeft += OnPlayerLeft;

		LanDiscovery.ServersUpdated += OnServersUpdated;
		
		ShowMenu();
	}

	private void OnServersUpdated(List<ServerInfo> servers)
	{
		if (MenuScene.Visible)
			MenuScene.ServerList.UpdateList(servers);
	}

	// --------------------------------------------------
	// UI state transitions
	// --------------------------------------------------
	private void ShowMenu()
	{
		MenuScene.Show();
		LobbyUI.Hide();
		LanDiscovery.StartClientDiscovery();
	}

	private void ShowLobby()
	{
		MenuScene.Hide();
		LobbyUI.Show();

		string ip = GetLocalIp();
		LobbyUI.Setup(_isHost, ip, "Cooking Chaos");

		// Add self
		LobbyUI.AddPlayer(NetworkManager.MyId, _isHost);
	}

	private void HideAll()
	{
		MenuScene.Hide();
		LobbyUI.Hide();
	}

	// --------------------------------------------------
	// Host flow
	// --------------------------------------------------
	private void OnHostRequested()
	{
		var err = NetworkManager.Host(GamePort);
		if (err != Error.Ok) return;

		_isHost = true;
		LanDiscovery.StopClientDiscovery();
		LanDiscovery.StartHostBroadcast("Cooking Chaos", GamePort);

		ShowLobby();
	}

	private void OnStartPressed()
	{
		if (!_isHost) return;

		// Tell all peers to load the arena
		Rpc(MethodName.LoadArenaRpc);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void LoadArenaRpc()
	{
		HideAll();
		LanDiscovery.StopClientDiscovery();
		LanDiscovery.StopHostBroadcast();

		// Only server creates the level — spawner replicates
		if (Multiplayer.IsServer())
			ChangeLevel(ArenaScene);
	}

	// --------------------------------------------------
	// Client flow
	// --------------------------------------------------
	private void OnJoinRequested(string ip)
	{
		var err = NetworkManager.Join(ip, GamePort);
		if (err != Error.Ok) return;

		_isHost = false;
		// Wait for Connected signal before showing lobby
	}

	private void OnClientConnected()
	{
		GD.Print("[AppRoot] Client connected to server");
		LanDiscovery.StopClientDiscovery();
		ShowLobby();
		RpcId(1, MethodName.RequestPlayerListRpc);
	}

	private void OnPlayerJoined(long id)
	{
		GD.Print($"[AppRoot] Player joined: {id}");
		LobbyUI.AddPlayer(id, false);
	}

	private void OnPlayerLeft(long id)
	{
		GD.Print($"[AppRoot] Player left: {id}");
		LobbyUI.RemovePlayer(id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestPlayerListRpc()
	{
		if (!Multiplayer.IsServer()) return;

		int senderId = Multiplayer.GetRemoteSenderId();

		// Send host
		RpcId(senderId, MethodName.AddPlayerToLobbyRpc, 1, true);

		// Send all other connected peers
		foreach (int peerId in Multiplayer.GetPeers())
		{
			if (peerId == senderId) continue;
			RpcId(senderId, MethodName.AddPlayerToLobbyRpc, peerId, false);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void AddPlayerToLobbyRpc(long id, bool isHost)
	{
		LobbyUI.AddPlayer(id, isHost);
	}

	// --------------------------------------------------
	// Disconnect / Leave
	// --------------------------------------------------
	private void OnLeavePressed()
	{
		NetworkManager.Disconnect();
		LanDiscovery.StopHostBroadcast();
		_isHost = false;
		ShowMenu();
	}

	private void OnDisconnected()
	{
		LanDiscovery.StopHostBroadcast();
		_isHost = false;

		foreach (Node c in LevelContainer.GetChildren())
			c.QueueFree();

		ShowMenu();
	}

	// --------------------------------------------------
	// Level management
	// --------------------------------------------------
	private void ChangeLevel(PackedScene scene)
	{
		foreach (Node c in LevelContainer.GetChildren())
		{
			LevelContainer.RemoveChild(c);
			c.QueueFree();
		}
		LevelContainer.AddChild(scene.Instantiate());
	}

	// --------------------------------------------------
	// Helpers
	// --------------------------------------------------
	private string GetLocalIp()
	{
		foreach (var ip in IP.GetLocalAddresses())
		{
			if (ip.Contains('.') && !ip.StartsWith("127."))
				return ip;
		}
		return "unknown";
	}
}
