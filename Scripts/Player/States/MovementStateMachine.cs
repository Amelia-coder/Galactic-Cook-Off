using Godot;
using System;

public partial class MovementStateMachine : Node
{
	[Export] public MovementState  InitialState { get; set; }

	public MovementState CurrentState;

	public override void _Ready()
	{
		var entity = GetParent() as IEntity  ?? 
			throw new System.Exception("StateMachine must be child of IEntity");
		StaminaComponent staminaComponent = GetNode<StaminaComponent>("../StaminaComponent");
		GD.Print($"StamionaComponent is null: ", staminaComponent == null);
		foreach (var child in GetChildren())
		{
			if (child is MovementState state)
			{
				state.Finished += OnStateFinished;
				state.Initialize(entity, staminaComponent);
			}
		}

		CurrentState = InitialState ?? GetChild(0) as MovementState;
		CurrentState.Enter();
		
	}

	private void OnStateFinished(string nextStatePath)
	{
		var nextState = GetNodeOrNull<MovementState>(nextStatePath);
		GD.Print($"State finished:", CurrentState.GetType());
		if (nextState == null)
		{
			GD.PrintErr($"State not found: {nextStatePath}");
			GD.Print("Current node:", GetPath());
			GD.Print("Looking for:", nextStatePath);
			return;
		}

		if (!nextState.CanEnter())
			return;

		CurrentState?.Exit();
		CurrentState = nextState;
		CurrentState.Enter();
	}

	public override void _Process(double delta)
	{
		CurrentState?.Update(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		CurrentState?.PhysicsUpdate(delta);
	}
}
