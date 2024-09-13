using System;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolObject : MonoBehaviour, IDisposable
    {
        [SerializeField][ShowOnly] private bool _isDisposedBase = false;

        // ���������� ���������� IDisposable.
        public void Dispose()
        {
            // ����������� ������������� �������
            Dispose(true);
            // ��������� �����������
            GC.SuppressFinalize(this);
        }
        private void OnEnable()
        {
            _isDisposedBase = false;
            //int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            //gameObject.layer = LayerIgnoreRaycast;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposedBase) return;
            if (disposing)
            {
                // ����������� ����������� �������

                this.gameObject.SetActive(false);
                ResetTransform();
            }
            // ����������� ������������� �������
            _isDisposedBase = true;
        }

        private void ResetTransform()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }

        // ����������
        ~PoolObject()
        {
            Dispose(false);
        }
    }
}
