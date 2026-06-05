using Godot;
namespace Scripts.Game
{
    public interface IThrowable
    {
        void Throw(Vector3 impulse);
    }
}