using System.Collections.Generic;

namespace weave
{
    public class CategoryComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Category obj)
        {
            return obj.Name.GetHashCode();
        }

        static CategoryComparer instance = new CategoryComparer();

        public static CategoryComparer Instance { get { return instance; } }
    }
}
