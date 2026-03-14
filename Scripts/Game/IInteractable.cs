using Godot;
public interface IInteractable
{
    bool CanPickup(IEntity actor);
    void Pickup(IEntity actor);
}

