using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public abstract class PoolObject : MonoBehaviour, IDisposable
    {
        [SerializeField] private bool _isDisposed = false;

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

                DOTween.Kill(transform);

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