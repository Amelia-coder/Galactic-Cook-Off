using Godot;

namespace Scripts.Networking
{

	//public partial class PlayerSpawner : MultiplayerSpawner
	//{
	//	[Export] public PackedScene PlayerScene;
	//	[Export] public Node PlayersContainer;

	//	public override void _Ready()
	//	{
	//		SpawnPath = PlayersContainer.GetPath();

	//		AddSpawnableScene(PlayerScene.ResourcePath);

	//		if (!Multiplayer.IsServer())
	//			return;

	//		Multiplayer.PeerConnected += OnPeerConnected;
	//		Multiplayer.PeerDisconnected += OnPeerDisconnected;

	//		// Spawn already connected peers (important when scene reloads)
	//		foreach (var id in Multiplayer.GetPeers())
	//		{
	//			SpawnPlayer(id);
	//		}

	//		// Spawn host player
	//		SpawnPlayer(Multiplayer.GetUniqueId());
	//	}

	//	private void OnPeerConnected(long id)
	//	{
	//		SpawnPlayer(id);
	//	}

	//	private void OnPeerDisconnected(long id)
	//	{
	//		var node = PlayersContainer.GetNodeOrNull<Node>(id.ToString());
	//		node?.QueueFree();
	//	}

	//	private void SpawnPlayer(long id)
	//	{
	//		var player = PlayerScene.Instantiate<Node3D>();

	//		player.Name = id.ToString();

	//		player.SetMultiplayerAuthority((int)id);

	//		PlayersContainer.AddChild(player, true);

	//		GD.Print($"[Spawner] Spawned player {id}");
	//	}
	//}
}
