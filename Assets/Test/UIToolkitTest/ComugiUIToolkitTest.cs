using Comugi.Test;
using UnityEngine;


namespace Comugi.UIToolkit.Test
{
    [RequireComponent(typeof(ComugiUIToolkitRoot))]
    public class ComugiUIToolkitTest : ComugiTest
    {
        ComugiUIToolkitRoot comugiRoot;

        protected override void BuildElement(Element rootElement)
        {
            if (comugiRoot == null)
            {
                comugiRoot = GetComponent<ComugiUIToolkitRoot>();
            }

            comugiRoot.Build(rootElement);
        }
    }
}