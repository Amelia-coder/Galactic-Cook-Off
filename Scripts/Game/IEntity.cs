using Godot;
using System.ComponentModel;

public interface IEntity
{
    T GetComponent<T>() where T : Component;

    void RegisterComponent(Component component);
}