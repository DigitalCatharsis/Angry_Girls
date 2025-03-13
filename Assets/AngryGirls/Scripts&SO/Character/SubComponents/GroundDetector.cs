using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        private BoxCollider _boxCollider;
        private Rigidbody _rigidbody;
        private LayerMask _ignoreLayerMask;
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;

        public override void OnAwake()
        {
            _boxCollider = control.boxCollider;
            _rigidbody = control.rigidBody;
        }

        public override void OnStart()
        {
            // Получаем маску для стандартных слоев
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch", "Character", "Bot" /*layerToAdd*/);
        }

        public override void OnFixedUpdate()
        {
            CheckIfGrounded();
            control.isGrounded = _isGrounded;

            _wasGroundedLastFrame = _isGrounded;
        }

        private void CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // Нижняя точка коллайдера
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // Центр BoxCast области (0.13 выше нижней точки)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;

            // Размер BoxCast области (ширина и глубина как у коллайдера, высота небольшая)
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // Длина луча (0.13 вниз от центра BoxCast)
            float rayLength = 0.13f;

            // Рисуем Gizmos для отладки
            Debug.DrawLine(boxCenter, boxCenter + Vector3.down * rayLength, Color.cyan, 0.1f);

            RaycastHit hit;
            _isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);
        }

        // Отрисовка Gizmos
        private void OnDrawGizmos()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // Нижняя точка коллайдера
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // Центр BoxCast области (0.13 выше нижней точки)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;

            // Размер BoxCast области
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // Цвет в зависимости от состояния
            Gizmos.color = _isGrounded ? Color.green : Color.red;

            // Рисуем BoxCast область
            Gizmos.DrawWireCube(boxCenter, boxSize);

            // Если есть пересечение, рисуем нормаль и точку пересечения
            RaycastHit hit;
            if (Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.13f, ~_ignoreLayerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal); // Нормаль поверхности
                Gizmos.DrawSphere(hit.point, 0.1f); // Точка пересечения
            }
        }

        // Остальные методы интерфейса SubComponent
        public override void OnUpdate() { }
        public override void OnComponentEnable() { }
        public override void OnLateUpdate() { }
    }
}