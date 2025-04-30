using UnityEngine;

namespace Angry_Girls
{
    [RequireComponent(typeof(Rigidbody), typeof(CControl))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private float _repelValue = 0.4f;

        public bool IsGrounded => CheckIfGrounded();
        private CControl _control;
        private Rigidbody _rigidbody;
        private BoxCollider _boxCollider;
        public Rigidbody Rigidbody => _rigidbody;

        //GroundDetection
        private LayerMask _ignoreLayerMask; // Маска для первого BoxCast (игнорируемые слои)
        private LayerMask _enemyLayerMask; // Маска для второго BoxCast (только противники)
        [Header("Multi-Collision Grounded Settings")]
        [SerializeField] private float _multiCollisionTimeThreshold = 4f; // Время для установки grounded
        private float _multiCollisionTimer; // Таймер для multi-collision


        private void Awake()
        {
            _control = GetComponent<CControl>();
            _boxCollider = GetComponent<BoxCollider>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        private void Start()
        {
            // Инициализация масок слоев
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch", "Character", "Bot", "DeadBody");
            _enemyLayerMask = LayerMask.GetMask("Bot");
        }

        public void ResetVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }

        public void ApplyRigidForce(Vector3 force, ForceMode forceMode = ForceMode.VelocityChange)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        public void ApplyKnockbackFromEnemy(GameObject opponent, float knockbackForce)
        {
            if (_rigidbody.velocity.z != 0) return;

            var direction = _rigidbody.position - opponent.transform.position;
            direction.z = direction.z < 0 ? -1 : 1;
            _rigidbody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameLoader.Instance.interactionManager.HandleCollision(gameObject, collision);
        }
        private void OnTriggerEnter(Collider other)
        {
            GameLoader.Instance.interactionManager.HandleTrigger(gameObject, other);
        }

        private void OnCollisionStay(Collision collision)
        {
            GameLoader.Instance.interactionManager.HandleCollision(gameObject, collision);
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

        public void HandleRepel(CharacterMovement target)
        {
            float zDiff = Rigidbody.position.z - target.Rigidbody.position.z;
            Vector3 repelDirection = zDiff switch
            {
                > 0 => Vector3.forward,
                < 0 => Vector3.back,
                _ => UnityEngine.Random.Range(0, 2) == 0 ? Vector3.back : Vector3.forward
            };

            Teleport(Rigidbody.position + repelDirection * (_repelValue * Time.fixedDeltaTime));
        }

        private bool CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return false;

            // Нижняя точка коллайдера
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);
            // Центр BoxCast области (0.13 выше нижней точки)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;
            // Размер BoxCast области (ширина и глубина как у коллайдера, высота небольшая)
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);
            // Длина луча (0.13 вниз от центра BoxCast)
            float rayLength = 0.13f;

            // Проверяем BoxCast (только противники)
            var enemyHits = Physics.BoxCastAll(boxCenter, boxSize / 2, Vector3.down, Quaternion.identity, rayLength, _enemyLayerMask);

            // Если второй BoxCast пересекает больше одного коллайдера
            if (enemyHits.Length >= 1 && _control.playerOrAi != PlayerOrAi.Bot)
            {
                // Увеличиваем таймер
                _multiCollisionTimer += Time.fixedDeltaTime;

                //ColorDebugLog.Log(control.name + "  " + _multiCollisionTimer.ToString(), System.Drawing.KnownColor.Red);

                // Если таймер превышает порог, устанавливаем isGrounded = true
                if (_multiCollisionTimer >= _multiCollisionTimeThreshold)
                {
                    foreach (var enemy in enemyHits)
                    {
                        _control.detectedGroundObject.Clear();
                        _control.detectedGroundObject.Add(enemy.collider.gameObject);
                    }
                    return true;// Не проверяем первый BoxCast
                }
            }
            else
            {
                // Сбрасываем таймер, если коллайдеров меньше двух
                _multiCollisionTimer = 0f;
            }

            // Проверяем первый BoxCast (обычная проверка приземления)
            RaycastHit hit;



            var isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);
            if (hit.collider != null && hit.collider.transform.root != gameObject.transform.root)
            {
                _control.detectedGroundObject.Clear();
                _control.detectedGroundObject.Add(hit.collider.gameObject);
            }

            return isGrounded;
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
            Gizmos.color = CheckIfGrounded() ? Color.green : Color.red;

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
    }
}