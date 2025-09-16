using System;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RosettaUI
{
    /// <summary>
    /// static なリソースのリセットが必要なタイミングでコールバックを呼び出すサービス
    /// 現状、PlayModeを抜けた際にDomainReloadがなくてもテクスチャなどは解放されるっぽい
    /// </summary>
    public static class StaticResourceUtility
    {
#if UNITY_EDITOR
        public static event Action onResetStaticResource;
        
        static StaticResourceUtility()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                switch (state)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        onResetStaticResource?.Invoke();
                        break;
                    
                    case PlayModeStateChange.EnteredEditMode:
                    case PlayModeStateChange.EnteredPlayMode:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            };
        }
#endif
        
        
        [Conditional("UNITY_EDITOR")]
        public static void AddResetStaticResourceCallback(Action callback)
        {
#if UNITY_EDITOR
            onResetStaticResource += callback;
#endif
        }
    }
}