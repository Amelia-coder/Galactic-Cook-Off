using Godot;
using System.Collections.Generic;

namespace Scripts.Game.GenericComponents
{
    public enum EntityAnimation
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Attack,
        Hurt,
        Death
    }

    public partial class GenericAnimationComponent : Component
    {
        private AnimationPlayer _animPlayer;
        private readonly Dictionary<EntityAnimation, string> _animMap = new();
        private EntityAnimation _current = EntityAnimation.Idle;
        private EntityAnimation _synced = EntityAnimation.Idle;

        public void Init(AnimationPlayer player, Dictionary<EntityAnimation, string> animations)
        {
            _animPlayer = player;
            _animMap.Clear();

            foreach (var pair in animations)
                _animMap[pair.Key] = pair.Value;
        }

        public void SetCurrent(EntityAnimation anim)
        {
            _current = anim;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_animPlayer == null) return;

            if (!_animMap.TryGetValue(_current, out string animName))
                return;

            // Only play if changed
            if (_animPlayer.CurrentAnimation != animName)
                _animPlayer.Play(animName);

            // Only sync if changed
            if (_synced != _current)
            {
                _synced = _current;
                Rpc(MethodName.SyncAnimationRpc, (int)_current);
            }
        }

        // Sync — fire and forget, visuals only
        [Rpc(MultiplayerApi.RpcMode.AnyPeer,
             CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncAnimationRpc(int animIndex)
        {
            var anim = (EntityAnimation)animIndex;
            _current = anim;

            if (_animMap.TryGetValue(anim, out string animName))
                _animPlayer.Play(animName);
        }
    }
}
