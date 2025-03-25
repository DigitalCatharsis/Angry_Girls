using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Repel Settings")]
        [SerializeField] private float _repelForce = 0.25f; // Базовая сила отталкивания
        [SerializeField] private float _repelForceDelta = 0.05f; // Дельта увеличения силы
        [SerializeField] private float _maxRepelForce = 1f; // Максимальная сила отталкивания
        private float _currentRepelForce; // Текущая сила отталкивания
        private int _hangCounter; // Счетчик "висения" на другом персонаже
        private bool _isHanging; // Флаг, что персонаж "висит" на другом


        public Vector3 bottomRaycastContactPoint;
        private List<Collider> _currentCollisions = new List<Collider>(); // Список текущих столкновений

        [Header("Core")]
        private Rigidbody _rigidbody;
        private CControl _control;

        public Rigidbody Rigidbody => _rigidbody;

        private void FixedUpdate()
        {
            // Если персонаж больше не "висит", сбрасываем счетчик и силу отталкивания
            if (!_isHanging)
            {
                _hangCounter = 0;
                _currentRepelForce = _repelForce;
            }

            // Сбрасываем флаг "висения" каждый кадр
            _isHanging = false;
        }

        public void Initialize(CControl control)
        {
            _control = control;
            _rigidbody = GetComponent<Rigidbody>();
        }

        #region Rigidbody Operations
        public void ResetVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }
        public void ApplyRigidForce(Vector3 force, ForceMode forceMode = ForceMode.VelocityChange)
        {
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }
        public void ApplyKnockbackFromEnemy(GameObject opponent, float knockbackForce)
        {
            if (_rigidbody.velocity.z != 0)
            {
                return;
            }

            var direction = _rigidbody.position - opponent.transform.position;

            // Проверить, что объекты не находятся в одной точке
            if (Math.Abs(direction.z) < 0.001f)
            {
                // Случайное направление
                int randomDirection = UnityEngine.Random.Range(0, 2);
                direction = new Vector3(0, 0, randomDirection == 0 ? -1 : 1);
            }
            else
            {
                // Нормализуем только z-компонент
                direction.z /= Math.Abs(direction.z);
            }

            _rigidbody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
        }
        public void SetPosition(Vector3 position)
        {
            _rigidbody.position = position;
        }
        public void SetRotation(Quaternion rotation)
        {
            _rigidbody.MoveRotation(rotation);
        }
        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.velocity = velocity;
        }
        public void Teleport(Vector3 position)
        {
            _rigidbody.MovePosition(position);
        }
        #endregion

        #region Collision operations
        private void OnCollisionEnter(Collision collision)
        {
            // Добавляем коллайдер в список текущих столкновений
            if (!_currentCollisions.Contains(collision.collider))
            {
                _currentCollisions.Add(collision.collider);
            }

            HandleCharacterCollision();
        }

        private void OnCollisionStay(Collision collision)
        {
            HandleCharacterCollision();
        }

        private void OnCollisionExit(Collision collision)
        {
            // Удаляем коллайдер из списка текущих столкновений
            if (_currentCollisions.Contains(collision.collider))
            {
                _currentCollisions.Remove(collision.collider);
            }
        }
        private void HandleRepel(Collider other)
        {
            // Получаем Rigidbody другого персонажа
            var otherRigidbody = other.attachedRigidbody;
            if (otherRigidbody == null)
                return;

            // Увеличиваем счетчик "висения"
            _hangCounter++;
            _isHanging = true;

            // Увеличиваем силу отталкивания, но не больше максимальной
            _currentRepelForce = Mathf.Min(_repelForce + _hangCounter * _repelForceDelta, _maxRepelForce);

            // Вычисляем разницу по оси Z между текущим персонажем и другим
            var zDifference = _rigidbody.position.z - otherRigidbody.transform.position.z;

            // Определяем направление отталкивания
            Vector3 repelDirection;
            if (zDifference > 0)
            {
                // Текущий персонаж впереди другого, отталкиваем вперед
                repelDirection = Vector3.forward;
            }
            else if (zDifference < 0)
            {
                // Текущий персонаж сзади другого, отталкиваем назад
                repelDirection = Vector3.back;
            }
            else
            {
                // Разница по Z равна нулю, отталкиваем в случайную сторону
                repelDirection = UnityEngine.Random.Range(0, 2) == 0 ? Vector3.back : Vector3.forward;
            }

            // Смещаем персонажа плавно, без изменения скорости
            Vector3 repelOffset = repelDirection * (_currentRepelForce * Time.fixedDeltaTime);

            Teleport(_rigidbody.position + repelOffset);
            //rigidBody.MovePosition(rigidBody.position + repelOffset);
        }
        private void HandleCharacterCollision()
        {
            if (_control.isGrounded)
            {
                return;
            }

            if (_currentCollisions.Count == 0)
                return;

            // Находим ближайшего противника
            Collider nearestCollider = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in _currentCollisions)
            {
                // Проверяем, что это другой персонаж (слой "Character" или "Bot")
                var otherLayer = collider.gameObject.layer;
                if (otherLayer != LayerMask.NameToLayer("Character") && otherLayer != LayerMask.NameToLayer("Bot"))
                    continue;

                // Получаем Rigidbody другого персонажа
                var otherRigidbody = collider.attachedRigidbody;
                if (otherRigidbody == null)
                    continue;

                // Проверяем, что текущий персонаж движется вниз
                if (_rigidbody.velocity.y >= 0)
                    continue;

                // Проверяем, что другой персонаж стоит на земле или неподвижен
                var otherIsGrounded = otherRigidbody.velocity.magnitude < 0.1f; // Примерное условие
                if (!otherIsGrounded)
                    continue;

                // Вычисляем расстояние до противника
                float distance = Vector3.Distance(_rigidbody.position, otherRigidbody.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCollider = collider;
                }
            }

            // Если ближайший противник найден, обрабатываем отталкивание
            if (nearestCollider != null)
            {
                HandleRepel(nearestCollider);
            }
        }
        #endregion

        // Вызывается из FixedUpdate
        public void UpdateGroundCheck() { }
    }
}