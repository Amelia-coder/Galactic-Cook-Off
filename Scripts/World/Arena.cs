using Godot;
using System.Collections.Generic;
using Scripts.Player;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.World.WorldObjects;


namespace Scripts.World
{
	public partial class Arena : Node3D
	{

		[Export] public PackedScene PlayerScene;
		[Export] public Node PlayersContainer;

		[Export] public PackedScene MeleeEnemyScene;
		[Export] public Node EnemiesContainer;
		[Export] public PackedScene BossScene;

		[Export] public Node3D[] SpawnPoints;

		private RespawnManager _respawnManager;
		private int _dishesCooked = 0;


		public override void _Ready()
		{
			_respawnManager = GetNode<RespawnManager>("RespawnManager");
			_respawnManager.GameOver += OnGameOver;
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

			var player = PlayerScene.Instantiate<Player.Player>();
			player.Name = id.ToString();
			player.PlayerId = (int)id;
			PlayersContainer.AddChild(player, true);
			player.Position = new Vector3(0, 0, id);


			// Wire signal → RespawnManager (Player doesn't know who listens)
			player.PlayerDied += _respawnManager.OnPlayerDied;

			// Register through interface
			_respawnManager.RegisterPlayer(player);

		}

		private void DelPlayer(long id)
		{
			var node = PlayersContainer.GetNodeOrNull<Player.Player>(id.ToString());
			if (node == null) return;

			// Unwire signal
			node.PlayerDied -= _respawnManager.OnPlayerDied;
			_respawnManager.UnregisterPlayer((int)id);

			var input = node.GetNodeOrNull<MultiplayerSynchronizer>("PlayerInput");
			if (input != null) input.PublicVisibility = false;
			var sync = node.GetNodeOrNull<MultiplayerSynchronizer>("ServerSync");
			if (sync != null) sync.PublicVisibility = false;

			node.QueueFree();
		}

		private void OnGameOver()
		{
			GD.Print("[Arena] GAME OVER");
		}


		private void SpawnEnemy(Vector3 position)
		{
			var enemy = MeleeEnemyScene.Instantiate<Node3D>();
			enemy.Position = position;
			EnemiesContainer.AddChild(enemy, true);
		}


		private Player.Player GetPlayerNode(int playerId)
		{
			return PlayersContainer.GetNodeOrNull<Player.Player>(playerId.ToString());
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
}
