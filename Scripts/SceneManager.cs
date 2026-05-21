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
			=> LoadSceneAsync(scene.ResourcePath);

		public void LoadSceneAsync(string path)
		{
			if (_loading)
			{
				GD.PrintErr("[SceneManager] Already loading, ignoring request");
				return;
			}

			GD.Print($"[SceneManager] Loading: {path}");
			_loading = true;
			_path = path;
			ResourceLoader.LoadThreadedRequest(path);
		}

		public override void _Process(double delta)
		{
			if (!_loading) return;

			var progressArray = new Godot.Collections.Array();
			var status = ResourceLoader.LoadThreadedGetStatus(_path, progressArray);
			float progress = progressArray.Count > 0 ? (float)progressArray[0] : 0f;
			ProgressChanged?.Invoke(progress);

			if (status == ResourceLoader.ThreadLoadStatus.InProgress)
				return;

			_loading = false;

			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				var resource = ResourceLoader.LoadThreadedGet(_path);
				if (resource is PackedScene scene)
				{
					var instance = scene.Instantiate();
					SwitchScene(instance, () => SceneLoaded?.Invoke(instance));
				}
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Failed)
			{
				LoadFailed?.Invoke(_path);
			}
		}

		private void SwitchScene(Node instance, Action onAdded)
		{
			foreach (Node child in SceneContainer.GetChildren())
				child.QueueFree();

			SceneContainer.CallDeferred(Node.MethodName.AddChild, instance);
			Callable.From(onAdded).CallDeferred();
		}
	}
}
