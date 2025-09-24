using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace RosettaUI
{
    public static class PlayerLoopInjector
    {
        private static readonly Dictionary<(Type,bool), Action> Actions = new();

#if UNITY_EDITOR
        static PlayerLoopInjector()
        {
            EditorApplication.playModeStateChanged += change => {
                if (change == PlayModeStateChange.EnteredEditMode)
                    Actions.Clear();
            };
        }
#endif

        public static void AddActionBefore(Type timing, Action action) => AddAction(timing, action, false);

        public static void AddActionAfter(Type timing, Action action) => AddAction(timing, action, true);

        public static void AddAction(Type timing, Action action, bool after)
        {
            var key = (timing, after);
            if (!Actions.ContainsKey(key))
            {
                var loops = PlayerLoop.GetCurrentPlayerLoop();
                var success = AddSubSystem(ref loops, timing, after);
                if (!success)
                {
                    Debug.LogError($"Failed to add action to PlayerLoop. timing:{timing} after:{after}");
                    return;
                }
             
                PlayerLoop.SetPlayerLoop(loops);
                Actions[key] = null;
            }
            
            Actions[key] += action;
        }

        public static bool AddSubSystem(ref PlayerLoopSystem loop, Type timing, bool after)
        {
            if (loop.subSystemList == null)
            {
                return false;
            }
            
            var list = loop.subSystemList.ToList();
            var index = list.FindIndex(s => s.type == timing);
            if (index < 0)
            {
                for (var i = 0; i < list.Count; ++i)
                {
                    var success = AddSubSystem(ref loop.subSystemList[i], timing, after);
                    if (success)
                    {
                        return true;
                    }
                }

                return false;
            }
            
            var key = (timing, after);
            var newPlayerLoop = new PlayerLoopSystem()
            {
                type = typeof(PlayerLoopInjector),
                updateDelegate = () =>
                {
                    if (Actions.TryGetValue(key, out var a))
                        a?.Invoke();
                }
            };

            list.Insert(after ? index + 1 : index, newPlayerLoop);
            
            loop.subSystemList = list.ToArray();
            return true;
        }
        
        
        public static void RemoveActionBefore(Type timing, Action action) => RemoveAction(timing, action, false);
        public static void RemoveActionAfter(Type timing, Action action) => RemoveAction(timing, action, true);

        public static void RemoveAction(Type timing, Action action, bool after) {
            var key = (timing, after);
            if (!Actions.ContainsKey(key)) return;
            
            Actions[key] -= action;
        }
    }
}