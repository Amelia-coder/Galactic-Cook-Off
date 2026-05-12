using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game
{
    public interface IItemReceiver
    {
        // Return true  → item was accepted, PlayerInteractionComponent will destroy it
        // Return false → item was rejected, player keeps holding it
        bool TryInsert(IIngredient ingredient, IEntity actor);
    }
}