using System;
using UnityEngine;

namespace Editorator.Editor
{
    public class FoldoutSection : Element
    {
        private readonly Foldout foldout;
        private readonly Section section;
        
        public FoldoutSection(string reference)
        {
            foldout = new Foldout(reference);
            section = new Section
            {
                body = () => foldout.Finish()
            };
        }

        public FoldoutSection Header(Header header)
        {
            foldout.header = header;
            return this;
        }
        
        public FoldoutSection Body(Body body)
        {
            foldout.body = body;
            return this;
        }
        
        public override void Finish()
        {
            section.Finish();
        }
    }
}