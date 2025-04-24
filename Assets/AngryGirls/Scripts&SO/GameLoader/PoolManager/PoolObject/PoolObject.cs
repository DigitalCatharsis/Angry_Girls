using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public abstract class PoolObject : MonoBehaviour, IDisposable
    {
        public void Dispose()
        {
            OnDispose();
            this.gameObject.SetActive(false);

            DOTween.Kill(transform);

            ResetRigidbody();
            ResetTransform();
            ReturnToPool();
            Debug.Log(this.name + "has been returned to pool");
        }

        protected virtual void OnDispose() { }

        protected abstract void ReturnToPool();

        private void ResetRigidbody()
        {
            var rigid = gameObject.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                rigid.transform.position = Vector3.zero;
                rigid.rotation = Quaternion.identity;
            }
        }

        private void ResetTransform()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }
    }
}