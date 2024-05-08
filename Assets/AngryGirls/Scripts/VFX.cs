using DG.Tweening;
using System.Collections;
using System.Drawing;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    public class VFX : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private float _timeToLive = 1;
        [SerializeField] private bool _isTimeToLiveIsNormilizedTime = false;

        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        public float projectileDamage = 10f;


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

        public void SendProjectile_Fireball(Vector3 startPoint, Vector3 finalRotationDegree, float moveDuration = 1.5f)
        {
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

        private void OnCollisionEnter(Collision collision)
        { 
            if (collision.gameObject.GetComponent<CharacterControl>())
            {
                return;
            }
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Damage_White, transform.position, Quaternion.identity);
            DestroySelf();
        }

        private IEnumerator VFXLiving_Routine()
        {
            var time = 0f;

            while (time <= _timeToLive)
            {
                time += Time.deltaTime;
                yield return null;
            }

            DestroySelf();
        }

        private void DestroySelf()
        {
            this.gameObject.SetActive(false);
        }
    }
}