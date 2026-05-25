using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.World
{
	
	public partial class RespawnManager : Node
	{
		[Export] public Node3D[] SpawnPoints;
		[Export] public float RespawnDelaySec = 3.0f;
		[Export] public int MaxSoloRespawns = 2;

		private readonly Dictionary<int, Player.IPlayerLifecycle> _players = new();
		private readonly HashSet<int> _alivePlayers = new();
		private readonly Dictionary<int, int> _soloRespawnsLeft = new();
		private readonly HashSet<int> _permanentlyDead = new();

		/// <summary>
		/// Fired when every player is permanently dead.
		/// Arena (or whatever level manager) subscribes to this.
		/// </summary>
		public event Action GameOver;


		public void RegisterPlayer(Player.IPlayerLifecycle player)
		{
			int id = player.PlayerId;

			if (_permanentlyDead.Contains(id))
			{
				GD.Print($"[RespawnManager] Player {id} is permanently dead. Spectator only.");
				_players[id] = player;
				player.EnterSpectator();
				return;
			}

			_players[id] = player;
			_alivePlayers.Add(id);

			if (!_soloRespawnsLeft.ContainsKey(id))
				_soloRespawnsLeft[id] = MaxSoloRespawns;

			GD.Print($"[RespawnManager] Registered player {id}. Alive: {_alivePlayers.Count}");
		}

		public void UnregisterPlayer(int playerId)
		{
			_players.Remove(playerId);
			_alivePlayers.Remove(playerId);
			// Keep _soloRespawnsLeft and _permanentlyDead across reconnects
		}

		// =========================================================
		// Death handling (Player signals this — doesn't care who listens)
		// =========================================================

		public void OnPlayerDied(int playerId)
		{
			if (!Multiplayer.IsServer()) return;

			_alivePlayers.Remove(playerId);
			int aliveCount = _alivePlayers.Count;

			GD.Print($"[RespawnManager] Player {playerId} died. Alive: {aliveCount}");

			if (aliveCount >= 1)
			{
				BeginRespawn(playerId);
			}
			else
			{
				if (_soloRespawnsLeft.TryGetValue(playerId, out int remaining) && remaining > 0)
				{
					_soloRespawnsLeft[playerId] = remaining - 1;
					GD.Print($"[RespawnManager] Solo respawn. Budget left: {remaining - 1}");
					BeginRespawn(playerId);
				}
				else
				{
					_permanentlyDead.Add(playerId);
					_players[playerId]?.EnterSpectator();
					CheckGameOver();
				}
			}
		}

		private async void BeginRespawn(int playerId)
		{
			if (!_players.TryGetValue(playerId, out var player)) return;

			player.Disable();

			await ToSignal(GetTree().CreateTimer(RespawnDelaySec), SceneTreeTimer.SignalName.Timeout);

			// Player might have disconnected during wait
			if (!_players.ContainsKey(playerId)) return;

			Vector3 pos = GetSpawnPosition();
			_players[playerId].Respawn(pos);
			_alivePlayers.Add(playerId);

			GD.Print($"[RespawnManager] Player {playerId} respawned. Alive: {_alivePlayers.Count}");
		}

		private void CheckGameOver()
		{
			if (_alivePlayers.Count > 0) return;

			bool anyCanRespawn = _soloRespawnsLeft.Values.Any(v => v > 0);
			if (!anyCanRespawn)
			{
				GD.Print("[RespawnManager] ===== GAME OVER =====");
				GameOver?.Invoke();
			}
		}

		// =========================================================
		// Public queries (for spectator camera — no Arena dependency)
		// =========================================================

		public List<Player.IPlayerLifecycle> GetAlivePlayers()
		{
			var result = new List<Player.IPlayerLifecycle>();
			foreach (int id in _alivePlayers)
			{
				if (_players.TryGetValue(id, out var p))
					result.Add(p);
			}
			return result;
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
