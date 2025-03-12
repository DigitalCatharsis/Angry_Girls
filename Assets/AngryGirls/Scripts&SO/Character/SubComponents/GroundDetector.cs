using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        [SerializeField] private float _groundCheckDistance = 0.1f; // Дистанция для проверки земли
        [SerializeField] private LayerMask _ignoreLayerMask; // Слои, которые игнорируются

        private bool _isGrounded;
        private BoxCollider _boxCollider;
        private Rigidbody _rigidbody;

        public bool IsGrounded => _isGrounded;

        public override void OnAwake()
        {
            // Инициализация компонентов из control
            _boxCollider = control.boxCollider;
            _rigidbody = control.rigidBody;

            // Проверяем, что компоненты найдены
            if (_boxCollider == null || _rigidbody == null)
            {
                Debug.LogError("BoxCollider или Rigidbody не найдены в control!");
            }
        }

        public override void OnStart()
        {
            // Инициализация маски слоев (пример из твоего кода)
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "Bot");
        }

        public override void OnUpdate()
        {
            CheckGround();
            control.isGrounded = IsGrounded;
        }

        public override void OnFixedUpdate()
        {
            // Можно добавить логику, если нужно
        }

        public override void OnComponentEnable()
        {
            // Логика при включении компонента
        }

        public override void OnLateUpdate()
        {
            // Логика после обновления
        }

        private void CheckGround()
        {
            // Инвертируем маску, чтобы исключить ненужные слои
            LayerMask groundLayerMask = ~_ignoreLayerMask;

            // Получаем текущие параметры BoxCollider
            Vector3 boxColliderCenter = _boxCollider.center;
            Vector3 boxColliderSize = _boxCollider.size;

            // Вычисляем точку для проверки земли (нижняя часть BoxCollider)
            Vector3 groundCheckPosition = transform.position + boxColliderCenter - Vector3.up * (boxColliderSize.y / 2);

            // Выполняем проверку с помощью Raycast
            bool hitGround = Physics.Raycast(
                groundCheckPosition,
                Vector3.down,
                _groundCheckDistance,
                groundLayerMask
            );

            // Обновляем состояние
            _isGrounded = hitGround;
        }

        // Визуализация для дебага (можно убрать в финальной версии)
        private void OnDrawGizmos()
        {
            if (_boxCollider != null)
            {
                // Получаем текущие параметры BoxCollider
                Vector3 boxColliderCenter = _boxCollider.center;
                Vector3 boxColliderSize = _boxCollider.size;

                // Вычисляем точку для проверки земли (нижняя часть BoxCollider)
                Vector3 groundCheckPosition = transform.position + boxColliderCenter - Vector3.up * (boxColliderSize.y / 2);

                // Рисуем луч для визуализации
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawLine(groundCheckPosition, groundCheckPosition + Vector3.down * _groundCheckDistance);
            }
        }
    }
}