using Godot;
using Scripts.Game;
namespace Scripts.Enemy.Components
{
	public partial class LootDropComponent : Component
	{
		[Export]
		public Godot.Collections.Array<PackedScene> PossibleDrops = [];

		[Export]
		public float DropChance = 0.5f;
		private Node _itemsContainer;



		private IEntity entity;
		///позция дропа может быть настриваемой. но при прочих ранвых, лоично было бы ее сделать по месту положения врага

		public void Initilaize(IEntity parent, Node itemsContainer)
		{ 
			entity = parent;

			_itemsContainer = itemsContainer;
			PackedScene dougScene = GD.Load<PackedScene>("res://Scenes/Dough/Dough.tscn");
			PossibleDrops.Add(dougScene);
		}

		public void Drop()
		{
			var _owner = (Node3D)entity;
			if (!_owner.Multiplayer.IsServer()) return;
			if (PossibleDrops.Count == 0)
				return;

			int index = GD.RandRange(0, PossibleDrops.Count - 1);

			var scene = PossibleDrops[index];
			var item = scene.Instantiate<Node3D>();
			item.Position = _owner.GlobalPosition;
			_itemsContainer.AddChild(item, true);
		}
	}
}
