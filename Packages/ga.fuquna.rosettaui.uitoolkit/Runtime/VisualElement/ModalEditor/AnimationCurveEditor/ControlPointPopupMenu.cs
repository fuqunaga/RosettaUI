using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

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
        public static void Show(Vector2 position, SelectedControlPointsEditor controlPointsEditor, VisualElement target)
        {
            PopupMenuUtility.Show(
                CreateMenuItemsDeleteEdit(controlPointsEditor)
                    .Append(new MenuItemSeparator())
                    .Concat(CreateMenuItemsBothTangentMode(controlPointsEditor))
                    .Append(new MenuItemSeparator())
                    .Concat(CreateMenuItemsTangent(controlPointsEditor)),
                position,
                target
            );
        }

        private static IEnumerable<IMenuItem> CreateMenuItemsDeleteEdit(SelectedControlPointsEditor controlPointsEditor)
        {
            var isMultiSelection = controlPointsEditor.IsMultiSelection;
            yield return new MenuItem(isMultiSelection ? "Delete Keys" :"Delete Key", controlPointsEditor.RemoveAll);
            // yield return new MenuItem(isMultiSelection ? "Edit Keys..." :"Edit Key...", curveController.ShowEditKeyPopup);
        }

        private static IEnumerable<IMenuItem> CreateMenuItemsBothTangentMode(SelectedControlPointsEditor controlPointsEditor)
        {
            var anyKeys = !controlPointsEditor.IsEmpty;
            var allFreeSmooth = anyKeys;
            var allFlat = anyKeys;
            var allBroken = anyKeys;
            
            var controlPoints = controlPointsEditor.ControlPoints;
            foreach (var controlPoint in controlPoints)
            {
                var key = controlPoint.Keyframe;
                var leftMode = controlPoint.InTangentMode;
                var rightMode = controlPoint.OutTangentMode;
                var broken = controlPoint.IsKeyBroken;
                
                if (broken || leftMode != TangentMode.Free || rightMode != TangentMode.Free) allFreeSmooth = false;
                if (broken || leftMode != TangentMode.Free || key.inTangent != 0 || rightMode != TangentMode.Free || key.outTangent != 0) allFlat = false;
                if (!broken) allBroken = false;
            }
            
            
            yield return Create("Free Smooth", allFreeSmooth, () =>
            {
                controlPointsEditor.SetBothTangentMode(TangentMode.Free);
                controlPointsEditor.SetKeyBroken(false);
            });
            yield return Create("Flat", allFlat, () =>
            {
                controlPointsEditor.SetBothTangentMode(TangentMode.Free, false);
                controlPointsEditor.SetKeyBroken(false, false);
                controlPointsEditor.UpdateKeyframe(keyframe =>
                {
                    keyframe.inTangent = 0;
                    keyframe.outTangent = 0;
                    return keyframe;
                });
            });
            yield return Create("Broken", allBroken, () => controlPointsEditor.SetKeyBroken(true));
            
            yield break;

            MenuItem Create(string name, bool isChecked, Action action)
            {
                return new MenuItem(name, action)
                {
                    isChecked = isChecked
                };
            }
        }
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangent(SelectedControlPointsEditor controlPointsEditor)
        {
            var anyKeys = !controlPointsEditor.IsEmpty;
            var allLeftWeighted = anyKeys;
            var allLeftFree = anyKeys;
            var allLeftLinear = anyKeys;
            var allLeftConstant = anyKeys;
            var allRightWeighted = anyKeys;
            var allRightFree = anyKeys;
            var allRightLinear = anyKeys;
            var allRightConstant = anyKeys;
            
            var controlPoints = controlPointsEditor.ControlPoints;
            foreach (var controlPoint in controlPoints)
            {
                var key = controlPoint.Keyframe;
                var leftMode = controlPoint.InTangentMode;
                var rightMode = controlPoint.OutTangentMode;
                var broken = controlPoint.IsKeyBroken;
                
                if (!broken || leftMode  != TangentMode.Free) allLeftFree = false;
                if (!broken || leftMode  != TangentMode.Linear) allLeftLinear = false;
                if (!broken || leftMode  != TangentMode.Constant) allLeftConstant = false;
                if (!broken || rightMode != TangentMode.Free) allRightFree = false;
                if (!broken || rightMode != TangentMode.Linear) allRightLinear = false;
                if (!broken || rightMode != TangentMode.Constant) allRightConstant = false;
                
                if (((int)key.weightedMode & (int)WeightedMode.In) == 0) allLeftWeighted = false;
                if (((int)key.weightedMode & (int)WeightedMode.Out) == 0) allRightWeighted = false;
            }
            
            
            var leftItems = CreateMenuItemsTangent("Left Tangent",
                allLeftFree, allLeftLinear, allLeftConstant, allLeftWeighted,
                mode => SetTangentMode((true, false), mode),
                weighted => controlPointsEditor.SetWeightedMode(WeightedMode.In, weighted)
            );
            
            var rightItems = CreateMenuItemsTangent("Right Tangent",
                allRightFree, allRightLinear, allRightConstant, allRightWeighted,
                mode => SetTangentMode((false, true), mode),
                weighted => controlPointsEditor.SetWeightedMode(WeightedMode.Out, weighted)
            );
            
            var bothItems = CreateMenuItemsTangent("Both Tangents",
                allLeftFree && allRightFree,
                allLeftLinear && allRightLinear,
                allLeftConstant && allRightConstant,
                allLeftWeighted && allRightWeighted,
                mode => SetTangentMode((true, true), mode),
                weighted => controlPointsEditor.SetWeightedMode(WeightedMode.Both, weighted)
            );
            
            return leftItems.Concat(rightItems).Concat(bothItems);

            void SetTangentMode((bool inEnable, bool outEnable) enable, TangentMode mode)
            {
                if (enable.inEnable)
                {
                    controlPointsEditor.SetTangentMode(InOrOut.In, mode);
                }

                if (enable.outEnable)
                {
                    controlPointsEditor.SetTangentMode(InOrOut.Out, mode);
                }
                
                controlPointsEditor.SetKeyBroken(true);
            }
        }
        
        private static IEnumerable<IMenuItem> CreateMenuItemsTangent(string menuPath,
            bool allFree, bool allLinear, bool allConstant, bool allWeighted,
            Action<TangentMode> setTangentMode,
            Action<bool> setWeightedMode)
        {
            yield return TangentModeMenu(TangentMode.Free, allFree);
            yield return TangentModeMenu(TangentMode.Linear, allLinear);
            yield return TangentModeMenu(TangentMode.Constant, allConstant);
            
            yield return new MenuItem($"{menuPath}/Weighted", () => setWeightedMode?.Invoke(!allWeighted))
            {
                isChecked = allWeighted
            };
            
            yield break;

            
            MenuItem TangentModeMenu(TangentMode mode, bool isChecked)
            {
                return new MenuItem($"{menuPath}/{mode.ToString()}", () => setTangentMode?.Invoke(mode))
                {
                    isChecked = isChecked
                };
            }
        }
    }
}