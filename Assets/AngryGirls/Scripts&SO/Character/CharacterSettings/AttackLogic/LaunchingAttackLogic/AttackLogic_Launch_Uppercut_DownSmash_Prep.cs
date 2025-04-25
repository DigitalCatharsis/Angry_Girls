using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_Uppercut_Prep : AttackAbilityLogic
    {
        public AttackLogic_Launch_Uppercut_Prep(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private GameObject _vfx;
        private LayerMask _originalExcludeLayers;
        private bool _layersModified;
        private LayerMask _targetLayer;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);

            _vfx = GameLoader.Instance.VFXManager.SpawnByProjectileAbility(control);
            _layersModified = false;

            // Определяем целевой слой в зависимости от типа объекта
            _targetLayer = control.playerOrAi == PlayerOrAi.Character ?
                LayerMask.GetMask("Bot") :
                LayerMask.GetMask("Character");
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            // Получаем нижнюю точку коллайдера
            var bounds = control.boxCollider.bounds;
            var bottomPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            // Параметры BoxCast
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

            // Если обнаружено 2+ объекта нужного слоя
            if (hits.Length >= 1)
            {
                if (!_layersModified)
                {
                    // Сохраняем оригинальные исключенные слои
                    _originalExcludeLayers = control.boxCollider.excludeLayers;

                    // Добавляем целевой слой в исключенные
                    control.boxCollider.excludeLayers |= _targetLayer;
                    _layersModified = true;
                }
            }
            else if (_layersModified)
            {
                // Восстанавливаем оригинальные исключенные слои
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            // Гарантированно восстанавливаем оригинальные excludeLayers
            if (_layersModified && control.boxCollider != null)
            {
                control.boxCollider.excludeLayers = _originalExcludeLayers;
                _layersModified = false;
            }

            GameLoader.Instance.VFXManager.FadeOutAndDisposeVFX(_vfx, 2f, 3f);
        }
    }
}