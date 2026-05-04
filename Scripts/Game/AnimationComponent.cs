using Godot;
using System.Collections.Generic;

//todo: добавить пространства имен для возомжности дублирования названий без страха получить конфликт имен
public partial class AnimationComponent : Component
{
   
	private AnimationTree _animationTree;
	private AnimationPlayer _animationPlayer;

	private readonly Dictionary<EntityAnimation, StringName> _animations = new();

	public void Initialize(
		AnimationTree animationTree,
		AnimationPlayer animationPlayer)
	{
		_animationTree = animationTree; //Q: а нужно ли оно нам на самом деле?
		_animationPlayer = animationPlayer;
	}

	public void RegisterAnimation(
		EntityAnimation animation,
		string actualName)
	{
		_animations[animation] = actualName;
	}

	public void Play(EntityAnimation animation)
	{
		if (!_animations.TryGetValue(animation, out var actual))
			return;

		_animationPlayer.Play(actual);
	}
   

}
