using Scripts.Player.Components;

namespace Scripts.Game
{
    public interface IEntity
    {
        T GetComponent<T>() where T : Component;

        void RegisterComponent(Component component);
    }
}