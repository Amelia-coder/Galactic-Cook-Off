using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Scripts.Game.RecipeSystem.Dishes;
using Scripts.Game.RecipeSystem.Ingredients;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.Player.Components;
using Scripts.Game;


namespace Scripts.World.WorldObjects
{
	public partial class CookStation : StaticBody3D, IInteractable, IItemReceiver
	{
		//[SignalS] public delegate void DishCookedEventHandler();
		[Signal] public delegate void GoodsChangedEventHandler(int lol, int required);
		[Signal] public delegate void WaveStartedEventHandler(int wave);
		[Signal] public delegate void GoalChangedEventHandler(int newGoal);
		[Signal] public delegate void MinigameStartedEventHandler(int wave);
		[Signal] public delegate void MinigameFailedEventHandler();
		[Signal] public delegate void StationDestroyedEventHandler();
		[Signal] public delegate void CookOptionsChangedEventHandler();
		public event Action<Recipe> DishCooked;

		public event Action<IEntity> PlayerEnteredInteractionZone;
		public event Action<IEntity> PlayerExitedInteractionZone;

		[Export] public int RequiredGoods = 10;
		private int _storedGoods = 0;

		private List<IngredientData> _storedIngredients = new List<IngredientData>();
		private List<Recipe> _availableRecipes = new();
		private readonly Dictionary<string, int> _ingredientCounts = new();

		private bool _hasValidRecipe;
		private IEntity _cookingOwner;
		private IReadOnlyCollection<Recipe> _recipes = RecipeRegistry.GetAll();
		private bool _canCook;
		private bool _isCooking;

		private readonly HashSet<IEntity> _playersInZone = new();

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
		// Zone detection — local per peer (UI only)
		// =========================================================
		private void OnBodyEntered(Node3D body)
		{
			if (!body.IsInGroup("Player") || body is not IEntity entity)
				return;

			_playersInZone.Add(entity);
			entity.GetComponent<PlayerInteractionComponent>()
				  ?.SetCurrentInteractable(this);
			PlayerEnteredInteractionZone?.Invoke(entity);
		}

		private void OnBodyExited(Node3D body)
		{
			if (!body.IsInGroup("Player") || body is not IEntity entity)
				return;

			entity.GetComponent<PlayerInteractionComponent>()
				  ?.ClearCurrentInteractable(this);
			PlayerExitedInteractionZone?.Invoke(entity);
		}

		// =========================================================
		// IItemReceiver — routes to server
		// =========================================================
		public bool TryInsert(IIngredient ingredient, IEntity actor)
		{
			if (ingredient == null)
				return false;

			var data = ingredient.getIngredientIdentData;
			Rpc(MethodName.InsertIngredientRpc, data.Id);
			return true;
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void InsertIngredientRpc(string ingredientId)
		{
			if (!Multiplayer.IsServer()) return;

			var data = IngredientRegistry.Get(ingredientId);
			if (data == null) return;

			_storedIngredients.Add(data);

			if (!_ingredientCounts.TryGetValue(data.Id, out int count))
				count = 0;
			_ingredientCounts[data.Id] = count + 1;

			UpdateCookingState();
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
					return false;
			}
			GD.Print($"Can cook {recipe.Id}");
			return true;
		}

		// =========================================================
		// IInteractable — routes cook request to server
		// =========================================================
		public void Interact(IEntity actor)
		{
			Rpc(MethodName.InteractRpc);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void InteractRpc()
		{
			if (!Multiplayer.IsServer()) return;

			if (_availableRecipes.Count == 0)
				return;

			if (_availableRecipes.Count == 1)
			{
				GD.Print($"Only one recipe available: {_availableRecipes[0].Id}");
				Cook(_availableRecipes[0]);
				return;
			}

			// Multiple recipes — for now cook first available
			// TODO: selection UI
			GD.Print("Multiple recipes available, cooking first:");
			Cook(_availableRecipes[0]);
		}

		private void Cook(Recipe recipe)
		{
			if (recipe == null) return;
			if (!Matches(recipe, _ingredientCounts)) return;

			GD.Print($"Cooking {recipe.Id}...");

			foreach (var req in recipe.Ingredients)
			{
				if (_ingredientCounts.TryGetValue(req.Id, out int count))
				{
					count -= req.Amount;
					if (count <= 0)
						_ingredientCounts.Remove(req.Id);
					else
						_ingredientCounts[req.Id] = count;
				}

				int remainingToRemove = req.Amount;
				for (int i = _storedIngredients.Count - 1;
					 i >= 0 && remainingToRemove > 0;
					 i--)
				{
					if (_storedIngredients[i].Id == req.Id)
					{
						_storedIngredients.RemoveAt(i);
						remainingToRemove--;
					}
				}
			}

			UpdateCookingState();
			GD.Print($"Finished cooking {recipe.Id}");
			DishCooked?.Invoke(recipe);
		}

		// At the end of Cook(), replace the placeholder SpawnResult:
		private void SpawnResult(string resultId)
		{
			var scene = DishRegistry.GetScene(resultId);
			if (scene == null) return;

			var dish = scene.Instantiate<CookedDish>();
			dish.GlobalPosition = GlobalPosition + Vector3.Up * 1.2f;
			GetTree().Root.AddChild(dish, true); // true for multiplayer authority
		}

		// =========================================================
		// Minigame
		// =========================================================
		private async void StartMinigame()
		{
			if (!Multiplayer.IsServer()) return;

			GD.Print("[CookStation] Minigame started");
			EmitSignal(SignalName.MinigameStarted, 0);

			bool success = await RunMinigame();

			if (success)
			{
				_storedGoods = 0;
				EmitSignal(SignalName.GoodsChanged, _storedGoods, RequiredGoods);
			}
			else
			{
				EmitSignal(SignalName.MinigameFailed);
			}
		}

		private async Task<bool> RunMinigame()
		{
			await ToSignal(GetTree().CreateTimer(3.0), "timeout");
			return true;
		}
	}
}
// sgahl be reodne later - bwecise reuqired goods is rendant amd wehave reewcipe system 

//public partial class CookStation : Node3D
//{
//    [Signal] public delegate void DishCookedEventHandler(string recipeId);
//    [Signal] public delegate void InventoryChangedEventHandler();

//    // Ingredient inventory: "meat" -> 2, "bread" -> 1, etc.
//    private readonly Dictionary<string, int> _inventory = new();

//    // Queue of recipe IDs to cook, in order
//    private readonly Queue<string> _recipeQueue = new();

//    private bool _isCooking = false;

//    // ─── Queuing ───

//    public void EnqueueRecipe(string recipeId)
//    {
//        if (!RecipeRegistry.Contains(recipeId))
//        {
//            GD.PrintErr($"[CookStation] Unknown recipe: {recipeId}");
//            return;
//        }
//        _recipeQueue.Enqueue(recipeId);
//        GD.Print($"[CookStation] Queued recipe: {recipeId} (queue size: {_recipeQueue.Count})");
//    }

//    public IReadOnlyCollection<string> GetQueue() => _recipeQueue.ToArray();

//    // ─── Ingredient deposit ───

//    public void DepositIngredient(string ingredientId, int amount = 1)
//    {
//        if (!Multiplayer.IsServer()) return;

//        if (!_inventory.ContainsKey(ingredientId))
//            _inventory[ingredientId] = 0;

//        _inventory[ingredientId] += amount;
//        GD.Print($"[CookStation] Deposited {amount}x {ingredientId} (total: {_inventory[ingredientId]})");
//        EmitSignal(SignalName.InventoryChanged);
//    }

//    public IReadOnlyDictionary<string, int> GetInventory() => _inventory;

//    // ─── Cooking ───

//    public bool CanCookNext()
//    {
//        if (_isCooking || _recipeQueue.Count == 0) return false;
//        var recipe = RecipeRegistry.Get(_recipeQueue.Peek());
//        return HasIngredients(recipe);
//    }

//    public void CookNext()
//    {
//        if (!Multiplayer.IsServer()) return;
//        if (!CanCookNext()) return;

//        _isCooking = true;
//        var recipeId = _recipeQueue.Dequeue();
//        var recipe = RecipeRegistry.Get(recipeId);

//        ConsumeIngredients(recipe);

//        // Later: await minigame using recipe.CookTime
//        GD.Print($"[CookStation] Cooked: {recipeId}");
//        EmitSignal(SignalName.InventoryChanged);
//        EmitSignal(SignalName.DishCooked, recipeId);
//        _isCooking = false;
//    }

//    // ─── Helpers ───

//    private bool HasIngredients(Recipe recipe)
//    {
//        foreach (var req in recipe.Ingredients)
//        {
//            if (!_inventory.TryGetValue(req.Id, out int have) || have < req.Amount)
//                return false;
//        }
//        return true;
//    }

//    private void ConsumeIngredients(Recipe recipe)
//    {
//        foreach (var req in recipe.Ingredients)
//        {
//            _inventory[req.Id] -= req.Amount;
//            if (_inventory[req.Id] <= 0)
//                _inventory.Remove(req.Id);
//        }
//    }
//}
