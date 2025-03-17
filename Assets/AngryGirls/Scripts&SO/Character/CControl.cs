using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Player,
        Ai,
    }

    [Serializable]
    public class CControl : PoolObject
    {
        [Header("Health")]
        [SerializeField] private float _currentHealth = 100f;
        public Slider healthSlider;
        public float CurrentHealth => _currentHealth;

        [Header("Repel Settings")]
        [SerializeField] private float _repelForce = 0.25f; // Базовая сила отталкивания
        [SerializeField] private float _repelForceDelta = 0.05f; // Дельта увеличения силы
        [SerializeField] private float _maxRepelForce = 1f; // Максимальная сила отталкивания
        private float _currentRepelForce; // Текущая сила отталкивания
        private int _hangCounter; // Счетчик "висения" на другом персонаже
        private bool _isHanging; // Флаг, что персонаж "висит" на другом

        private const float _finishTurnTimerValue = 3f;

        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;
        public bool unitGotHit = false;
        public bool hasUsedAbility = false;
        public bool canUseAbility = false;
        public bool hasBeenLaunched = false;
        public bool hasFinishedLaunchingTurn = false;
        public bool hasFinishedAlternateAttackTurn = true;

        [Space(5)]
        public bool unitBehaviorIsAlternate = true;
        public bool checkGlobalBehavior = false;

        public SubComponentMediator subComponentMediator;
        private SubComponentsController subComponentsController;

        public Rigidbody rigidBody;
        public BoxCollider boxCollider;
        public Animator animator;
        public AttackSystem_Data attackSystem_Data;
        private List<Collider> _currentCollisions = new List<Collider>(); // Список текущих столкновений


        [Space(10)]
        public Vector3 bottomRaycastContactPoint;

        [Header("Setup")]
        public PlayerOrAi playerOrAi;
        [Space(5)]
        public CharacterSettings characterSettings;
        [Space(5)]
        [Header("VFX")]
        public Transform projectileSpawnTransform;
        public Transform wingsTransform;
        public UnityEngine.Color VFX_Color;

        [Header("Weapon")]
        [SerializeReference]
        public Transform weaponHolder;

        public void UpdateHealth(float value)
        {
            _currentHealth += value;
        }

        public AttackAbilityLogic Get_AttackFinish_AttackAbilityLogic()
        {
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase)
            {
                return attackSystem_Data.launch_AttackFinishLogic;
            }
            else
            {
                return attackSystem_Data.alternate_AttackFinishLogic;
            }
        }

        public AttackAbility Get_AttackAbility()
        {
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase)
            {
                return characterSettings.AttackAbility_Launch;
            }
            else
            {
                return characterSettings.AttackAbility_Alternate;
            }
        }

        public bool CheckAttackFinishCondition()
        {
            if (isDead)
            {
                return false;
            }

            if (unitGotHit)
            {
                return true;
            }

            if (characterSettings.unitType == UnitType.Air && hasUsedAbility == true)
            {
                return true;
            }

            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.AlternatePhase)
            {
                if (hasFinishedAlternateAttackTurn)
                {
                    return false;
                }

                if (canUseAbility)
                {
                    return false;
                }
            }

            if (isGrounded)
            {
                return true;
            }

            return false;
        }

        public void FinishTurn(float finishAttackTimer = _finishTurnTimerValue)
        {
            StopCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
            isAttacking = false;

            ColorDebugLog.Log("ExecuteFinishTurnTimer" + gameObject.name, KnownColor.Yellow);
            //check for calls. Has been fixed, but to be sure
            StartCoroutine(ExecuteFinishTurnTimer(finishAttackTimer));
        }

        private IEnumerator ExecuteFinishTurnTimer(float timeToCheck)
        {
            var time = Time.deltaTime;
            while (time >= timeToCheck)
            {
                time += Time.deltaTime;
                yield return null;
            }

            hasFinishedLaunchingTurn = true;
            hasFinishedAlternateAttackTurn = true;
            ColorDebugLog.Log("Finishing Trun" + gameObject.name, KnownColor.Yellow);
            yield break;
        }

        public void ApplyKnockback(GameObject opponent, float knockbackForce)
        {
            if (rigidBody.velocity.z != 0)
            {
                return;
            }

            var direction = rigidBody.position - opponent.transform.position;

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

            if (rigidBody != null)
            {
                rigidBody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
            }
        }

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            subComponentMediator = GetComponentInChildren<SubComponentMediator>();
            subComponentsController = GetComponentInChildren<SubComponentsController>();

            subComponentMediator.OnAwake();
            subComponentsController.OnAwake();
        }

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

        private void HandleCharacterCollision()
        {
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
                if (rigidBody.velocity.y >= 0)
                    continue;

                // Проверяем, что другой персонаж стоит на земле или неподвижен
                var otherIsGrounded = otherRigidbody.velocity.magnitude < 0.1f; // Примерное условие
                if (!otherIsGrounded)
                    continue;

                // Вычисляем расстояние до противника
                float distance = Vector3.Distance(rigidBody.transform.position, otherRigidbody.transform.position);
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
            var zDifference = rigidBody.transform.position.z - otherRigidbody.transform.position.z;

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
            rigidBody.MovePosition(rigidBody.position + repelOffset);
        }

        private void FixedUpdate()
        {
            subComponentsController.OnFixedUpdate();

            // Если персонаж больше не "висит", сбрасываем счетчик и силу отталкивания
            if (!_isHanging)
            {
                _hangCounter = 0;
                _currentRepelForce = _repelForce;
            }


            // Сбрасываем флаг "висения" каждый кадр
            _isHanging = false;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            subComponentMediator.Notify_TriggerCheck(this, other);
        }

        private void Update()
        {
            subComponentsController.OnUpdate();
        }

        private void LateUpdate()
        {
            subComponentsController.OnLateUpdate();
        }

        private void Start()
        {
            subComponentsController.OnStart();

            //TEST WEAPON
            //TODO: implement properly
            if (weaponHolder != null)
            {
                var weaponPrefab = Resources.Load("DefinetlyNotAWeapon") as GameObject;
                var weapon = Instantiate(weaponPrefab, weaponHolder.transform.position, Quaternion.identity);

                weapon.gameObject.layer = this.gameObject.layer;
                foreach (Transform child in weapon.transform)
                {
                    child.gameObject.layer = this.gameObject.layer;
                }

                weapon.transform.parent = weaponHolder;
                weapon.transform.localScale = weaponHolder.transform.localScale;
                weapon.transform.position = weaponHolder.transform.position;
                weapon.transform.rotation = weaponHolder.transform.rotation;
            }
        }

        private void OnEnable()
        {
            if (playerOrAi == PlayerOrAi.Player)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(this);
            }
            else if (playerOrAi == PlayerOrAi.Ai)
            {
                GameLoader.Instance.characterManager.enemyCharacters.Add(this);
            }

            subComponentsController.OnComponentEnable();

            GameLoader.Instance.attackLogicContainer.SetCharacterAttackLogic(this);
            GameLoader.Instance.gameLogic_UIManager.CreateHealthBar(this);
        }


        //ObjectPooling
        protected override void Dispose(bool disposing)
        {
            GameLoader.Instance.gameLogic_UIManager.RemoveHealthBar(this);
            base.Dispose(disposing);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[characterSettings.characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(characterSettings.characterType, GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }
    }
}