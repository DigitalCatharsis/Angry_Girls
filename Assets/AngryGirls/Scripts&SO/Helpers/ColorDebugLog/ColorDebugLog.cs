using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Provides colored debug logging functionality
    /// </summary>
    public static class ColorDebugLog
    {
        /// <summary>
        /// Logs message with specified color
        /// </summary>
        public static void Log(string content, KnownColor color)
        {
            Debug.Log($"<color={color}>{content}</color>");
        }
    }
}