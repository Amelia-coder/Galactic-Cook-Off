using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
	public partial class SceneManager : Node
	{
		[Export] public Node SceneContainer;

		private string _path;
		private bool _loading;

		public event Action<float> ProgressChanged;
		public event Action<Node> SceneLoaded;
		public event Action<string> LoadFailed;

		public void LoadSceneAsync(PackedScene scene)
		{
			LoadSceneAsync(scene.ResourcePath);
		}

		public void LoadSceneAsync(string path)
		{
			if (_loading)
			{
				GD.PrintErr("Scene already loading");
				return;
			}

			_loading = true;
			_path = path;

			ResourceLoader.LoadThreadedRequest(path);
		}

		public override void _Process(double delta)
		{
			if (!_loading)
				return;

			var status =
				ResourceLoader.LoadThreadedGetStatus(_path);

			ProgressChanged?.Invoke(
				GetProgress(status));

			if (status ==
				ResourceLoader.ThreadLoadStatus.InProgress)
			{
				return;
			}

			if (status ==
				ResourceLoader.ThreadLoadStatus.Loaded)
			{
				var resource = ResourceLoader.LoadThreadedGet(_path);

				if (resource is PackedScene scene)
				{
					var instance = scene.Instantiate();

					SwitchScene(instance);

					_loading = false;

					SceneLoaded?.Invoke(instance);
				}
			}
			else if (status ==
				ResourceLoader.ThreadLoadStatus.Failed)
			{
				_loading = false;

				LoadFailed?.Invoke(_path);
			}
		}

		private void SwitchScene(Node instance)
		{
			foreach (Node child in SceneContainer.GetChildren())
				child.QueueFree();

			SceneContainer.AddChild(instance);
		}

		private float GetProgress(
			ResourceLoader.ThreadLoadStatus status)
		{
			return status ==
				ResourceLoader.ThreadLoadStatus.InProgress
				? 0.5f
				: 1f;
		}
	}
}
