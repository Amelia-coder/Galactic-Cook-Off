using Godot;
using System.Security.Principal;

public abstract partial class State<T> : Node
{


	[Signal] public delegate void FinishedEventHandler(string nextStatePath);

	protected T Entity { get; set; }

	public virtual void Initialize(T entity)
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
