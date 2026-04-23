using Godot;

/// <summary>
/// Provides camera look direction for aiming, pickup targeting, etc.
/// NOT needed for movement (player rotates with camera)
/// </summary>
public partial class CameraComponent : Component
{
	private Camera3D _camera;

	public void Initialize(Camera3D camera)
	{
		_camera = camera;
	}

	/// <summary>
	/// Gets the 3D forward direction (where camera is looking)
	/// Used for: aiming, pickup cone detection, throw direction
	/// </summary>
	public Vector3 GetForwardDirection()
	{
		return -_camera.GlobalTransform.Basis.Z;
	}
}
