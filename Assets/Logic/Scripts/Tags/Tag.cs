using System.Collections.Generic;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Tags
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Tags/Tag")]
    public class Tag : ScriptableObject
    {
        private static Dictionary<Tag, List<TagHolder>> TagMap { get; } = new();
        private static HashSet<TagHolder> Registered { get; } = new();

        public static void Register (TagHolder holder)
        {
            if (Registered.Contains(holder)) return;

            foreach (var tag in holder.Tags)
            {
                if (!TagMap.ContainsKey(tag)) TagMap.Add(tag, new List<TagHolder>());

                TagMap[tag].Add(holder);
            }

            Registered.Add(holder);
        }

        public static void Deregister (TagHolder holder)
        {
            if (!Registered.Contains(holder)) return;

            foreach (var tag in holder.Tags)
            {
                TagMap[tag].Remove(holder);

                if (TagMap[tag].Count == 0) TagMap.Remove(tag);
            }

            Registered.Remove(holder);
        }

        public static IReadOnlyList<TagHolder> GetHoldersForTag (Tag tag)
        {
            if (!tag) return new List<TagHolder>();
            if (!TagMap.ContainsKey(tag)) return new List<TagHolder>();

            return TagMap[tag];
        }
    }

    public static class Extensions
    {
        public static bool HasTag(this Component component, Tag tag) => component.gameObject.HasTag(tag);
        public static bool HasTag (this GameObject gameObject, Tag tag)
        {
            if (!tag) return true;

            var holder = gameObject.GetComponentInParent<TagHolder>();
            if (!holder) return false;

            return holder.HasTag(tag);
        }
    }
}
