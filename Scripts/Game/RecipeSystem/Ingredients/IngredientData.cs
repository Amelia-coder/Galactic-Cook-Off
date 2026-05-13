using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game.RecipeSystem.Ingredients
{
    public class IngredientData
    {
        public string Id;
        public string DisplayName;

        public IngredientData(string id, string diaplyName)
        {
            Id = id;
            DisplayName = diaplyName;
        }
    }
}
