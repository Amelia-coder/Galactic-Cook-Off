using Godot;

/// <summary>
/// Обобщенная стейт-машина. В нашей реализации, если у нас появлятся машина состояний, 
/// то она будет менеджерить какой-то отдельный компонент(например, выносливость). И для создания овой стетй-маиын достаточно проосто ередать опреденный класс 
/// </summary>
/// <typeparam name="T"> Тип компонента который мы хотим менеджерить</typeparam>
/// 
public partial class StateMachine<T> : Node
{
    public State<T> InitialState { get; set; }

    public State<T> CurrentState;

    public override void _Ready()
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

    }

    private void OnStateFinished(string nextStatePath)
    {
        var nextState = GetNodeOrNull<State<T>>(nextStatePath);
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
