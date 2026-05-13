using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using Scripts.Player.Components;
using Scripts.Game.RecipeSystem.Ingredients;
using Scripts.Game.RecipeSystem.Recipes;

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
		[Signal] public delegate void CookOptionsChangedEventHandler();

		// IInteractable — zone awareness events (used externally if needed)
		public event Action<IEntity> PlayerEnteredInteractionZone;
		public event Action<IEntity> PlayerExitedInteractionZone;

				
		[Export] public int RequiredGoods = 10;
		private int _storedGoods = 0;

		private List<IngredientData> _storedIngredients = new List<IngredientData>();
		private List<Recipe> _availableRecipes = new();
		private readonly Dictionary<string, int> _ingredientCounts = new();

		private bool _hasValidRecipe;
		private IEntity _cookingOwner; //на будущее - первый, кто провзаимоедйствует, сможет готвоить, остальные - только приносить ингредиенты
		private IReadOnlyCollection<Recipe> _recipes = RecipeRegistry.GetAll();
		private bool _canCook;
		private bool _isCooking;


		private readonly HashSet<IEntity> _playersInZone = new();

		// =========================================================
		// Lifecycle
		// =========================================================
		public override void _Ready()
		{
			//Recipe cheesePizza = new Recipe(2.0f, new List<IngredientData>());


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

			_playersInZone.Add(entity);

			// Wire up — PlayerInteractionComponent will now route
			// pickup input to this station's TryInsert()
			entity.GetComponent<PlayerInteractionComponent>()
				  ?.SetCurrentInteractable(this);

			PlayerEnteredInteractionZone?.Invoke(entity);
		}

		private void OnBodyExited(Node3D body)
		{
			if (!body.IsInGroup("Player") || body is not IEntity entity)
				return;

			var interaction = entity.GetComponent<PlayerInteractionComponent>();

			interaction?.ClearCurrentInteractable(this);

			PlayerExitedInteractionZone?.Invoke(entity);
		}

		// =========================================================
		// IItemReceiver
		// Called by PlayerInteractionComponent when player presses
		// the interact button while holding an ingredient.
		// =========================================================
		public bool TryInsert(IIngredient ingredient, IEntity actor)
		{
			if (ingredient == null)
				return false;

			var data = ingredient.getIngredientIdentData;

			_storedIngredients.Add(data);

			if (!_ingredientCounts.TryGetValue(data.Id, out int count))
				count = 0;

			_ingredientCounts[data.Id] = count + 1;

			UpdateCookingState();

			return true;
		}

		private void UpdateCookingState()
		{
			_availableRecipes = FindMatchingRecipes();

			EmitSignal(
				SignalName.CookOptionsChanged,
				_availableRecipes.Count);

			if (_availableRecipes.Count > 0)
				GD.Print("Cook is now possible!");
		}

		private List<Recipe> FindMatchingRecipes()
		{
			var result = new List<Recipe>();

			foreach (var recipe in _recipes)
			{
				if (Matches(recipe, _ingredientCounts))
					result.Add(recipe);
			}

			return result;
		}
		private bool Matches(Recipe recipe, Dictionary<string, int> storedCounts)
		{
			foreach (var ingredient in recipe.Ingredients)
			{
				storedCounts.TryGetValue(ingredient.Id, out int count);

				if (count < ingredient.Amount)
				{
					GD.Print(
						$"Can't cook {recipe.Id} because missing " +
						$"{ingredient.Id}: need {ingredient.Amount}, have {count}");

					return false;
				}
			}

			GD.Print($"Can cook {recipe.Id}");

			return true;
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
