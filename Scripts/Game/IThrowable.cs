using Godot;
using System;
public interface IThrowable
{
    event Action<bool> PickupAvailabilityChanged;
    bool CanBePickedUpBy(IEntity actor);
    void PickUp(IEntity actor);
    void Throw(Vector3 impulse);
    void Drop();
}
