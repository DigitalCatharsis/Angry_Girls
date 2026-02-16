using UnityEngine;

namespace Angry_Girls.Extensions
{
    /// <summary>
    /// Extension methods for GameObject and Component classes
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Safely attempts to get component in children
        /// </summary>
        public static bool TryGetComponentInChildren<T>(
            this GameObject gameObject,
            out T component,
            bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component != null;
        }

        /// <summary>
        /// Safely attempts to get component in children from a component
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this Component component, out T result, bool includeInactive = false) where T : Component
        {
            result = component.GetComponentInChildren<T>(includeInactive);
            return result != null;
        }
    }
}