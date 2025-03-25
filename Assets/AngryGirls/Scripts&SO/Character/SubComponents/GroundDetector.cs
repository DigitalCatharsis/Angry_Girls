using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        private BoxCollider _boxCollider;
        private LayerMask _ignoreLayerMask; // Маска для первого BoxCast (игнорируемые слои)
        private LayerMask _enemyLayerMask; // Маска для второго BoxCast (только противники)
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;

        [Header("Multi-Collision Grounded Settings")]
        [SerializeField] private float _multiCollisionTimeThreshold = 4f; // Время для установки grounded
        private float _multiCollisionTimer; // Таймер для multi-collision

        public override void OnAwake()
        {
            _boxCollider = control.boxCollider;
        }

        public override void OnStart()
        {
            // Инициализация масок слоев
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch","Character","Bot");
            _enemyLayerMask = LayerMask.GetMask("Bot");
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

            // Проверяем второй BoxCast (только противники)
            var enemyHits = Physics.BoxCastAll(boxCenter, boxSize / 2, Vector3.down, Quaternion.identity, rayLength, _enemyLayerMask);

            // Если второй BoxCast пересекает больше одного коллайдера
            if (enemyHits.Length >= 1 && control.playerOrAi != PlayerOrAi.Ai)
            {
                // Увеличиваем таймер
                _multiCollisionTimer += Time.fixedDeltaTime;

                //ColorDebugLog.Log(control.name + "  " + _multiCollisionTimer.ToString(), System.Drawing.KnownColor.Red);

                // Если таймер превышает порог, устанавливаем isGrounded = true
                if (_multiCollisionTimer >= _multiCollisionTimeThreshold)
                {
                    _isGrounded = true;
                    return; // Не проверяем первый BoxCast
                }
            }
            else
            {
                // Сбрасываем таймер, если коллайдеров меньше двух
                _multiCollisionTimer = 0f;
            }

            // Проверяем первый BoxCast (обычная проверка приземления)
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