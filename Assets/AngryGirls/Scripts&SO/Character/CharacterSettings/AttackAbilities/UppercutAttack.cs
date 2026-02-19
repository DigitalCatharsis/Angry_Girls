using UnityEngine;

namespace Angry_Girls
{
    public class UppercutAttack : AttackAbility
    {
        private GameObject _projectile;
        private LayerMask _originalExcludeLayers;
        private bool _layersModified;
        private LayerMask _targetLayer;
        private bool _cameraShaked = false;

        public UppercutAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }

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

        public override void OnLaunchFinishEnter(CControl control)
        {
            base.OnLaunchFinishEnter(control);
            _projectile = projectileManager.SpawnDownSmash(control, control.attackAbility.LaunchFinishData);
        }
        public override void OnLaunchFinishUpdate(CControl control)
        {
            FinishUpdate(control, 0.09f);
        }
        public override void OnLaunchFinishExit(CControl control)
        {
            base.OnLaunchFinishExit(control);
            FinishEnd(control);
        }
        public override void OnAlternateFinishEnter(CControl control)
        {
            base.OnAlternateFinishEnter(control);
            _projectile = projectileManager.SpawnDownSmash(control, control.attackAbility.AlternateFinishData);
            _projectile.transform.position = control.CharacterMovement.Rigidbody.position;
        }
        public override void OnAlternateFinishUpdate(CControl control)
        {
            FinishUpdate(control, 0.14f);
        }
        public override void OnAlternateFinishExit(CControl control)
        {
            base.OnAlternateFinishExit(control);
            FinishEnd(control);
        }


        #region same logic
        private void PrepEnter(CControl control, AttackAbilityData abilityData)
        {

            _projectile = projectileManager.SpawnByProjectileAbility(control, abilityData);
            _layersModified = false;

            // Determine the target layer depending on the object type
            _targetLayer = control.playerOrAi == PlayerOrAi.Player ? LayerMask.GetMask("Bot") : LayerMask.GetMask("Character");
        }
        private void PrepUpdate(CControl control)
        {
            // Get the bottom point of the collider
            var bounds = control.boxCollider.bounds;
            var bottomPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            // BoxCast parameters
            var boxCenter = bottomPoint + Vector3.up * 0.13f;
            var boxHalfExtents = new Vector3(bounds.size.x * 0.5f, 0.05f, bounds.size.z * 0.5f);
            float rayLength = 0.13f;

            var hits = Physics.BoxCastAll(
            boxCenter,
            boxHalfExtents,
            Vector3.down,
            control.transform.rotation,
            rayLength,
            _targetLayer);

            // If 2+ objects of the desired layer are detected
            if (hits.Length >= 1)
            {
                if (!_layersModified)
                {
                    // Keep the original excluded layers
                    _originalExcludeLayers = control.boxCollider.excludeLayers;

                    // Add the target layer to the excluded layers
                    control.boxCollider.excludeLayers |= _targetLayer;
                    _layersModified = true;
                }
            }
            else if (_layersModified)
            {
                // Restore the original excluded layers
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }

            if (control.CharacterMovement.IsGrounded)
            {
                control.UnitCallsForStopAttack?.Invoke();
            }
        }
        private void PrepExit(CControl control)
        {
            // Ensure we restore the original excludeLayers
            if (_layersModified && control.boxCollider != null)
            {
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }

            vFXManager.FadeOutAndDisposeVFX(_projectile, 2f, 3f);
        }


        private void FinishUpdate(CControl control, float animationNormilizedTimeForShakeCamera)
        {
            base.OnLaunchFinishUpdate(control);
            var stateInfo = control.GetAnimatorStateInfo();
            if (stateInfo.normalizedTime >= animationNormilizedTimeForShakeCamera && control.CharacterMovement.IsGrounded && _cameraShaked == false)
            {
                GameplayCoreManager.Instance.CameraManager.ShakeCamera();

                _cameraShaked = true;
            }

            if (stateInfo.normalizedTime >= 1 && control.CharacterMovement.IsGrounded)
            {
                control.UnitCallsForStopAttackfinish?.Invoke();
            }
        }
        private void FinishEnd(CControl control)
        {
            GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_projectile);
            _cameraShaked = false;
            control.UnitPerformedAttackFinish?.Invoke();
        }
        #endregion
    }
}