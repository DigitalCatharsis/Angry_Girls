using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Defines how the direction of the deadbody push is derived from a projectile.
    /// </summary>
    public enum DeadbodyForceDirectionMode
    {
        /// <summary>Push along the projectile velocity at the moment of impact. Falls back to AwayFromProjectile when the projectile is static.</summary>
        ProjectileVelocity = 0,

        /// <summary>Push from the projectile center towards the body center (area attacks, static triggers).</summary>
        AwayFromProjectile = 1
    }

    /// <summary>
    /// Immutable snapshot of a projectile state taken at the moment it hits a target.
    /// Required because pooled projectiles get their position and velocity reset on disposal,
    /// which can happen before the hit is processed by the target.
    /// </summary>
    public readonly struct ProjectileImpact
    {
        public readonly bool IsValid;
        public readonly Vector3 Position;
        public readonly Vector3 Velocity;

        public ProjectileImpact(Vector3 position, Vector3 velocity)
        {
            IsValid = true;
            Position = position;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Pure helper that resolves the deadbody push direction on the XY gameplay plane.
    /// </summary>
    public static class DeadbodyForceCalculator
    {
        private const float MinPlanarSqrMagnitude = 0.01f;
        private const float MinHorizontalOffset = 0.001f;

        /// <summary>
        /// Calculates a normalized push direction on the XY plane (z is always zero).
        /// </summary>
        /// <param name="impact">Projectile snapshot taken at the moment of impact.</param>
        /// <param name="targetCenter">World center of the body being pushed.</param>
        /// <param name="originatorPosition">World position of the attacker, used as the last resort.</param>
        /// <param name="mode">Direction resolving mode.</param>
        /// <param name="verticalBias">Optional extra lift added to the Y component before normalization.</param>
        public static Vector3 CalculateDirection(
            in ProjectileImpact impact,
            Vector3 targetCenter,
            Vector3 originatorPosition,
            DeadbodyForceDirectionMode mode,
            float verticalBias)
        {
            var direction = ResolveBaseDirection(in impact, targetCenter, originatorPosition, mode);

            direction.y += verticalBias;
            direction.z = 0f;

            return direction.sqrMagnitude > MinPlanarSqrMagnitude * 0.01f
                ? direction.normalized
                : Vector3.right;
        }

        private static Vector3 ResolveBaseDirection(
            in ProjectileImpact impact,
            Vector3 targetCenter,
            Vector3 originatorPosition,
            DeadbodyForceDirectionMode mode)
        {
            if (impact.IsValid && mode == DeadbodyForceDirectionMode.ProjectileVelocity)
            {
                var velocity = impact.Velocity;
                velocity.z = 0f;

                if (velocity.sqrMagnitude > MinPlanarSqrMagnitude)
                    return velocity.normalized;
            }

            if (impact.IsValid)
            {
                var away = targetCenter - impact.Position;
                away.z = 0f;

                if (away.sqrMagnitude > MinPlanarSqrMagnitude)
                    return away.normalized;
            }

            // Last resort: push horizontally away from the attacker, never invent a vertical component.
            var fromOriginator = targetCenter - originatorPosition;
            var sign = Mathf.Abs(fromOriginator.x) > MinHorizontalOffset ? Mathf.Sign(fromOriginator.x) : 1f;
            return new Vector3(sign, 0f, 0f);
        }
    }
}
