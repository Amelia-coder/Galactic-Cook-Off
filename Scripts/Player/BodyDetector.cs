using System;
using System.Collections.Generic;
using Godot;

public partial class BodyDetector : Area3D
{
	public event Action<IThrowable> ThrowableEntered;
	public event Action<IThrowable> ThrowableExited;

	private readonly List<IThrowable> _itemsInRange = new();

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	public IEnumerable<IThrowable> GetNearby() => _itemsInRange;

	public IThrowable GetClosest(Vector3 fromPosition)
	{
		IThrowable closest = null;
		float best = float.MaxValue;
		foreach (var item in _itemsInRange)
		{
			if (item is not Node3D node) continue;
			float d = fromPosition.DistanceSquaredTo(node.GlobalPosition);
			if (d < best) { best = d; closest = item; }
		}
		return closest;
	}

	private void OnAreaEntered(Area3D area)
	{
		GD.Print("Something entered!");
		//safer than GetParent
		if (area.GetOwner() is not IThrowable throwable) return;
		GD.Print("Something throwable entered!");
		_itemsInRange.Add(throwable);
		ThrowableEntered?.Invoke(throwable);
	}

	private void OnAreaExited(Area3D area)
	{
		GD.Print("Something exited");
		if (area.GetOwner() is not IThrowable throwable) return; 
		GD.Print("Something eчшеув!");
		_itemsInRange.Remove(throwable);
		ThrowableExited?.Invoke(throwable);
	}
}
