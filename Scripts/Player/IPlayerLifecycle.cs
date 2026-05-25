using Godot;

namespace Scripts.Player
{
    public interface IPlayerLifecycle
    {
        int PlayerId { get; }
        void Disable();
        void Respawn(Vector3 position);
        void EnterSpectator();
    }
}
