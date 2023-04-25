using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    [CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField][HideInInspector] private List<PostEffect> effects;

        public List<PostEffect> Effects => effects;

        public static List<Type> GetEffectTypes()
        {
            var types = new List<Type>();
            var allTypes = Assembly.GetAssembly(typeof(PostEffect)).GetTypes();
            foreach (var type in allTypes)
            {
                if (!type.IsClass) continue;
                if (type.IsAbstract) continue;
                if (!type.IsSubclassOf(typeof(PostEffect))) continue;
                types.Add(type);
            }
            return types;
        }

        public void ReinstanceEffects(Action<PostEffect> newEffectCallback = null)
        {
            RemoveDuplicateTypes();
            
            var types = GetEffectTypes();
            foreach (var effect in effects)
            {
                types.Remove(effect.GetType());
            }

            foreach (var type in types)
            {
                var instance = (PostEffect)CreateInstance(type);
                instance.name = instance.DisplayName;
                newEffectCallback?.Invoke(instance);
                effects.Add(instance);
                
            }

            effects.RemoveAll(e => !e);
        }

        private void RemoveDuplicateTypes()
        {
            var types = new List<Type>();
            var itemsToRemove = new List<PostEffect>();

            foreach (var effect in effects)
            {
                var type = effect.GetType();
                if (types.Contains(type))
                {
                    itemsToRemove.Add(effect);
                    continue;
                }
                types.Add(type);
            }

            foreach (var effect in itemsToRemove)
            {
                effects.Remove(effect);
            }
        }
    }
}