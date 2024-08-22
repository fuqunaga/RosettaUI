using System;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RosettaUI
{
    /// <summary>
    /// static なリソースのリセットが必要なタイミングでコールバックを呼び足すサービス
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
                if (state != PlayModeStateChange.ExitingPlayMode) return;

                onResetStaticResource?.Invoke();
            };
        }
#endif
        
        
        [Conditional("UNITY_EDITOR")]
        public static void AddResetStaticResourceCallback(Action callback)
        {
            onResetStaticResource += callback;
        }
    }
}