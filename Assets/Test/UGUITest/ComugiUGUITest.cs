using Comugi.Test;
using UnityEngine;

namespace Comugi.UGUI.Test
{
    public class ComugiUGUITest : ComugiTest
    {
        [SerializeField]
        public ComugiUGUIRoot root;

        protected override void BuildElement(Element rootElement)
        {
            root.Build(rootElement);
        }
    }
}