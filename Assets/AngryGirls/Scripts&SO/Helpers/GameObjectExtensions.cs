using UnityEngine;

namespace Angry_Girls.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool TryGetComponentInChildren<T>(
            this GameObject gameObject,
            out T component,
            bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component != null;
        }

        public static bool TryGetComponentInChildren<T>(this Component component, out T result, bool includeInactive = false) where T : Component
        {
            result = component.GetComponentInChildren<T>(includeInactive);
            return result != null;
        }
    }
}