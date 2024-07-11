using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class VFX : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private float _timeToLive = 1;
        [SerializeField] private bool _isTimeToLiveIsNormilizedTime = false;
        [SerializeField] private bool _destroyOnCollision = false;

        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        [ShowOnly] public float projectileDamage = 0f;


        private void OnEnable()
        {
            int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            gameObject.layer = LayerIgnoreRaycast;

            if (_isTimeToLiveIsNormilizedTime)
            {
                _timeToLive = _particleSystem.main.duration;
            }

            StartCoroutine(VFXLiving_Routine());
        }

        public void ApplyFlame(float damageAmount)
        {
            projectileDamage = damageAmount;
        }

        public void SendProjectile_Fireball__TweenMove(Vector3 startPoint, Vector3 finalRotationDegree, float damage, float moveDuration = 1.5f)
        {
            projectileDamage = damage;

            Vector3 frontDistance = new Vector3(0, 0, 2.2f);
            Vector3 gravityDistance = new Vector3(0, 2.8f, 8f);

            var waypoints = new[]
            {
                startPoint,
                new Vector3(transform.position.x, transform.position.y, transform.position.z + this.gameObject.transform.forward.z * frontDistance.z),
                new Vector3(transform.position.x, transform.position.y + Vector3.down.y * gravityDistance.y, transform.position.z + this.gameObject.transform.forward.z * gravityDistance.z),
            };


            transform.DOPath(waypoints, moveDuration, pathType: PathType.Linear, pathMode: PathMode.Full3D, resolution: 10, gizmoColor: UnityEngine.Color.green);
            transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * this.gameObject.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);
        }

        public void SendProjectile_Fireball(Vector3 impulse, Vector3 finalRotationDegree, float damage, float moveDuration = 1.5f)
        {
            projectileDamage = damage;

            this.gameObject.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
            transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * this.gameObject.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_destroyOnCollision == false)
            {
                return;
            }

            if (collision.gameObject.GetComponent<CharacterControl>())
            {
                return;
            }
            var poolManager = Singleton.Instance.poolManager;
            poolManager.GetObject(VFX_Type.VFX_Damage_White, poolManager.vfxPoolDictionary, transform.position, Quaternion.identity);
            this.gameObject.SetActive(false);
        }

        private IEnumerator VFXLiving_Routine()
        {
            yield return new WaitForSeconds(_timeToLive); //try this later!
            //var time = 0f;

            //while (time <= _timeToLive)
            //{
            //    time += Time.deltaTime;
            //    yield return null;
            //}

            this.gameObject.SetActive(false);
        }
    }
}