using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    public static class Extensions
    {
        
        public static T Lowest<T>(this IEnumerable<T> list, System.Func<T, float> scoringMethod) => list.Lowest(scoringMethod, out _);
        public static T Lowest<T>(this IEnumerable<T> list, System.Func<T, float> scoringMethod, out int i) => list.Highest(e => -scoringMethod(e), out i);
        public static T Highest<T>(this IEnumerable<T> list, System.Func<T, float> scoringMethod) => list.Highest(scoringMethod, out _);
        public static T Highest<T>(this IEnumerable<T> list, System.Func<T, float> scoringMethod, out int i)
        {
            i = 0;
            var c = list.Count();
            if (c == 0) return default;
            if (c == 1) return list.First();

            var best = list.First();
            for (var j = 0; j < list.Count(); j++)
            {
                var element = list.ElementAt(j);
                var s1 = scoringMethod(element);
                var s2 = scoringMethod(best);
                if (s1 > s2)
                {
                    i = j;
                    best = element;
                }
            }

            return best;
        }

        public static Transform DeepFind (this Transform transform, string name)
        {
            var found = transform.Find(name);
            if (found) return found;
            foreach (Transform child in transform)
            {
                child.DeepFind(name);
            }
            return null;
        }
    }
}