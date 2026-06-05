using System;

namespace Scripts.Game
{
    public interface IPickable : IThrowable
    {
        event Action<bool> PickupAvailabilityChanged;
        bool CanBePickedUpBy(IEntity actor);
        void PickUp(IEntity actor);
        void Drop();
        void Consume();
    }
}
