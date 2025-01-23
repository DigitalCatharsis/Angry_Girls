using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public abstract class PoolObject : MonoBehaviour, IDisposable
    {
        [SerializeField]
        [ShowOnly]
        private bool _isDisposed = false;

        // Do we need Dispose here?
        // ðåàëèçàöèÿ èíòåðôåéñà IDisposable.
        public void Dispose()
        {
            // îñâîáîæäàåì íåóïðàâëÿåìûå ðåñóðñû
            Dispose(true);
        }

        private void OnEnable()
        {
            _isDisposed = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                this.gameObject.SetActive(false);

                ResetRigidbody();
                ResetTransform();
                ReturnToPool();
            }
            // îñâîáîæäàåì íåóïðàâëÿåìûå îáúåêòû
            _isDisposed = true;
        }

        protected abstract void ReturnToPool();

        public virtual void Init_PoolObject()
        {
            _isDisposed = false;
        }

        private void ResetRigidbody()
        {
            var rigid = gameObject.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
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