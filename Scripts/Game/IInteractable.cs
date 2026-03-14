using Godot;
public interface IThrowable
{
    bool CanBePickedUpBy(IEntity actor);  // optional, usually the same as "can throw"
    void PickUp(IEntity actor);
    void Throw(Vector3 impulse);
}
