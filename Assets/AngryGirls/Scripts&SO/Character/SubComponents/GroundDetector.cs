using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        [SerializeField] private float _groundCheckDistance = 0.1f; // ��������� ��� �������� �����
        [SerializeField] private LayerMask _ignoreLayerMask; // ����, ������� ������������

        private bool _isGrounded;
        private BoxCollider _boxCollider;
        private Rigidbody _rigidbody;

        public bool IsGrounded => _isGrounded;

        public override void OnAwake()
        {
            // ������������� ����������� �� control
            _boxCollider = control.boxCollider;
            _rigidbody = control.rigidBody;

            // ���������, ��� ���������� �������
            if (_boxCollider == null || _rigidbody == null)
            {
                Debug.LogError("BoxCollider ��� Rigidbody �� ������� � control!");
            }
        }

        public override void OnStart()
        {
            // ������������� ����� ����� (������ �� ������ ����)
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "Bot");
        }

        public override void OnUpdate()
        {
            CheckGround();
            control.isGrounded = IsGrounded;
        }

        public override void OnFixedUpdate()
        {
            // ����� �������� ������, ���� �����
        }

        public override void OnComponentEnable()
        {
            // ������ ��� ��������� ����������
        }

        public override void OnLateUpdate()
        {
            // ������ ����� ����������
        }

        private void CheckGround()
        {
            // ����������� �����, ����� ��������� �������� ����
            LayerMask groundLayerMask = ~_ignoreLayerMask;

            // �������� ������� ��������� BoxCollider
            Vector3 boxColliderCenter = _boxCollider.center;
            Vector3 boxColliderSize = _boxCollider.size;

            // ��������� ����� ��� �������� ����� (������ ����� BoxCollider)
            Vector3 groundCheckPosition = transform.position + boxColliderCenter - Vector3.up * (boxColliderSize.y / 2);

            // ��������� �������� � ������� Raycast
            bool hitGround = Physics.Raycast(
                groundCheckPosition,
                Vector3.down,
                _groundCheckDistance,
                groundLayerMask
            );

            // ��������� ���������
            _isGrounded = hitGround;
        }

        // ������������ ��� ������ (����� ������ � ��������� ������)
        private void OnDrawGizmos()
        {
            if (_boxCollider != null)
            {
                // �������� ������� ��������� BoxCollider
                Vector3 boxColliderCenter = _boxCollider.center;
                Vector3 boxColliderSize = _boxCollider.size;

                // ��������� ����� ��� �������� ����� (������ ����� BoxCollider)
                Vector3 groundCheckPosition = transform.position + boxColliderCenter - Vector3.up * (boxColliderSize.y / 2);

                // ������ ��� ��� ������������
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawLine(groundCheckPosition, groundCheckPosition + Vector3.down * _groundCheckDistance);
            }
        }
    }
}