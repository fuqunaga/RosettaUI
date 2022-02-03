using System;
using System.Collections.Generic;
using System.Linq;
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

        public static DynamicElement DynamicElementOnTrigger(Func<DynamicElement, bool> rebuildIf, Func<Element> build) => new(build, rebuildIf);

        #endregion

        
        #region FindObject

        public static DynamicElement FieldIfObjectFound<T>()
            where T : Object
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

        public static DynamicElement DynamicElementIfObjectFound(Type type, Func<Object, Element> build,
            bool supportMultiple = false, bool includeInactive = false)
        {
            Assert.IsTrue(typeof(Object).IsAssignableFrom(type));
            if (build == null) return null;

            List<Object> targets = null;
            var lastCheckTime = 0f;
            // 起動時などに多くのFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす
            var intervalMinMax = UISettings.dynamicElementIfObjectFoundInterval;
            var interval = Random.Range(intervalMinMax.min, intervalMinMax.max);

            Func<Element> buildFunc = supportMultiple ? BuildMultipleTarget : BuildFirstTarget;
  
            return DynamicElementOnStatusChanged(
                readStatus: () =>
                {
                    targets?.RemoveAll(t => t == null);
                    
                    var time = Time.realtimeSinceStartup;
                    if (time - lastCheckTime > interval)
                    {
                        lastCheckTime = time;
                        if (targets == null || !targets.Any() || supportMultiple)
                        {
                            targets = Object.FindObjectsOfType(type, includeInactive).ToList();
                        }
                    }

                    var hash =  targets?.Aggregate(0, (hash, t) => hash ^ t.GetHashCode());
                    return hash;
                },
                build: _ => buildFunc()
            );

            
            
            Element BuildFirstTarget()
            {
                var target = targets?.FirstOrDefault();
                return target != null ? build(target) : null;
            }
            
            Element BuildMultipleTarget()
            {
                if (targets == null || !targets.Any()) return null;

                var options = targets.Select(obj => obj.name);
                var contents = targets.Select(build).ToList();

                var currentIdx = 0;
                
                return Column(
                    Row(Label(type.Name),
                        Space(),
                        Dropdown(() => currentIdx, options)
                            .RegisterValueChangeCallback(() =>
                            {
                                for (var i = 0; i < contents.Count; ++i)
                                {
                                    contents[i].Enable = i == currentIdx;
                                }
                            })
                    ),
                    Indent(contents)
                );
            }
        }
        #endregion
    }
}