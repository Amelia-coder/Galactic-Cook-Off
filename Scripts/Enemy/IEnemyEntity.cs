using Godot;

public interface IEnemyEntity : IEntity
{
    T GetTarget<T>() where T : Node3D;
}
