using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class CallbackEventHandlerExtension
    {
        public static void RegisterCallbackOnce<TEventType>(this CallbackEventHandler handler,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback<TEventType>(Callback, useTrickleDown);

            void Callback(TEventType evt)
            {
                handler.UnregisterCallback<TEventType>(Callback);
                callback(evt);
            }
        }
        
        public static  void RegisterCallbackUntil<TEventType>(this CallbackEventHandler handler,
            Func<bool> until,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback<TEventType>(Callback, useTrickleDown);

            void Callback(TEventType evt)
            {
                if (until())
                {
                    handler.UnregisterCallback<TEventType>(Callback);
                    return;
                }

                callback(evt);
            }
        }
    }
}