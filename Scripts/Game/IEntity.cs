using Godot;
using System.ComponentModel;

public interface IEntity
{
    Vector3 Position { get; }
    T GetComponent<T>() where T : Component;
}