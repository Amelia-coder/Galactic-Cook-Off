using Scripts.Game.RecipeSystem.Ingredients;
using System;
using System.Collections.Generic;


namespace Scripts.Game.RecipeSystem.Recipes
{
    
    public static class RecipeRegistry
    {
        // =========================================================
        // Global recipe storage
        // Key = recipe ID
        // =========================================================
        private static readonly Dictionary<string, Recipe> _recipes = new();

        // =========================================================
        // Static initialization
        // Automatically runs once on first access
        // =========================================================
        static RecipeRegistry()
        {
            Register(new Recipe(
                id: "burger",
                cookTime: 5f,
                resultId: "burger_item",
                ingredients: new()
                {
            new RecipeIngredient("bread", 1),
            new RecipeIngredient("meat", 1),
            new RecipeIngredient("cheese", 1)
                }
            ));

            Register(new Recipe(
                id: "pizza",
                cookTime: 8f,
                resultId: "pizza_item",
                ingredients: new()
                {
            new RecipeIngredient("dough", 1),
            new RecipeIngredient("cheese", 1),
            new RecipeIngredient("tomato", 1)
                }
            ));

            Register(new Recipe(
                id: "toast",
                cookTime: 2f,
                resultId: "toast_item",
                ingredients: new()
                {
            new RecipeIngredient("bread", 1)
                }
            ));

            Register(new Recipe(
                id: "mega_dough",
                cookTime: 8f,
                resultId: "mega_dough_item",
                ingredients: new()
                {
            new RecipeIngredient("dough", 2)
                }
            ));
        }

        // =========================================================
        // Register recipe
        // =========================================================
        public static void Register(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            if (string.IsNullOrWhiteSpace(recipe.Id))
            {
                throw new ArgumentException(
                    "Recipe ID cannot be null or empty.");
            }

            if (_recipes.ContainsKey(recipe.Id))
            {
                throw new InvalidOperationException(
                    $"Recipe '{recipe.Id}' is already registered.");
            }

            _recipes.Add(recipe.Id, recipe);
        }

        // =========================================================
        // Get recipe by ID
        // Throws if not found
        // =========================================================
        public static Recipe Get(string id)
        {
            if (!_recipes.TryGetValue(id, out var recipe))
            {
                throw new KeyNotFoundException(
                    $"Recipe '{id}' is not registered.");
            }

            return recipe;
        }

        // =========================================================
        // Safe lookup
        // =========================================================
        public static bool TryGet(string id, out Recipe recipe)
        {
            return _recipes.TryGetValue(id, out recipe);
        }

        // =========================================================
        // Returns all recipes
        // =========================================================
        public static IReadOnlyCollection<Recipe> GetAll()
        {
            return _recipes.Values;
        }

        // =========================================================
        // Checks if recipe exists
        // =========================================================
        public static bool Contains(string id)
        {
            return _recipes.ContainsKey(id);
        }
    }
}