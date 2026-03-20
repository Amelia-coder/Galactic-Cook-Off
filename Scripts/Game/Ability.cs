using Godot;

public abstract partial class Ability : Node
{
    public virtual bool IsActive() { return true; }
    public virtual bool BlocksOtherAbilities() { return false; }

    public virtual void Update(double delta) { }
    //public virtual void PhysicsProcess(double delta) { }
}