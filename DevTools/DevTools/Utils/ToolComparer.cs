using DevTools.Entities;

namespace DevTools.Utils
{
    public class ToolComparer : IEqualityComparer<Tool>
    {
        public bool Equals(Tool? x, Tool? y)
        {
            if (x == null || y == null) return false;

            if (!string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(Tool obj)
        {
            return obj.Name.ToLower().GetHashCode();
        }
    }
}
