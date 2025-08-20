using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Context menu for control points in the Animation Curve Editor.
    /// </summary>
    /// <details>
    /// refs: Unity CurveMenuManager
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Editor/Mono/Animation/AnimationWindow/CurveMenuManager.cs#L72
    /// </details>
    public static class ControlPointPopupMenu
    {
            public static void Show(Vector2 position, ControlPoint controlPoint)
        {
            PopupMenuUtility.Show(
                CreateMenuItemsDeleteEdit(controlPoint)
                    .Append(new MenuItemSeparator())
                    .Concat(CreateMenuItemsPointMode(controlPoint))
                    .Append(new MenuItemSeparator())
                    .Concat(CreateMenuItemsTangentLeft(controlPoint))
                    .Concat(CreateMenuItemsTangentRight(controlPoint))
                    .Concat(CreateMenuItemsTangentBoth(controlPoint)),
                position,
                controlPoint
            );
        }

        private static IEnumerable<IMenuItem> CreateMenuItemsDeleteEdit(ControlPoint controlPoint)
        {
            yield return new MenuItem("Delete Key", controlPoint.Remove);
            yield return new MenuItem("Edit Key...", controlPoint.ShowEditKeyPopup);
        }

        private static IEnumerable<IMenuItem> CreateMenuItemsPointMode(ControlPoint controlPoint)
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
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangentLeft(ControlPoint controlPoint)
        {
            return CreateMenuItemsTangent("Left Tangent",
                controlPoint,
                () => controlPoint.InTangentMode,
                mode => controlPoint.SetInTangentMode(mode),
                WeightedMode.In
            );
        }
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangentRight(ControlPoint controlPoint)
        {
            return CreateMenuItemsTangent("Right Tangent",
                controlPoint,
                () => controlPoint.OutTangentMode,
                mode => controlPoint.SetOutTangentMode(mode),
                WeightedMode.Out
            );
        }
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangentBoth(ControlPoint controlPoint)
        {
            return CreateMenuItemsTangent("Both Tangents",
                controlPoint,
                GetTangentMode,
                SetTangentMode,
                WeightedMode.Both
            );

            TangentMode? GetTangentMode()
            {
                var inTangentMode = controlPoint.InTangentMode;
                var outTangentMode = controlPoint.OutTangentMode;
                if (inTangentMode == outTangentMode)
                {
                    return inTangentMode;
                }
                return null;
            }
            
            void SetTangentMode(TangentMode mode)
            {
                controlPoint.SetInTangentMode(mode);
                controlPoint.SetOutTangentMode(mode);
            }
        }
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangent(string menuPath,
            ControlPoint controlPoint,
            Func<TangentMode?> getTangentMode,
            Action<TangentMode> setTangentMode,
            WeightedMode weightedMode)
        {
            yield return TangentModeMenu(TangentMode.Free);
            yield return TangentModeMenu(TangentMode.Linear);
            yield return TangentModeMenu(TangentMode.Constant);
            var weightedModeFlag = GetWeightedMode();
            yield return new MenuItem($"{menuPath}/Weighted", () => SetWeightedMode(!weightedModeFlag))
            {
                isChecked = weightedModeFlag
            };
            yield break;

            MenuItem TangentModeMenu(TangentMode mode)
            {
                return new MenuItem($"{menuPath}/{mode.ToString()}", () => setTangentMode?.Invoke(mode))
                {
                    isChecked = getTangentMode?.Invoke() == mode
                };
            };

            void SetWeightedMode(bool flag)
            {
                var keyframe = controlPoint.Keyframe;
                keyframe.SetWeightedFrag(weightedMode, flag);
                controlPoint.Keyframe = keyframe;
            }
            
            bool GetWeightedMode()
            {
                return controlPoint.Keyframe.GetWeightedFrag(weightedMode);
            }
        }
    }
}