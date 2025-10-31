#if ENABLE_INPUT_SYSTEM

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace RosettaUI
{
    /// <summary>
    /// UIの状態に応じて入力デバイスをブロックするクラス
    /// PreUpdate.InputForUIUpdateフェーズのあとでcurrentデバイスをダミーに差し替えてアプリケーション側に入力が届かないようにする
    /// * FixedUpdateはPreUpdateの前なので入力は有効
    /// </summary>
    public static class InputDeviceBlocker
    {
        public enum Device
        {
            Keyboard,
            Pointer,
            Mouse,
        }
        
        private static readonly Dictionary<Device, List<Func<bool>>> DeviceShouldBlockFuncListTable = new();
        
        private static Keyboard _dummyKeyboard;
        private static Mouse _dummyMouse;
        private static Pointer _dummyPointer;
        
        private static Keyboard _originalKeyboard;
        private static Mouse _originalMouse;
        private static Pointer _originalPointer;

        
        public static bool Enabled { get; private set; }

        
        static InputDeviceBlocker()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                SetEnable(false);
                _dummyMouse = null;
                _dummyPointer = null;
            });
        }
        
        public static void RegisterShouldBlockFuncIfNotYet(Device device, Func<bool> shouldBlockFunc)
        {
            if (shouldBlockFunc == null) return;
            
            if (!DeviceShouldBlockFuncListTable.TryGetValue(device, out var list))
            {
                list = new List<Func<bool>>();
                DeviceShouldBlockFuncListTable[device] = list;
                
                SetEnable(true);
            }
            
            if (list.Contains(shouldBlockFunc)) return;
            
            list.Add(shouldBlockFunc);
        }
        
        public static void UnregisterShouldBlockFunc(Device device, Func<bool> shouldBlockFunc)
        {
            if (shouldBlockFunc == null) return;
            if (!DeviceShouldBlockFuncListTable.TryGetValue(device, out var list)) return;
            
            list.Remove(shouldBlockFunc);
        }

        private static void SetEnable(bool enable)
        {
            if (Enabled == enable) return;
            
            // PreUpdate.InputForUIUpdateの前後のほうがより厳密だがUnity2022にはないのでPreUpdate全体を指定
            var enableDeviceTiming = typeof(PreUpdate);
            var disableDeviceTiming = typeof(PreUpdate);
            
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
            _originalKeyboard?.MakeCurrent();
            _originalKeyboard = null;
            
            _originalMouse?.MakeCurrent();
            _originalMouse = null;
            
            _originalPointer?.MakeCurrent();
            _originalPointer = null;
        }
        
        private static void CheckAndDisableDevice()
        {
            foreach (var (device, funcList) in DeviceShouldBlockFuncListTable)
            {
                var block = funcList.Any(f => f());
                if (!block) continue;
                
                switch (device)
                {
                    case Device.Keyboard:
                        _originalKeyboard = Keyboard.current;
                        
                        _dummyKeyboard ??= InputSystem.AddDevice<Keyboard>();
                        _dummyKeyboard.MakeCurrent();
                        break;
                    
                    case Device.Pointer:
                        _originalPointer = Pointer.current;

                        _dummyPointer ??= InputSystem.AddDevice<Pointer>();
                        _dummyPointer.MakeCurrent();
                        break;
                    
                    case Device.Mouse:
                        _originalMouse = Mouse.current;

                        _dummyMouse ??= InputSystem.AddDevice<Mouse>();
                        _dummyMouse.MakeCurrent();
                        break;


                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}

#endif