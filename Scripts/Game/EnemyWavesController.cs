using Scripts.Game;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scripts.Game
{

	/// <summary>
	/// Owns all enemy spawning. Arena tells it "start wave X" and listens to signals.
	/// Knows nothing about Arena, Player, or game flow.
	///
	/// Scene tree: Arena/EnemyWavesController  (add as child node)
	/// Also needs spawn points as child Node3Ds, or assign via export.
	/// </summary>
	public partial class EnemyWavesController : Node
	{
		private int _spawnCounter = 0;
		/// <summary>
		/// List of wave definitions, in order. Assign in Inspector.
		/// </summary>
		[Export] public WaveDefinition[] Waves;

		/// <summary>
		/// Spawn point markers. Place Node3Ds in the scene and assign here.
		/// </summary>
		[Export] public Node3D[] SpawnPoints;

		/// <summary>
		/// Container node for spawned enemies (keeps scene tree clean).
		/// </summary>
		[Export] public Node EnemiesContainer;

		// ─── Signals for whoever cares (Arena, UI, etc.) ───

		/// <summary>Fired when a wave starts. Arg: wave index.</summary>
		public event Action<int> WaveStarted;

		/// <summary>Fired when all enemies in a wave are dead. Arg: wave index.</summary>
		public event Action<int> WaveCleared;

		/// <summary>Fired when all waves are completed.</summary>
		public event Action AllWavesCleared;

		/// <summary>Fired when any enemy dies. Args: wave index, remaining in wave.</summary>
		public event Action<int, int> EnemyDied;


		private int _currentWaveIndex = -1;
		private readonly List<Node> _aliveEnemies = new();
		private bool _waveInProgress = false;



		/// <summary>
		/// Start a specific wave by index.
		/// </summary>
		public void StartWave(int waveIndex)
		{
			if (!Multiplayer.IsServer()) return;

			if (waveIndex < 0 || waveIndex >= Waves.Length)
			{
				GD.PrintErr($"[WavesController] Invalid wave index: {waveIndex}");
				return;
			}

			if (_waveInProgress)
			{
				GD.PrintErr($"[WavesController] Wave {_currentWaveIndex} still in progress!");
				return;
			}

			_currentWaveIndex = waveIndex;
			RunWaveAsync(Waves[waveIndex]);
		}

		/// <summary>
		/// Start the next wave (after the current one).
		/// Convenience for sequential wave progression.
		/// </summary>
		public void StartNextWave()
		{
			StartWave(_currentWaveIndex + 1);
		}

		/// <summary>
		/// Kill all alive enemies immediately (e.g. for skip/reset).
		/// </summary>
		public void ClearAllEnemies()
		{
			foreach (var enemy in _aliveEnemies.ToArray())
			{
				if (IsInstanceValid(enemy))
					enemy.QueueFree();
			}
			_aliveEnemies.Clear();
		}

		public int GetAliveEnemyCount() => _aliveEnemies.Count;
		public int GetCurrentWaveIndex() => _currentWaveIndex;
		public bool IsWaveInProgress() => _waveInProgress;

		// =========================================================
		// Wave execution
		// =========================================================

		private async void RunWaveAsync(WaveDefinition wave)
		{
			if (!Multiplayer.IsServer()) return;
			_waveInProgress = true;
			_aliveEnemies.Clear();

			GD.Print($"[WavesController] Wave {_currentWaveIndex} starting. " +
					 $"Enemies: {wave.EnemyCount}, Interval: {wave.SpawnInterval}s");

			WaveStarted?.Invoke(_currentWaveIndex);

			// Pre-wave delay (e.g. show "Wave 2 incoming!" UI)
			if (wave.PreWaveDelay > 0)
				await ToSignal(GetTree().CreateTimer(wave.PreWaveDelay), SceneTreeTimer.SignalName.Timeout);

			// Spawn enemies one by one with interval
			for (int i = 0; i < wave.EnemyCount; i++)
			{
				SpawnEnemy(wave);

				// Don't wait after the last spawn
				if (i < wave.EnemyCount - 1 && wave.SpawnInterval > 0)
					await ToSignal(GetTree().CreateTimer(wave.SpawnInterval), SceneTreeTimer.SignalName.Timeout);
			}

			GD.Print($"[WavesController] All {wave.EnemyCount} enemies spawned.");

			// If wave requires clearing, wait for all enemies to die
			if (wave.WaitForClear)
			{
				await WaitForWaveClear();
			}

			_waveInProgress = false;
			GD.Print($"[WavesController] Wave {_currentWaveIndex} cleared!");
			WaveCleared?.Invoke(_currentWaveIndex);

			// Check if that was the last wave
			if (_currentWaveIndex >= Waves.Length - 1)
			{
				GD.Print("[WavesController] All waves completed!");
				AllWavesCleared?.Invoke();
			}
		}

		private async Task WaitForWaveClear()
		{
			// Poll until all enemies are dead
			while (_aliveEnemies.Count > 0)
			{
				// Clean up freed references
				_aliveEnemies.RemoveAll(e => !IsInstanceValid(e));

				if (_aliveEnemies.Count == 0) break;

				await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
			}
		}

		// Spawning
		private void SpawnEnemy(WaveDefinition wave)
		{
			var enemy = wave.EnemyScene.Instantiate<Node3D>();

			_spawnCounter++;
			enemy.Name = $"WaveEnemy_{_currentWaveIndex}_{_spawnCounter}";

			// Debug: check if name already exists
			if (EnemiesContainer.HasNode($"Enemies:/{enemy.Name}"))
			{
				GD.PrintErr($"[WavesController] DUPLICATE NAME: {enemy.Name} already in container!");
				GD.PrintErr($"[WavesController] Existing children:");
				foreach (var child in EnemiesContainer.GetChildren())
					GD.PrintErr($"  - {child.Name}");
				return;
			}


			ApplyWaveModifiers(enemy, wave);
			EnemiesContainer.AddChild(enemy, true);
			enemy.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(1);
			enemy.GlobalPosition = GetRandomSpawnPoint();
			_aliveEnemies.Add(enemy);
			enemy.TreeExited += () => OnEnemyRemoved(enemy);

			GD.Print($"[WavesController] Spawned {enemy.Name}");
		}


		private void ApplyWaveModifiers(Node3D enemy, WaveDefinition wave)
		{
			// If enemy implements IEntity, try to get health component
			if (enemy is IEntity entity)
			{
				var health = entity.GetComponent<GenericHealthComponent>();
				if (health != null && wave.HealthMultiplier != 1.0f)
				{
					health.MaxHealth = health.MaxHealth * wave.HealthMultiplier;
					GD.Print($"[WavesController] Health scaled to {health.MaxHealth}");
				}

				// Future: speed, damage, etc.
				// var movement = entity.GetComponent<MovementComponent>();
				// if (movement != null)
				//     movement.SpeedMultiplier = wave.SpeedMultiplier;
			}
		}

		private void OnEnemyRemoved(Node enemy)
		{
			_aliveEnemies.Remove(enemy);
			GD.Print($"[WavesController] Enemy died. Remaining: {_aliveEnemies.Count}");
			EnemyDied?.Invoke(_currentWaveIndex, _aliveEnemies.Count);
		}

		private Vector3 GetRandomSpawnPoint()
		{
			if (SpawnPoints == null || SpawnPoints.Length == 0)
				return new Vector3(
					GD.Randf() * 10f - 5f,
					0f,
					GD.Randf() * 10f - 5f
				);

			var rng = new RandomNumberGenerator();
			rng.Randomize();
			return SpawnPoints[rng.RandiRange(0, SpawnPoints.Length - 1)].GlobalPosition;
		}
	}
}
