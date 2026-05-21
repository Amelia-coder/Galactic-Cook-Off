using Godot;
using System;

using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.Player.Components;
using Scripts.Networking;
using Scripts.Player;

public partial class Arena : Node3D
{

	[Export] public PackedScene PlayerScene;
	[Export] public Node PlayersContainer; // the "Players" node

	[Export] public PackedScene MeleeEnemyScene;
	[Export] public Node EnemiesContainer;

	private int _dishesCooked = 0;

	public override void _Ready()
	{
		if (!Multiplayer.IsServer())
			return;

		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += DelPlayer;

		// Spawn already connected peers
		foreach (int id in Multiplayer.GetPeers())
			AddPlayer(id);

		// Spawn the host's own player
		AddPlayer(Multiplayer.GetUniqueId());
		// CLIENTS do nothing gameplay-related here
		if (!Multiplayer.IsServer())
			return;

		var station = GetNode<CookStation>("CookStation");
		station.DishCooked += OnDishCooked;

		GD.Print("[Arena] Server game logic initialized");

        SpawnEnemy(new Vector3(5, 0, 5));
        SpawnEnemy(new Vector3(-5, 0, 3));
    }

	private void OnDishCooked(Recipe recipe)
	{
		if (!Multiplayer.IsServer())
			return;

		_dishesCooked++;

		GD.Print($"Dish cooked: {recipe.Id} ({_dishesCooked})");

		HandleGameFlow();
	}

	private void HandleGameFlow()
	{
		switch (_dishesCooked)
		{
			case 1:
				GD.Print("Wave 1 starting");
				break;

			case 3:
				GD.Print("Wave 2 starting");
				break;

			case 5:
				SummonBoss();
				break;
		}
	}

	private void SummonBoss()
	{
		GD.Print("Boss spawn trigger");
		// actual spawn handled by EnemySpawner system later
	}


	public override void _ExitTree()
	{
		if (!Multiplayer.IsServer())
			return;

		Multiplayer.PeerConnected -= AddPlayer;
		Multiplayer.PeerDisconnected -= DelPlayer;
	}

	private void AddPlayer(long id)
	{
		GD.Print($"[Arena] PlayerScene null? {PlayerScene == null}");
		GD.Print($"[Arena] PlayersContainer null? {PlayersContainer == null}");

		var player = PlayerScene.Instantiate<Player>();
		player.Name = id.ToString();

		// Set the player ID BEFORE adding to tree.
		// This export is synced as a "Spawn" property by the
		// MultiplayerSynchronizer, so clients get it on spawn.
		player.PlayerId = (int)id;

		PlayersContainer.AddChild(player, true);
		GD.Print($"[Arena] Spawned player {id}");
	}

	private void DelPlayer(long id)
	{
		var node = PlayersContainer.GetNodeOrNull(id.ToString());
        if (node == null) return;

        // Disable syncs before removal to avoid "node not found" errors
        var input = node.GetNodeOrNull<MultiplayerSynchronizer>("PlayerInput");
        if (input != null) input.PublicVisibility = false;

        var sync = node.GetNodeOrNull<MultiplayerSynchronizer>("ServerSync");
        if (sync != null) sync.PublicVisibility = false;
        node?.QueueFree();
	}


    private void SpawnEnemy(Vector3 position)
    {
        var enemy = MeleeEnemyScene.Instantiate<Node3D>();
        enemy.Position = position;
        EnemiesContainer.AddChild(enemy, true);
    }
}
