#nullable enable

using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.NestedDropdownMenuSystem
{
    public static class VisualElementExtensions
    {
        private const int PseudoStatesHoverFlag = 2; // Corresponds to PseudoStates.Hover
        private static Func<VisualElement, int>? _getPseudoStatesFunc;
        private static Action<VisualElement, int>? _setPseudoStatesFunc;
        
        public static VisualElement GetFirstAncestorByClassName(this VisualElement element, string className)
        {
            while (element != null)
            {
                if (element.ClassListContains(className))
                {
                    return element;
                }

                element = element.parent;
            }

            throw new InvalidOperationException($"Ancestor with class name '{className}' not found.");
        }
        
        public static int GetPseudoStates(this VisualElement element)
        {
            if (_getPseudoStatesFunc == null)
            {
                var propertyInfo = typeof(VisualElement).GetProperty("pseudoStates", BindingFlags.Instance | BindingFlags.NonPublic);
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException("VisualElement に 'pseudoStates' プロパティがありません。");
                }

                var elementParam = Expression.Parameter(typeof(VisualElement), "element");
                var propertyAccess = Expression.Property(elementParam, propertyInfo);
                var convertToInt = Expression.Convert(propertyAccess, typeof(int));
                var getLambda = Expression.Lambda<Func<VisualElement, int>>(convertToInt, elementParam);
                _getPseudoStatesFunc = getLambda.Compile();
            }

            return _getPseudoStatesFunc(element);
        }
        
        public static void SetPseudoStates(this VisualElement element, int pseudoStates)
        {
            if (_setPseudoStatesFunc == null)
            {
                var propertyInfo = typeof(VisualElement).GetProperty("pseudoStates", BindingFlags.Instance | BindingFlags.NonPublic);
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException("VisualElement に 'pseudoStates' プロパティがありません。");
                }

                var elementParam = Expression.Parameter(typeof(VisualElement), "element");
                var valueParam = Expression.Parameter(typeof(int), "value");
                var valueConverted = Expression.Convert(valueParam, propertyInfo.PropertyType);
                var propertyAccess = Expression.Property(elementParam, propertyInfo);
                var assign = Expression.Assign(propertyAccess, valueConverted);
                var setLambda = Expression.Lambda<Action<VisualElement, int>>(assign, elementParam, valueParam);
                _setPseudoStatesFunc = setLambda.Compile();
            }

            _setPseudoStatesFunc(element, pseudoStates);
        }
        
        public static void AddPseudoStatesHover(this VisualElement element)
        {
            var currentStates = element.GetPseudoStates();
            element.SetPseudoStates(currentStates | PseudoStatesHoverFlag);
        }
    }
}