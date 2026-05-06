using Godot;
using Scripts.Game;

namespace Scripts.Enemy
{
	public interface IEnemyEntity : IEntity
	{
		T GetTarget<T>() where T : Node3D;
	}
}
