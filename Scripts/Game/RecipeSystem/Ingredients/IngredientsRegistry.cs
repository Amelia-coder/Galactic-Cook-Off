using System;
using System.Collections.Generic;

namespace Scripts.Game.RecipeSystem.Ingredients
{
    public static class IngredientRegistry
    {
        private static readonly Dictionary<string, IngredientData> _ingredients = new();

        static IngredientRegistry()
        {
            Register(new IngredientData(
                "meat",
                "Meat"
            ));

            Register(new IngredientData
            (
                "bread",
                "Bread"
            ));

            Register(new IngredientData
            (
                "cheese",
                "Cheese"
            ));

            Register(new IngredientData
            (
                "tomato",
                "Tomato"
            ));

            Register(new IngredientData
            (
                "dough",
                "Dough"
            ));
        }

        public static void Register(IngredientData ingredient)
        {
            if (ingredient == null)
                throw new ArgumentNullException(nameof(ingredient));

            if (string.IsNullOrWhiteSpace(ingredient.Id))
                throw new ArgumentException("Ingredient ID cannot be null or empty.");

            if (_ingredients.ContainsKey(ingredient.Id))
            {
                throw new InvalidOperationException(
                    $"Ingredient with ID '{ingredient.Id}' is already registered.");
            }

            _ingredients.Add(ingredient.Id, ingredient);
        }

        public static IngredientData Get(string id)
        {
            if (!_ingredients.TryGetValue(id, out var ingredient))
            {
                throw new KeyNotFoundException(
                    $"Ingredient '{id}' is not registered.");
            }

            return ingredient;
        }

        public static bool TryGet(string id, out IngredientData ingredient)
        {
            return _ingredients.TryGetValue(id, out ingredient);
        }
        public static IReadOnlyCollection<IngredientData> GetAll()
        {
            return _ingredients.Values;
        }

        public static bool Contains(string id)
        {
            return _ingredients.ContainsKey(id);
        }
    }
}
