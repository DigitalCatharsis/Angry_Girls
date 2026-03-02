using UnityEngine;

namespace Angry_Girls
{
    public class UppercutAttack : AttackAbility
    {
        private GameObject _projectile;
        private LayerMask _originalExcludeLayers;
        private bool _layersModified;
        private LayerMask _targetLayer;
        //private bool _cameraShaked = false;

        private bool _animationFrozen = false;
        private float _freezeNormalizedTime = 0.33f;
        private bool _waitingForLanding = false;
        private bool _freezeTriggered = false;

        public UppercutAttack(AttackAbilityData launchPrep, AttackAbilityData alternatePrep): base(launchPrep,alternatePrep) { }

        #region Launch
        public override void OnLaunchPrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
            PrepEnter(control, control.attackAbility.LaunchPrepData);
        }

        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
            PrepUpdate(control);
        }

        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);
            PrepExit(control);
        }
        #endregion

        #region Alternate
        public override void OnAlternatePrepEnter(CControl control)
        {
            base.OnAlternatePrepEnter(control);
            PrepEnter(control, control.attackAbility.AlternatePrepData);
        }

        public override void OnAlternatePrepUpdate(CControl control)
        {
            base.OnAlternatePrepUpdate(control);
            PrepUpdate(control);
        }

        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            PrepExit(control);
        }
        #endregion

        #region Main Logic
        private void PrepEnter(CControl control, AttackAbilityData abilityData)
        {
            _projectile = projectileManager.SpawnByProjectileAbilityData(control, abilityData);
            _projectile.transform.position = control.CharacterMovement.Rigidbody.position;

            control.CharacterMovement.ResetVelocity();

            _layersModified = false;
            _animationFrozen = false;
            _waitingForLanding = false;
            //_cameraShaked = false;
            _freezeTriggered = false;

            _targetLayer = control.playerOrAi == PlayerOrAi.Player
                ? LayerMask.GetMask("Bot")
                : LayerMask.GetMask("Character");

            SetAnimationSpeed(control, 1f);
        }

        private void PrepUpdate(CControl control)
        {
            var animator = control.animator;
            var stateInfo = animator.IsInTransition(0)
                ? animator.GetNextAnimatorStateInfo(0)
                : animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime % 1f;

            if (normalizedTime >= 0.9f)
            {
                control.UnitCallsForStopAttack?.Invoke();
                return;
            }

            UpdateLayerModification(control);

            if (_animationFrozen && _waitingForLanding)
            {
                if (control.CharacterMovement.IsGrounded)
                {
                    SetAnimationSpeed(control, 1f);
                    _animationFrozen = false;
                    _waitingForLanding = false;
                    GameplayCoreManager.Instance.CameraManager.ShakeCamera();
                    //_cameraShaked = true;
                }
                return;
            }

            if (!_freezeTriggered && !_animationFrozen && normalizedTime >= _freezeNormalizedTime)
            {
                _freezeTriggered = true;
                SetAnimationSpeed(control, 0f);
                _animationFrozen = true;

                if (control.CharacterMovement.IsGrounded)
                {
                    SetAnimationSpeed(control, 1f);
                    _animationFrozen = false;
                    GameplayCoreManager.Instance.CameraManager.ShakeCamera();
                    //_cameraShaked = true;
                }
                else
                {
                    _waitingForLanding = true;
                }
            }
        }

        private void PrepExit(CControl control)
        {
            SetAnimationSpeed(control, 1f);

            if (_layersModified && control.boxCollider != null)
            {
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }

            if (_projectile != null)
            {
                vFXManager.FadeOutAndDisposeVFX(_projectile, 2f, 3f);
            }
        }

        private void SetAnimationSpeed(CControl control, float speed)
        {
            if (control.animator != null)
            {
                control.animator.speed = speed;
            }
        }

        private void UpdateLayerModification(CControl control)
        {
            var bounds = control.boxCollider.bounds;
            var bottomPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var boxCenter = bottomPoint + Vector3.up * 0.13f;
            var boxHalfExtents = new Vector3(bounds.size.x * 0.5f, 0.05f, bounds.size.z * 0.5f);

            var hits = Physics.BoxCastAll(
                boxCenter,
                boxHalfExtents,
                Vector3.down,
                control.transform.rotation,
                0.13f,
                _targetLayer);

            if (hits.Length >= 1)
            {
                if (!_layersModified)
                {
                    _originalExcludeLayers = control.boxCollider.excludeLayers;
                    control.boxCollider.excludeLayers |= _targetLayer;
                    _layersModified = true;
                }
            }
            else if (_layersModified)
            {
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }
        }
        #endregion
    }
}