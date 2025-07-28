using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public static class ControlPointPopupMenu
    {
            public static void Show(Vector2 position, ControlPoint controlPoint)
        {
            PopupMenuUtility.Show(
                CreateMenuItemsDeleteEdit(controlPoint)
                    .Append(MenuItem.Separator)
                    .Concat(CreateMenuItemsPointMode(controlPoint))
                ,
            
                position,
                controlPoint
            );
        }

        private static IEnumerable<MenuItem> CreateMenuItemsDeleteEdit(ControlPoint controlPoint)
        {
            yield return new MenuItem("Delete Key", controlPoint.Remove);
            yield return new MenuItem("Edit Key...", controlPoint.ShowEditKeyPopup);
        }
        
        public static IEnumerable<MenuItem> CreateMenuItemsPointMode(ControlPoint controlPoint)
        {
            yield return Create(PointMode.Smooth);
            yield return Create(PointMode.Flat);
            yield return Create(PointMode.Broken);
            yield break;

            MenuItem Create(PointMode mode)
            {
                return new MenuItem(mode.ToString(), () => controlPoint.SetPointMode(mode))
                {
                    isChecked = controlPoint.PointMode == mode
                };
            }
        }
    }
}