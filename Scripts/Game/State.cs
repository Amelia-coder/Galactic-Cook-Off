using Godot;
using System.Security.Principal;

public abstract partial class State : Node
{


    [Signal] public delegate void FinishedEventHandler(string nextStatePath);

    protected IEntity Entity { get; set; }

    public virtual void Initialize(IEntity entity)
    {
        Entity = entity;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void HandleInput(InputEvent @event) { }
    public virtual void Update(double delta) { }
    public virtual void PhysicsUpdate(double delta) { }
    public virtual bool CanEnter() => true;

    protected void TransitionTo(string nextStatePath)
    {
        EmitSignal(SignalName.Finished, nextStatePath);
    }

}
