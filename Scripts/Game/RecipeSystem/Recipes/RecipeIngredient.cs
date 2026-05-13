using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game.RecipeSystem.Recipes
{
    public class RecipeIngredient
    {
        public string Id;
        public int Amount;


        public RecipeIngredient(string id, int amount)
        { 
            Id = id;
            Amount = amount;
        }
    }
}
