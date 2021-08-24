using UnityEngine;
using UnityEngine.UIElements;

namespace Comugi.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class ComugiUIToolkitRoot : MonoBehaviour
    {
        UIDocument uiDocument;
        Element rootElement;


        void Update()
        {
            rootElement.Update();
        }

        public void Build(Element element)
        {
            rootElement = element;


            if ( uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }


            var root = uiDocument.rootVisualElement;
            root.Add(UIToolkitBuilder.Build(element));
        }
    }
}