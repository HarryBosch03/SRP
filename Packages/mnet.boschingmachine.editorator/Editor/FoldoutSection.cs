using System;
using UnityEngine;

namespace Editorator.Editor
{
    public static partial class E
    {
        public static void FoldoutSection(string reference, Action body)
        {
            Section(() => Foldout(reference, () =>
            {
                Separator(0.0f, 0.5f);
                body();
            }));
        }

        public static void FoldoutSection(string reference, Action<Rect> header, Action body)
        {
            Section(() => Foldout(reference, header, () =>
            {
                Separator();
                body();
            }));
        }
    }
}