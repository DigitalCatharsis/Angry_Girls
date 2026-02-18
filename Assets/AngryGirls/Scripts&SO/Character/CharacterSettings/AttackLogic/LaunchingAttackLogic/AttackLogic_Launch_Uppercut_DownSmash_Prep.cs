using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_Uppercut_Prep : AttackAbility
    {
        public AttackLogic_Launch_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private GameObject _vfx;
        private LayerMask _originalExcludeLayers;
        private bool _layersModified;
        private LayerMask _targetLayer;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);

            _vfx = GameplayCoreManager.Instance.ProjectileManager.SpawnByProjectileAbility(control);
            _layersModified = false;

            // Determine the target layer depending on the object type
            _targetLayer = control.playerOrAi == PlayerOrAi.Player ?
            LayerMask.GetMask("Bot") :
            LayerMask.GetMask("Character");
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
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

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            // Ensure we restore the original excludeLayers
            if (_layersModified && control.boxCollider != null)
            {
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }

            CoreManager.Instance.VFXManager.FadeOutAndDisposeVFX(_vfx, 2f, 3f);
        }
    }
}