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

        public List<RecipeIngredient> Ingredients;

        public float CookTime { get; init; }

        public string ResultId { get; init; }

        public Recipe(
        string id,
        float cookTime,
        string resultId,
        List<RecipeIngredient> ingredients)
        {
            Id = id;
            CookTime = cookTime;
            ResultId = resultId;
            Ingredients = ingredients;
        }
    }
}
