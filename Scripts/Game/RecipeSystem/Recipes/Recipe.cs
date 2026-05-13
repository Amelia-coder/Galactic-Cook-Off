using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Game.RecipeSystem.Ingredients;

namespace Scripts.Game.RecipeSystem.Recipes
{
    public class Recipe
    {
        public string Id { get; init; }

        public List<IngredientData> Ingredients { get; init; }

        public float CookTime { get; init; }

        public string ResultId { get; init; }

        public Recipe(
            string id,
            float cookTime,
            string resultId,
            List<IngredientData> ingredients)
        {
            Id = id;
            CookTime = cookTime;
            ResultId = resultId;
            Ingredients = ingredients;
        }
    }
}
