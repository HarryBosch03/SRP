using System;

namespace BMRP.Runtime
{
    public static class ScalarComparison
    {
        public static Comparison<T> AscendingFloat<T>(Func<T, float> v)
        {
            return (a, b) => v(a) > v(b) ? 1 : 0;
        }
    }
}