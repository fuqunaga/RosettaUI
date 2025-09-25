using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace RosettaUI.Test
{
    public class InGameInputListener : MonoBehaviour
    {
        public TextMeshPro pointerTextMesh;
        public TextMeshPro mouseTextMesh;
        public TextMeshPro keyboardTextMesh;
        
        private readonly Dictionary<ButtonControl, (float, float)> _buttonControlPressReleaseTimeTable = new();
        
        private void Update()
        {
            var pointerString = DetectPointerInput();
            var mouseString = DetectMouseInput();
            var keyboardString = DetectKeyboardInput();
            
            pointerTextMesh.text = pointerString;
            mouseTextMesh.text = mouseString;
            keyboardTextMesh.text = keyboardString;
        }

        private string DetectPointerInput()
        {
            return "Pointer.current\n" +
                   CreatePointerInputString(Pointer.current);
        }
        
        private string DetectMouseInput()
        {
            var mouse = Mouse.current;
            return "Mouse.current\n" +
                   CreatePointerInputString(mouse) +
                   CreateButtonControlString(nameof(mouse.leftButton), mouse.leftButton) +
                   CreateButtonControlString(nameof(mouse.rightButton), mouse.rightButton) +
                   CreateButtonControlString(nameof(mouse.middleButton), mouse.middleButton) +
                   CreateButtonControlString(nameof(mouse.forwardButton), mouse.forwardButton) +
                   CreateButtonControlString(nameof(mouse.backButton), mouse.backButton) +
                   $"{IndentLevelToTag(1)}" +
                   CreateParameterString(nameof(mouse.scroll), mouse.scroll.ReadValue());
        }

        private string DetectKeyboardInput()
        {
            var keyboard = Keyboard.current;
            return "Keyboard.current\n" +
                   CreateInputDeviceString(keyboard) +
                   CreateButtonControlString(nameof(keyboard.anyKey), keyboard.anyKey);
        }

        private static string CreateInputDeviceString(InputDevice inputDevice, int indentLevel = 1)
        {
            return $"{IndentLevelToTag(indentLevel)}" +
                   CreateParameterString(nameof(inputDevice.deviceId), inputDevice.deviceId) +
                   CreateParameterString(nameof(inputDevice.enabled), inputDevice.enabled);
        }

        private string CreatePointerInputString(Pointer pointer, int indentLevel = 1)
        {
            return CreateInputDeviceString(pointer, indentLevel) +
                   CreateParameterString("position", pointer.position.ReadValue()) +
                   CreateButtonControlString("press", pointer.press, indentLevel);
        }

        private string CreateButtonControlString(string buttonName, ButtonControl button, int indentLevel = 1)
        {
            if (!_buttonControlPressReleaseTimeTable.TryGetValue(button, out var pressReleaseTime))
            {
                pressReleaseTime = (0, 0);
                _buttonControlPressReleaseTimeTable[button] = pressReleaseTime;
            }
            
            if (button.wasPressedThisFrame)
            {
                pressReleaseTime.Item1 = Time.realtimeSinceStartup;
            }
            if (button.wasReleasedThisFrame)
            {
                pressReleaseTime.Item2 = Time.realtimeSinceStartup;
            }

            _buttonControlPressReleaseTimeTable[button] = pressReleaseTime;


            return $"{IndentLevelToTag(indentLevel)}<#808080>{buttonName}</color>\n" +
                   $"{IndentLevelToTag(indentLevel + 1)}" +
                   CreateParameterString("isPressed", button.isPressed) +
                   CreateTriggerParameterString("wasPressedThisFrame", pressReleaseTime.Item1) +
                   CreateTriggerParameterString("wasReleasedThisFrame", pressReleaseTime.Item2);
        }

        private static string CreateParameterString<T>(string parameterName, T value)
        {
            return $"<#808080>{parameterName}</color> {value}\n";
        }
        
        private static string CreateTriggerParameterString(string parameterName, float lastTriggeredTime)
        {
            const float duration = 0.2f;
            const float alphaMin = 0.5f;
            
            var rate = 1f - (Time.realtimeSinceStartup - lastTriggeredTime) / duration;
            var value = rate >= 0;

            var alpha = value
                ? Mathf.Clamp01(alphaMin + (1f - alphaMin) * rate)
                : alphaMin;
            var alphaHex = ((int)(alpha * 255)).ToString("X2");
            
            return $"<#808080>{parameterName}</color> <alpha=#{alphaHex}>{value}\n";
        }
        
        private static int IndentLevelToSpaces(int indentLevel)
        {
            return indentLevel * 4;
        }
        
        private static string IndentLevelToTag(int indentLevel)
        {
            return $"<indent={IndentLevelToSpaces(indentLevel)}>";
        }
    }
}