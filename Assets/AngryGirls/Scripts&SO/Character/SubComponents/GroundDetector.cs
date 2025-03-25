using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        private BoxCollider _boxCollider;
        private LayerMask _ignoreLayerMask; // ����� ��� ������� BoxCast (������������ ����)
        private LayerMask _enemyLayerMask; // ����� ��� ������� BoxCast (������ ����������)
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;

        [Header("Multi-Collision Grounded Settings")]
        [SerializeField] private float _multiCollisionTimeThreshold = 4f; // ����� ��� ��������� grounded
        private float _multiCollisionTimer; // ������ ��� multi-collision

        public override void OnAwake()
        {
            _boxCollider = control.boxCollider;
        }

        public override void OnStart()
        {
            // ������������� ����� �����
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable", "CharacterToLaunch","Character","Bot");
            _enemyLayerMask = LayerMask.GetMask("Bot");
        }

        public override void OnFixedUpdate()
        {
            CheckIfGrounded();
            control.isGrounded = _isGrounded;

            _wasGroundedLastFrame = _isGrounded;
        }

        private void CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // ������ ����� ����������
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // ����� BoxCast ������� (0.13 ���� ������ �����)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;

            // ������ BoxCast ������� (������ � ������� ��� � ����������, ������ ���������)
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            // ����� ���� (0.13 ���� �� ������ BoxCast)
            float rayLength = 0.13f;

            // ��������� ������ BoxCast (������ ����������)
            var enemyHits = Physics.BoxCastAll(boxCenter, boxSize / 2, Vector3.down, Quaternion.identity, rayLength, _enemyLayerMask);

            // ���� ������ BoxCast ���������� ������ ������ ����������
            if (enemyHits.Length >= 1 && control.playerOrAi != PlayerOrAi.Ai)
            {
                // ����������� ������
                _multiCollisionTimer += Time.fixedDeltaTime;

                //ColorDebugLog.Log(control.name + "  " + _multiCollisionTimer.ToString(), System.Drawing.KnownColor.Red);

                // ���� ������ ��������� �����, ������������� isGrounded = true
                if (_multiCollisionTimer >= _multiCollisionTimeThreshold)
                {
                    _isGrounded = true;
                    return; // �� ��������� ������ BoxCast
                }
            }
            else
            {
                // ���������� ������, ���� ����������� ������ ����
                _multiCollisionTimer = 0f;
            }

            // ��������� ������ BoxCast (������� �������� �����������)
            RaycastHit hit;
            _isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);
        }

        // ��������� Gizmos
        private void OnDrawGizmos()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            // ������ ����� ����������
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // ����� BoxCast ������� (0.13 ���� ������ �����)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;

            // ������ BoxCast �������
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

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