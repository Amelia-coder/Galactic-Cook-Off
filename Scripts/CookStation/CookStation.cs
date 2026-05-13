using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

using Scripts.Player.Components;

namespace Scripts.Game
{
	public partial class CookStation : StaticBody3D, IInteractable, IItemReceiver
	{
		// =========================================================
		// Signals — for game state transitions
		// =========================================================
		[Signal] public delegate void GoodsChangedEventHandler(int lol, int required);
		[Signal] public delegate void WaveStartedEventHandler(int wave);
		[Signal] public delegate void GoalChangedEventHandler(int newGoal);
		[Signal] public delegate void MinigameStartedEventHandler(int wave);
		[Signal] public delegate void MinigameFailedEventHandler();
		[Signal] public delegate void StationDestroyedEventHandler();

		// IInteractable — zone awareness events (used externally if needed)
		public event Action<IEntity> PlayerEnteredInteractionZone;
		public event Action<IEntity> PlayerExitedInteractionZone;

		[Export] public int RequiredGoods = 10;

		private int _storedGoods = 0;
		private IEntity _player;

		// =========================================================
		// Lifecycle
		// =========================================================
		public override void _Ready()
		{
			var area = GetNode<Area3D>("InteractionArea");
			area.BodyEntered += OnBodyEntered;
			area.BodyExited += OnBodyExited;
		}

		// =========================================================
		// Zone detection
		// Registers this station as the active interactable on the
		// player's InteractionComponent so it can handle input itself.
		// =========================================================
		private void OnBodyEntered(Node3D body)
		{
			if (!body.IsInGroup("Player") || body is not IEntity entity)
				return;

			_player = entity;

			// Wire up — PlayerInteractionComponent will now route
			// pickup input to this station's TryInsert()
			entity.GetComponent<PlayerInteractionComponent>()
				  ?.SetCurrentInteractable(this);

			PlayerEnteredInteractionZone?.Invoke(entity);
		}

		private void OnBodyExited(Node3D body)
		{
			if (body != _player as Node3D)
				return;

			_player.GetComponent<PlayerInteractionComponent>()
				   ?.ClearCurrentInteractable(this);
			PlayerExitedInteractionZone?.Invoke(_player);
			_player = null;
		}

		// =========================================================
		// IItemReceiver
		// Called by PlayerInteractionComponent when player presses
		// the interact button while holding an ingredient.
		// =========================================================
		public bool TryInsert(IIngredient ingredient, IEntity actor)
		{
			_storedGoods++;

			// Broadcast current progress — UI, game manager etc. listen here
			EmitSignal(SignalName.GoodsChanged, _storedGoods, RequiredGoods);
			GD.Print($"[CookStation] Goods: {_storedGoods}/{RequiredGoods}");

			if (_storedGoods >= RequiredGoods)
				StartMinigame();

			return true; // true = accepted, PlayerInteractionComponent destroys the item
		}

		// IInteractable fallback (not used for deposit flow)
		public void Interact(IEntity actor) { }

		// =========================================================
		// Minigame + state transitions
		// =========================================================
		private async void StartMinigame()
		{
			GD.Print("[CookStation] Minigame started");
			EmitSignal(SignalName.MinigameStarted, 0);

			bool success = await RunMinigame();

			if (success)
			{
				GD.Print("[CookStation] Minigame completed");
				_storedGoods = 0;
				EmitSignal(SignalName.GoodsChanged, _storedGoods, RequiredGoods);
				// Emit wave/goal signals here as the game grows:
				// EmitSignal(SignalName.WaveStarted, nextWave);
			}
			else
			{
				GD.Print("[CookStation] Minigame failed");
				EmitSignal(SignalName.MinigameFailed);
			}
		}

		// Swap this out for a real minigame later — returns success/fail
		private async Task<bool> RunMinigame()
		{
			await ToSignal(GetTree().CreateTimer(3.0), "timeout");
			return true;
		}
	}
}
