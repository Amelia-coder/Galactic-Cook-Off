using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game
{
    public interface IInteractable
    {
        event Action<IEntity> PlayerEnteredInteractionZone;
        event Action<IEntity> PlayerExitedInteractionZone;

        void Interact(IEntity actor) { }  // ← default empty body, no class is forced to override
    }
}
