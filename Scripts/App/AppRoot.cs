using Godot;
using Scripts;
using Scripts.Networking;
using Scripts.Networking.LANComponents;
using Scripts.UI;
using Scripts.World;
using System.Collections.Generic;
public partial class AppRoot : Node
{
	private const int GamePort = 65000;
	private const int DiscoveryPort = 65001;

	[Export] public Menu MenuScene;
	[Export] public PackedScene ArenaScene;
	[Export] public NetworkManager NetworkManager;
	[Export] public LanDiscovery LanDiscovery;
	[Export] public LobbyUI LobbyUI;
	[Export] public VictoryScreen VictoryScreen;
	[Export] public Node LevelContainer;
	[Export] public PackedScene PauseMenuScene;
	[Export] public SceneManager SceneManager;
	
	
	private bool _isHost;
	private PauseMenu _pauseMenu;


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

		_pauseMenu = PauseMenuScene.Instantiate<PauseMenu>();
		AddChild(_pauseMenu);
		_pauseMenu.ExitRequested += OnPauseExit;
		_pauseMenu.Disable();
				

		ShowMenu();
	}
	
	private void OnPauseExit()
	{
		_pauseMenu.Disable();
		NetworkManager.Disconnect();
		LanDiscovery.StopHostBroadcast();
		_isHost = false;

		foreach (Node c in LevelContainer.GetChildren())
			c.QueueFree();

		ShowMenu();
	}

	private void OnServersUpdated(List<ServerInfo> servers)
	{
		if (MenuScene.Visible)
			MenuScene.ServerList.UpdateList(servers);
	}

	// UI state transitions
	private void ShowMenu()
	{
		MenuScene.Show();
		LobbyUI.Hide();
		VictoryScreen.Hide();
		MenuScene.ServerList.Clear();
		LanDiscovery.StartClientDiscovery();
	}

	private void ShowLobby()
	{
		MenuScene.Hide();
		LobbyUI.Show();

		string localIp = GetLocalIp();
		string broadcastIp = LanDiscovery.Broadcaster.GetBroadcastSourceIp();

		LobbyUI.Setup(_isHost, localIp, broadcastIp, "Cooking Chaos");

		// Add self
		LobbyUI.AddPlayer(NetworkManager.MyId, _isHost);
	}

	private void HideAll()
	{
		MenuScene.Hide();
		LobbyUI.Hide();
	}

	// Host flow
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
		_pauseMenu.Enable();

		// Only server creates the level — spawner replicates
		if (Multiplayer.IsServer())
			ChangeLevel(ArenaScene);
	}

	// Client flow
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

	// Disconnect / Leave
	private void OnLeavePressed()
	{
		NetworkManager.Disconnect();
		LanDiscovery.StopHostBroadcast();
		_isHost = false;
		ShowMenu();
	}

	private void OnDisconnected()
	{
		_pauseMenu.Disable();
		LanDiscovery.StopHostBroadcast();
		_isHost = false;

		foreach (Node c in LevelContainer.GetChildren())
			c.QueueFree();

		ShowMenu();
	}

	// Level management
	private void ChangeLevel(PackedScene scene)
	{
		var level = scene.Instantiate();
		SceneManager.SwitchScene(level, () =>
		{
			if (level is Arena arena)
				arena.Victory += OnVictory;
		});
	}

	private void OnVictory()
	{
		Rpc(MethodName.OnVictoryRpc);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true,
		 TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void OnVictoryRpc()
	{
		CallDeferred(MethodName.DeferredVictoryCleanup);
	}

	private void DeferredVictoryCleanup()
	{
		_pauseMenu.Disable();
		SceneManager.SwitchScene();
		ShowVictoryScreen();
	}
	// Helpers
	private string GetLocalIp()
	{
		string fallback = "unknown";

		foreach (var ip in IP.GetLocalAddresses())
		{
			if (!ip.Contains('.') || ip.StartsWith("127.")) continue;

			// Skip common virtual adapter ranges
			if (ip.StartsWith("169.254.")) continue;  // APIPA (no DHCP)
			if (ip.StartsWith("172.17.")) continue;    // Docker
			if (ip.StartsWith("172.18.")) continue;    // Docker

			// Prefer typical LAN ranges
			if (ip.StartsWith("192.168.") || ip.StartsWith("10."))
				return ip;

			fallback = ip;
		}

		return fallback;
	}
	
	private void ShowVictoryScreen()
	{
		VictoryScreen.Show(); 
	}
}
