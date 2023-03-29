using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using BMRP.Runtime;
using Code.Scripts.Editor;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using BMRP.Runtime.PostFX;
using Editorator.Editor;
using UnityEditor.Experimental.GraphView;

namespace BMRP.Editor
{
    [CustomEditor(typeof(PostFXSettings))]
    public class PostFXSettingsEditor : BetterEditor
    {
        private PostFXSettings Target => target as PostFXSettings;

        private Queue<PostEffect> deletedEffects = new();

        public override void OnInspectorGUI()
        {
            E.FoldoutSection("General Settings", () =>
            {
                base.OnInspectorGUI();
            });

            E.Separator();
            
            E.FoldoutSection("Post Effect Settings", () =>
            {
                Target.Effects.RemoveAll(e => !e);

                foreach (var effect in Target.Effects)
                {
                    DrawEffect(effect);
                }

                E.Button("Add...", AddNewEffectMenu);
            });
            
            if (deletedEffects.Count <= 0) return;
            
            Undo.RecordObject(Target, "Removed Effects from Post Process Stack");
            while (deletedEffects.Count > 0)
            {
                var effect = deletedEffects.Dequeue();
                Target.Effects.Remove(effect);
                DestroyImmediate(effect);
            }
            
            SceneView.RepaintAll();
        }

        private void DrawEffect(PostEffect effect)
        {
            E.FoldoutSection(effect.name, r =>
            {
                r.x += r.width - r.height * 2.0f;
                r.width = r.height * 2.0f;
                E.ImageButton("TreeEditor.Trash", () =>
                {
                    deletedEffects.Enqueue(effect);
                });

            }, () =>
            {
                E.Separator();
                E.AllVisibleProperties(effect);
            });
            
            Section(
                effect.name,
                () =>
                {
                    var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

                    rect.x += rect.width - rect.height * 2.0f;
                    rect.width = rect.height * 2.0f;

                    if (GUI.Button(rect, EditorGUIUtility.IconContent("TreeEditor.Trash")))
                    {
                        deletedEffects.Enqueue(effect);
                    }
                },
                () =>
                {
                },
                s => s);
        }

        private void AddNewEffectMenu()
        {
            var allTypes = Assembly.GetAssembly(typeof(PostEffect)).GetTypes();
            var types = new List<System.Type>();

            foreach (var type in allTypes)
            {
                if (!type.IsClass) continue;
                if (type.IsAbstract) continue;
                if (!type.IsSubclassOf(typeof(PostEffect))) continue;

                types.Add(type);
            }

            var labels = new string[types.Count];
            var i = 0;
            foreach (var type in types)
            {
                labels[i++] = type.Name;
            }

            var searchProvider = CreateInstance<SearchProvider>();
            searchProvider.types = types;
            searchProvider.callback = AddNewEffectCallback;
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                searchProvider);
        }

        private void AddNewEffectCallback(System.Type type)
        {
            Undo.RecordObject(Target, "Add New Effect to Post Process Stack");

            var dupes = 0;
            foreach (var effect in Target.Effects)
            {
                if (effect.GetType() != type) continue;
                dupes++;
            }

            var instance = (PostEffect)CreateInstance(type);
            instance.name = instance.DisplayName;
            if (dupes > 0) instance.name += $".{dupes + 1}";
            Target.Effects.Add(instance);

            SceneView.RepaintAll();
        }

        public class SearchProvider : ScriptableObject, ISearchWindowProvider
        {
            public IEnumerable<System.Type> types;
            public System.Action<System.Type> callback;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var searchList = new List<SearchTreeEntry>();
                searchList.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));

                foreach (var type in types)
                {
                    var tmp = (PostEffect)CreateInstance(type);
                    var name = tmp.DisplayName;
                    DestroyImmediate(tmp);
                    
                    searchList.Add(new SearchTreeEntry(new GUIContent(name))
                    {
                        level = 1,
                        userData = type,
                    });
                }

                return searchList;
            }

            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                callback((System.Type)searchTreeEntry.userData);
                return true;
            }
        }
    }
}