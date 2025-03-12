using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        private BoxCollider _boxCollider;
        private Rigidbody _rigidbody;
        private LayerMask _ignoreLayerMask;
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;

        public override void OnAwake()
        {
            _boxCollider = control.boxCollider;
            _rigidbody = control.rigidBody;
        }

        public override void OnStart()
        {
            //string layerToAdd;
            //if (control.playerOrAi == PlayerOrAi.Player)
            //{
            //    layerToAdd = "Character";
            //}
            //else
            //{
            //    layerToAdd = "Bot";
            //}

            // �������� ����� ���� ��������� �������
            //int rootLayer = transform.root.gameObject.layer;

            // �������� ����� ��� ����������� �����
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch", "Character", "Bot" /*layerToAdd*/);
        }

        public override void OnFixedUpdate()
        {
            CheckIfGrounded();

            // ��������� control.isGrounded
            control.isGrounded = _isGrounded;

            // �������� ������� �����������
            if (_isGrounded && !_wasGroundedLastFrame && _rigidbody.velocity.y <= 0)
            {
                OnLanding();
            }

            _wasGroundedLastFrame = _isGrounded;
        }

        private void CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // ������ ����� ����������
            Vector3 bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // ����� BoxCast ������� (0.13 ���� ������ �����)
            Vector3 boxCenter = bottomPoint + Vector3.up * 0.13f;

            // ������ BoxCast ������� (������ � ������� ��� � ����������, ������ ���������)
            Vector3 boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // ����� ���� (0.13 ���� �� ������ BoxCast)
            float rayLength = 0.13f;

            // ������ Gizmos ��� �������
            Debug.DrawLine(boxCenter, boxCenter + Vector3.down * rayLength, Color.cyan, 0.1f);

            RaycastHit hit;
            _isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);

            // ����������� ��� �������
            if (_isGrounded && control.name == "YBot_Red(Clone)")
            {
                Debug.Log("�� �����. �������: " + hit.normal + ", �����: " + hit.point);
                Debug.Log(hit.collider.name);
            }
            else
            {
                //Debug.Log("� �������.");
            }
        }

        private void OnLanding()
        {
            Debug.Log("�����������!");
            // ����� ����� �������� ������ ��� ������� �����������
        }

        // ��������� Gizmos
        private void OnDrawGizmos()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // ������ ����� ����������
            Vector3 bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // ����� BoxCast ������� (0.13 ���� ������ �����)
            Vector3 boxCenter = bottomPoint + Vector3.up * 0.13f;

            // ������ BoxCast �������
            Vector3 boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // ���� � ����������� �� ���������
            Gizmos.color = _isGrounded ? Color.green : Color.red;

            // ������ BoxCast �������
            Gizmos.DrawWireCube(boxCenter, boxSize);

            // ���� ���� �����������, ������ ������� � ����� �����������
            RaycastHit hit;
            if (Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.13f, ~_ignoreLayerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal); // ������� �����������
                Gizmos.DrawSphere(hit.point, 0.1f); // ����� �����������
            }
        }

        // ��������� ������ ���������� SubComponent
        public override void OnUpdate() { }
        public override void OnComponentEnable() { }
        public override void OnLateUpdate() { }
    }
}