using Godot;
using System;

public partial class Dough : RigidBody3D, IThrowable
{
    // =========================================================
    // Exports
    // =========================================================
    [Export] public float Damage = 20f;
    [Export] public float StunDuration = 1.5f;
    [Export] public float DisappearTimeout = 115f;

    // =========================================================
    // IThrowable
    // =========================================================
    // Raised for UI only ("Press F to pick up") — NOT for tracking.
    // BodyDetector on the player owns the nearby-items list.
    public event Action<bool> PickupAvailabilityChanged;
    public bool CanBePickedUpBy(IEntity actor) => actor is Player;

    // =========================================================
    // Private state
    // =========================================================
    private Node _homeScene;   // scene to return to after being dropped/thrown
    private bool _inFlight = false;

    // =========================================================
    // Lifecycle
    // =========================================================
    public override void _Ready()
    {
        _homeScene = GetParent();

        BodyEntered += OnImpact;

        // PickupZone fires PickupAvailabilityChanged for the HUD prompt only.
        // Detection/tracking is handled by BodyDetector on the player side.
        var pickupZone = GetNode<Area3D>("PickupZone");
        pickupZone.BodyEntered += OnPickupZoneBodyEntered;
        pickupZone.BodyExited += OnPickupZoneBodyExited;

        GetTree().CreateTimer(DisappearTimeout).Timeout += QueueFree;
    }

    // =========================================================
    // IThrowable — state transitions
    // =========================================================
    public void PickUp(IEntity actor)
    {
        if (actor is not Node3D actorNode) return;

        _inFlight = false;
        Freeze = true;
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;

        SetPickupZoneActive(false);

        Reparent(actorNode);

        Position = actor is Player carrier && carrier.ThrowPoint != null
            ? actorNode.ToLocal(carrier.ThrowPoint.GlobalPosition)
            : Vector3.Up * 1.5f;
    }

    public void Throw(Vector3 impulse)
    {
        ReturnToScene();
        _inFlight = true;
        Freeze = false;
        ApplyCentralImpulse(impulse);
    }

    public void Drop()
    {
        ReturnToScene();
        _inFlight = false;
        Freeze = false;
    }

    // =========================================================
    // Impact — called by BodyEntered (RigidBody3D signal)
    // =========================================================
    private void OnImpact(Node body)
    {
        if (!_inFlight) return;

        if (body.HasMethod("TakeDamage")) //weird, rather check for fact of the implementation of interface like Dmagebale -r comebont like Healths. Otherwise, coupling
                                          //TODO: instead, send signal of being hit!
            body.Call("TakeDamage", Damage);

        // Stick to static geometry, keep bouncing off dynamic bodies
        if (body is StaticBody3D)
        {
            Freeze = true;
            _inFlight = false;
        }
    }

    // =========================================================
    // PickupZone
    // =========================================================
    private void OnPickupZoneBodyEntered(Node3D body)
    {
        GD.Print("Someone entred!");
        if (body is IEntity actor && CanBePickedUpBy(actor))
            PickupAvailabilityChanged?.Invoke(true);
    }

    private void OnPickupZoneBodyExited(Node3D body)
    {
        if (body is IEntity actor && CanBePickedUpBy(actor))
            PickupAvailabilityChanged?.Invoke(false);
    }

    // =========================================================
    // Helpers
    // =========================================================

    // Moves Dough back to the arena scene, preserving world position.
    private void ReturnToScene()
    {
        Vector3 worldPos = GlobalPosition;
        GetParent().RemoveChild(this);
        _homeScene.AddChild(this);
        GlobalPosition = worldPos;
        SetPickupZoneActive(true);
    }

    private void SetPickupZoneActive(bool active)
    {
        var zone = GetNodeOrNull<Area3D>("PickupZone");
        if (zone != null) zone.Monitoring = active;
    }
}