using Godot;
public interface IThrowable
{
    [Signal] public delegate void CanPickUpChangedEventHandler(bool canPickUp);
    bool CanBePickedUpBy(IEntity actor);
    void PickUp(IEntity actor);
    void Throw(Vector3 impulse);
}
