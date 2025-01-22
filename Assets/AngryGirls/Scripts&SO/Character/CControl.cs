using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using UnityEditor.ShaderGraph;
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
        [Space(15)]
        [Header("Debug")]
        [Header("Pool object")]
        [SerializeField] private CharacterType _characterType;

        [Header("Health")]
        public float currentHealth;
        public Slider healthSlider;

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
        public ContactPoint[] boxColliderContacts;
        [Space(10)]
        public CollisionSpheresData collisionSpheresData;

        [Header("Setup")]
        public PlayerOrAi playerOrAi;
        [Space(5)]
        public CharacterSettings characterSettings;
        [Space(5)]
        [Header("VFX")]
        public Transform projectileSpawnTransform;
        public UnityEngine.Color VFX_Color;

        [Header("Weapon")]
        [SerializeReference]
        public Transform weaponHolder;

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

        public void FinishTurn(float finishAttackTimer)
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
            if (rigidBody.velocity != Vector3.zero)
            {
                return;
            }
            var direction = transform.position - opponent.transform.position;

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
                rigidBody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.Impulse);
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

        private void OnCollisionStay(Collision collision)
        {
            if (collision != null)
            boxColliderContacts = (collision.contacts);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead)
            {
                return;
            }

            subComponentMediator.Notify(this, NotifyContact_EventNames.CharacterCollider_Trigger_Enter, other);
        }

        private void Update()
        {
            subComponentsController.OnUpdate();
        }

        private void FixedUpdate()
        {
            subComponentsController.OnFixedUpdate();
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
            GameLoader.Instance.UIManager.CreateHealthBar(this);
        }


        //ObjectPooling
        protected override void Dispose(bool disposing)
        {
            GameLoader.Instance.UIManager.RemoveHealthBar(this);
            base.Dispose(disposing);
        }

        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[_characterType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(_characterType, GameLoader.Instance.poolManager.characterPoolDictionary, this);
            }
        }
    }
}