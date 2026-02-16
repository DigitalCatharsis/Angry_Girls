using System.Collections.Generic;

namespace Angry_Girls
{
    public static class ListExtentions
    {
        public static bool TryAdd<T>(this List<T> list, T item)
        {
            if (item == null)
                return false;

            list.Add(item);
            return true;
        }
    }
}
