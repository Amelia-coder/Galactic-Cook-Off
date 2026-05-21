using Godot;

namespace Scripts.Game
{
	public partial class StateMachine<T> : Node
	{
		public State<T> InitialState { get; set; }
		public State<T> CurrentState;

		public override void _Ready()
		{
			SetProcess(false);
			SetPhysicsProcess(false);
		}

		public void ManualInitialize()
		{
			if (GetParent() is not T entity)
				throw new System.Exception($"StateMachine must be a child of {typeof(T).Name}");

			foreach (var child in GetChildren())
			{
				if (child is State<T> state)
				{
					state.Finished += OnStateFinished;
					state.Initialize(entity);
				}
			}

			CurrentState = InitialState ?? GetChild(0) as State<T>;
			CurrentState.Enter();
			SetProcess(true);
			SetPhysicsProcess(true);
			GD.Print($"[SM] Started with {CurrentState}");
		}

		private void OnStateFinished(string nextStatePath)
		{
			var next = GetNodeOrNull<State<T>>(nextStatePath);
			if (next == null || !next.CanEnter()) return;
			CurrentState?.Exit();
			CurrentState = next;
			CurrentState.Enter();
		}

		public override void _Process(double delta) => CurrentState?.Update(delta);
		public override void _PhysicsProcess(double delta) => CurrentState?.PhysicsUpdate(delta);
	}
}
