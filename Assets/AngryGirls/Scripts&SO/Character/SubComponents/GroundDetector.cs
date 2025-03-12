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
            //string layerToAdd;
            //if (control.playerOrAi == PlayerOrAi.Player)
            //{
            //    layerToAdd = "Character";
            //}
            //else
            //{
            //    layerToAdd = "Bot";
            //}

            // Получаем номер слоя корневого объекта
            //int rootLayer = transform.root.gameObject.layer;

            // Получаем маску для стандартных слоев
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch", "Character", "Bot" /*layerToAdd*/);
        }

        public override void OnFixedUpdate()
        {
            CheckIfGrounded();

            // Обновляем control.isGrounded
            control.isGrounded = _isGrounded;

            // Проверка момента приземления
            if (_isGrounded && !_wasGroundedLastFrame && _rigidbody.velocity.y <= 0)
            {
                OnLanding();
            }

            _wasGroundedLastFrame = _isGrounded;
        }

        private void CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // Нижняя точка коллайдера
            Vector3 bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // Центр BoxCast области (0.13 выше нижней точки)
            Vector3 boxCenter = bottomPoint + Vector3.up * 0.13f;

            // Размер BoxCast области (ширина и глубина как у коллайдера, высота небольшая)
            Vector3 boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // Длина луча (0.13 вниз от центра BoxCast)
            float rayLength = 0.13f;

            // Рисуем Gizmos для отладки
            Debug.DrawLine(boxCenter, boxCenter + Vector3.down * rayLength, Color.cyan, 0.1f);

            RaycastHit hit;
            _isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);

            // Логирование для отладки
            if (_isGrounded && control.name == "YBot_Red(Clone)")
            {
                Debug.Log("На земле. Нормаль: " + hit.normal + ", Точка: " + hit.point);
                Debug.Log(hit.collider.name);
            }
            else
            {
                //Debug.Log("В воздухе.");
            }
        }

        private void OnLanding()
        {
            Debug.Log("Приземление!");
            // Здесь можно добавить логику для момента приземления
        }

        // Отрисовка Gizmos
        private void OnDrawGizmos()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // Нижняя точка коллайдера
            Vector3 bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // Центр BoxCast области (0.13 выше нижней точки)
            Vector3 boxCenter = bottomPoint + Vector3.up * 0.13f;

            // Размер BoxCast области
            Vector3 boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

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