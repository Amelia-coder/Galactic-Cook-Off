using Godot;
using Scripts.Networking;
using Scripts.UI;
using System;

// AppRoot.cs — simplified
public partial class AppRoot : Node
{
	private const int GamePort = 65000;

	[Export] public Menu MenuScene;
	[Export] public PackedScene ArenaScene;

	[Export] public NetworkManager NetworkManager;
	[Export] public LanDiscovery LanDiscovery;

	// In editor: add a Node called "Level" as child of AppRoot
	[Export] public Node LevelContainer;

	// In editor: add a MultiplayerSpawner, set SpawnPath = LevelContainer,
	// and add ArenaScene to Auto Spawn List

	public override void _Ready()
	{
		MenuScene.HostRequested += StartHost;
		MenuScene.JoinRequested += StartClient;
		MenuScene.ExitRequested += () => GetTree().Quit();

		NetworkManager.Connected += OnClientConnected;
		NetworkManager.Disconnected += OnDisconnected;

		LanDiscovery.StartClientDiscovery();
		MenuScene.Show();
	}

	// --------------------------------------------------
	// Host
	// --------------------------------------------------
	private void StartHost()
	{
		var err = NetworkManager.Host(GamePort);
		if (err != Error.Ok) return;

		LanDiscovery.StopClientDiscovery();
		LanDiscovery.StartHostBroadcast("Cooking Chaos", GamePort);

		MenuScene.Hide();
		ChangeLevel(ArenaScene);
	}

	private void ChangeLevel(PackedScene scene)
	{
		// Clear old level
		foreach (Node c in LevelContainer.GetChildren())
		{
			LevelContainer.RemoveChild(c);
			c.QueueFree();
		}
		// Add new — the spawner detects this and replicates
		LevelContainer.AddChild(scene.Instantiate());
	}

	// --------------------------------------------------
	// Client
	// --------------------------------------------------
	private void StartClient(string ip)
	{
		var err = NetworkManager.Join(ip, GamePort);
		if (err != Error.Ok) return;
		// Do NOT load the arena here.
		// The MultiplayerSpawner will replicate it from the server.
	}

	private void OnClientConnected()
	{
		GD.Print("[AppRoot] Connected — level arriving via spawner");
		LanDiscovery.StopClientDiscovery();
		MenuScene.Hide();
	}

	// --------------------------------------------------
	// Disconnect
	// --------------------------------------------------
	private void OnDisconnected()
	{
		NetworkManager.Disconnect();
		LanDiscovery.StopHostBroadcast();

		foreach (Node c in LevelContainer.GetChildren())
			c.QueueFree();

		LanDiscovery.StartClientDiscovery();
		MenuScene.Show();
		// Show menu again (menu can be a static UI node you show/hide,
		// or you reload the menu scene here)
	}
}
