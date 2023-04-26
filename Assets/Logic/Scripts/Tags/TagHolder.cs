using System.Collections.Generic;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Tags
{
    public class TagHolder : MonoBehaviour
    {
        [SerializeField] private List<Tag> tags = new List<Tag>();

        public List<Tag> Tags => tags;

        public bool HasTag (Tag tag)
        {
            return tags.Contains(tag);
        }

        private void OnEnable()
        {
            Tag.Register(this);
        }

        private void OnDisable()
        {
            Tag.Deregister(this);
        }

        private void OnDestroy()
        {
            Tag.Deregister(this);
        }
    }
}
