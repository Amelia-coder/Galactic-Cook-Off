using Godot;
using Scripts.Enemy.Bosses.EvilRamsy;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.World.WorldObjects;


namespace Scripts.World
{
	public partial class Arena : Node3D
	{
		[Signal]
		public delegate void VictoryEventHandler();

		[Export] public PackedScene PlayerScene;
		[Export] public Node PlayersContainer;

		[Export] public PackedScene MeleeEnemyScene;
		[Export] public Node EnemiesContainer;
		[Export] public PackedScene BossScene;

		private RespawnManager _respawnManager;
		private EnemyWavesController _wavesController;
		private int _dishesCooked = 0;


		public override void _Ready()
		{
			_respawnManager = GetNode<RespawnManager>("RespawnManager");
			_respawnManager.GameOver += OnGameOver;

			_wavesController = GetNode<EnemyWavesController>("EnemyWavesController");
			_wavesController.WaveStarted += i => GD.Print($"[Arena] Wave {i} started!");
			_wavesController.WaveCleared += OnWaveCleared;
			_wavesController.AllWavesCleared += () => GD.Print("[Arena] All waves done!");

			if (!Multiplayer.IsServer())
				return;

			Multiplayer.PeerConnected += AddPlayer;
			Multiplayer.PeerDisconnected += DelPlayer;


			foreach (int id in Multiplayer.GetPeers())
				AddPlayer(id);

			AddPlayer(Multiplayer.GetUniqueId());
			if (!Multiplayer.IsServer())
				return;

			var station = GetNode<CookStation>("CookStation");
			station.DishCooked += OnDishCooked;

			GD.Print("[Arena] Server game logic initialized");

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
				//	{
				//		GD.Print("Wave 1 starting");
				//		_wavesController.StartWave(0);
				//	}
				//	break;

				//case 3:
				//	{ 
				//		GD.Print("Wave 2 starting");
				//		_wavesController.StartWave(1);
				//	}
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
			var boss = BossScene.Instantiate<EvilRamsy>();
			boss.Position = new Vector3(0, 0.7f, 0.6f);
			EnemiesContainer.AddChild(boss, true);
			boss.BossDefeated += OnBossDefeated;

			GD.Print("Boss spawn trigger");
		}

		
		private void OnBossDefeated()
		{
			GD.Print("Victory!");
			EmitSignal(SignalName.Victory);
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

		private void OnWaveCleared(int waveIndex)
		{
			GD.Print($"[Arena] Wave {waveIndex} cleared!");
		}
	}
}
