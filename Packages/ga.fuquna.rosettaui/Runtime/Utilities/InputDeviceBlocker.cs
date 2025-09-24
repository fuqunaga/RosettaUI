#if ENABLE_INPUT_SYSTEM

using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace RosettaUI
{
    /// <summary>
    /// UIの状態に応じて入力デバイスをブロックするためのクラス
    /// PreUpdate.InputForUIUpdateフェーズのあとでcurrentデバイスをダミーに差し替えてアプリケーション側に入力が届かないようにする
    /// * FixedUpdateはPreUpdateの前なので入力は有効
    /// </summary>
    public static class InputDeviceBlocker
    {
        public static bool Enabled { get; private set; }
        
        public static event Func<bool>　shouldBlockInput;
        
        private static Mouse _dummyMouse;
        private static Pointer _dummyPointer;
        private static Mouse _originalMouse;
        private static Pointer _originalPointer;

        static InputDeviceBlocker()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                SetEnable(false);
                _dummyMouse = null;
                _dummyPointer = null;
            });
        }
        
        public static void SetEnable(bool enable)
        {
            if (Enabled == enable) return;
            
            var enableDeviceTiming = typeof(PreUpdate.InputForUIUpdate);
            var disableDeviceTiming = typeof(PreUpdate.InputForUIUpdate);
            
            PlayerLoopInjector.RemoveActionBefore(enableDeviceTiming, EnableDevice);
            PlayerLoopInjector.RemoveActionAfter(disableDeviceTiming, CheckAndDisableDevice);
            
            if (enable)
            {
                PlayerLoopInjector.AddActionBefore(enableDeviceTiming, EnableDevice);
                PlayerLoopInjector.AddActionAfter(disableDeviceTiming, CheckAndDisableDevice);
            }
            else
            {
                EnableDevice();
            }

            Enabled = enable;
        }

        private static void EnableDevice()
        {
            _originalMouse?.MakeCurrent();
            _originalPointer?.MakeCurrent();
        }
        
        private static void CheckAndDisableDevice()
        {
            var shouldBlock = shouldBlockInput?.GetInvocationList().Cast<Func<bool>>().Any(f => f()) ?? false;
            if (!shouldBlock) return;

            _originalMouse = Mouse.current;
            _originalPointer = Pointer.current;
            
            _dummyMouse ??= InputSystem.AddDevice<Mouse>();
            _dummyPointer ??= InputSystem.AddDevice<Pointer>();
            
            _dummyMouse.MakeCurrent();
            _dummyPointer.MakeCurrent();
        }
    }
}

#endif