using Godot;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.Networking;
using Scripts.Player;
using Scripts.Player.Components;
using System;
using System.Collections.Generic;

public partial class Arena : Node3D
{

	[Export] public PackedScene PlayerScene;
	[Export] public Node PlayersContainer;

	[Export] public PackedScene MeleeEnemyScene;
	[Export] public Node EnemiesContainer;
	[Export] public PackedScene BossScene;

	[Export] public Node3D[] SpawnPoints;

	private readonly HashSet<int> _alivePlayers = new();
	private readonly Dictionary<int, int> _soloRespawnsLeft = new();
	private readonly HashSet<int> _permanentlyDead = new();

	private const int MaxSoloRespawns = 2;
	private const float RespawnDelaySec = 3.0f;
	
	private int _dishesCooked = 0;

	public static Arena Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
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
			//case 1:
			//	GD.Print("Wave 1 starting");
			//	break;

			//case 3:
			//	GD.Print("Wave 2 starting");
			//	break;

			case 1:
				{
					GD.Print("Summoned boss!");
					SummonBoss();
				}
			break;
		}
	}

	private void SummonBoss()
	{
		var boss = BossScene.Instantiate<Node3D>();
		boss.Position = new Vector3(0, 0.7f, 0.6f);
		EnemiesContainer.AddChild(boss, true);
		GD.Print("Boss spawn trigger");
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
        int playerId = (int)id;

		// If they were permanently dead before disconnect, don't let them play again
		if (_permanentlyDead.Contains(playerId))
		{
			GD.Print($"[Arena] Player {playerId} reconnected but is permanently dead.");
			// Still spawn the node so they can spectate
			var spectator = PlayerScene.Instantiate<Player>();
			spectator.Name = id.ToString();
			spectator.PlayerId = playerId;
			PlayersContainer.AddChild(spectator, true);
			// Immediately put them into spectator
			spectator.Rpc(Player.MethodName.RpcEnterSpectator);
			return;
		}

		var player = PlayerScene.Instantiate<Player>();
		player.Name = id.ToString();
		player.Position = GetSpawnPosition();
		player.PlayerId = playerId;
		PlayersContainer.AddChild(player, true);

		_alivePlayers.Add(playerId);

		// Only give fresh budget if they've never been tracked
        if (!_soloRespawnsLeft.ContainsKey(playerId))
            _soloRespawnsLeft[playerId] = MaxSoloRespawns;
        // else: keep whatever budget they had before disconnect

        GD.Print($"[Arena] Spawned player {id}. Alive: {_alivePlayers.Count}, " +
                 $"Solo respawns left: {_soloRespawnsLeft[playerId]}");
    }

    private void DelPlayer(long id)
    {
        int playerId = (int)id;
        _alivePlayers.Remove(playerId);
		// DON'T remove from _soloRespawnsLeft or _permanentlyDead
		// so state persists across reconnects

		var node = PlayersContainer.GetNodeOrNull(id.ToString());
		if (node == null) return;

		var input = node.GetNodeOrNull<MultiplayerSynchronizer>("PlayerInput");
		if (input != null) input.PublicVisibility = false;
		var sync = node.GetNodeOrNull<MultiplayerSynchronizer>("ServerSync");
		if (sync != null) sync.PublicVisibility = false;

		node.QueueFree();
	}


	private void SpawnEnemy(Vector3 position)
	{
		var enemy = MeleeEnemyScene.Instantiate<Node3D>();
		enemy.Position = position;
		EnemiesContainer.AddChild(enemy, true);
	}

	public void HandlePlayerDeath(int playerId)
	{
		if (!Multiplayer.IsServer()) return;

		_alivePlayers.Remove(playerId);
		int aliveCount = _alivePlayers.Count;

		if (aliveCount >= 1)
		{
			BeginRespawn(playerId);
		}
		else
		{
			if (_soloRespawnsLeft.TryGetValue(playerId, out int remaining) && remaining > 0)
			{
				_soloRespawnsLeft[playerId] = remaining - 1;
				BeginRespawn(playerId);
			}
			else
			{
				_permanentlyDead.Add(playerId);=
				var player = GetPlayerNode(playerId);
				player?.Rpc(Player.MethodName.RpcEnterSpectator);
				CheckGameOver();
			}
		}
	}

	private async void BeginRespawn(int playerId)
	{
		var player = GetPlayerNode(playerId);
		if (player == null) return;

		// Disable on all clients
		player.Rpc(Player.MethodName.RpcDisablePlayer);

		// Wait for respawn delay
		await ToSignal(GetTree().CreateTimer(RespawnDelaySec), SceneTreeTimer.SignalName.Timeout);

		// Player might have disconnected during the wait
		player = GetPlayerNode(playerId);
		if (player == null) return;

		Vector3 spawnPos = GetSpawnPosition();
		player.Rpc(Player.MethodName.RpcRespawn, spawnPos);
		_alivePlayers.Add(playerId);

		GD.Print($"[Arena] Player {playerId} respawned. Alive: {_alivePlayers.Count}");
	}

	private void CheckGameOver()
	{
		if (_alivePlayers.Count > 0) return;

		bool anyoneCanRespawn = false;
		foreach (var kvp in _soloRespawnsLeft)
		{
			if (kvp.Value > 0) { anyoneCanRespawn = true; break; }
		}

		if (!anyoneCanRespawn)
		{
			GD.Print("[Arena] ===== GAME OVER =====");
			// TODO: show game-over screen, return to lobby, etc.
		}
	}

	public List<Player> GetAlivePlayers()
	{
		var result = new List<Player>();
		foreach (int id in _alivePlayers)
		{
			var p = GetPlayerNode(id);
			if (p != null && IsInstanceValid(p))
				result.Add(p);
		}
		return result;
	}

	private Player GetPlayerNode(int playerId)
	{
		return PlayersContainer.GetNodeOrNull<Player>(playerId.ToString());
	}

	private Vector3 GetSpawnPosition()
	{
		if (SpawnPoints == null || SpawnPoints.Length == 0)
			return new Vector3(0, 2, 0);
		var rng = new RandomNumberGenerator();
		rng.Randomize();
		return SpawnPoints[rng.RandiRange(0, SpawnPoints.Length - 1)].GlobalPosition;
	}
}
