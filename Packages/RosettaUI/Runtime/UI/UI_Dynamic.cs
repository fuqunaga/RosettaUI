using System;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RosettaUI
{
    public static partial class UI
    {
        #region DynamicElement

        public static DynamicElement DynamicElementIf(Func<bool> condition, Func<Element> build)
        {
            return DynamicElementOnStatusChanged(
                condition,
                flag => flag ? build() : null
            );
        }

        public static DynamicElement DynamicElementOnStatusChanged<T>(Func<T> readStatus, Func<T, Element> build)
        {
            return DynamicElement.Create(readStatus, build);
        }

        public static DynamicElement DynamicElementOnTrigger(Func<DynamicElement, bool> rebuildIf, Func<Element> build)
        {
            return new DynamicElement(build, rebuildIf);
        }

        #endregion

        
        #region FindObject

        public static DynamicElement FieldIfObjectFound<T>()
            where T : Behaviour
        {
            return FieldIfObjectFound(typeof(T));
        }

        public static DynamicElement FieldIfObjectFound(Type type)
        {
            return DynamicElementIfObjectFound(type, obj =>
            {
                var binder = Binder.Create(obj, type);
                return Field(null, binder);
            });
        }

        
        public static DynamicElement DynamicElementIfObjectFound<T>(Func<T, Element> build)
            where T : Object
        {
            return DynamicElementIfObjectFound(typeof(T), (o) => build?.Invoke((T) o));
        }

        public static DynamicElement DynamicElementIfObjectFound(Type type, Func<Object, Element> build)
        {
            Assert.IsTrue(typeof(Object).IsAssignableFrom(type));

            Object target = null;
            var lastCheckTime = 0f;
            // 起動時に多くのFindObjectObserverElementが呼ばれるとFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす
            var interval = Random.Range(1f, 1.5f);

            return DynamicElementOnStatusChanged(
                readStatus: () =>
                {
                    if (target == null)
                    {
                        var t = Time.realtimeSinceStartup;
                        if (t - lastCheckTime > interval)
                        {
                            lastCheckTime = t;
                            target = Object.FindObjectOfType(type);
                        }
                    }

                    //return target is Behaviour {isActiveAndEnabled: false} ? null : target;
                    return target;
                },
                build: tgt => tgt != null ? build?.Invoke(tgt) : null);
        }

        #endregion
    }
}