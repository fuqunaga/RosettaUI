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

        public static DynamicElement FieldIfObjectFound<T>(bool supportMultiple = false, bool includeInactive = false)
            where T : Object
        {
            return FieldIfObjectFound(typeof(T), supportMultiple, includeInactive);
        }

        public static DynamicElement FieldIfObjectFound(Type type,  bool supportMultiple = false, bool includeInactive = false)
        {
            return DynamicElementIfObjectFound(type, obj =>
            {
                var binder = Binder.Create(obj, type);
                return Field(null, binder);
            }, supportMultiple, includeInactive);
        }

        
        public static DynamicElement DynamicElementIfObjectFound<T>(Func<T, Element> build,  bool supportMultiple = false, bool includeInactive = false)
            where T : Object
        {
            return DynamicElementIfObjectFound(typeof(T), (o) => build?.Invoke((T) o), supportMultiple, includeInactive);
        }

        public static DynamicElement DynamicElementIfObjectFound(Type type, Func<Object, Element> build,
            bool supportMultiple = false, bool includeInactive = false)
        {
            Assert.IsTrue(typeof(Object).IsAssignableFrom(type));
            if (build == null) return null;

            var targets = new List<Object>();
            var lastCheckTime = 0f;
            // 起動時などに多くのFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす
            var intervalMinMax = UISettings.dynamicElementIfObjectFoundInterval;
            var interval = Random.Range(intervalMinMax.min, intervalMinMax.max);

            var currentObjectHash = 0;
            Func<Element> buildFunc = supportMultiple ? BuildMultipleTarget : BuildFirstTarget;
            
  
            return DynamicElementOnStatusChanged(
                readStatus: () =>
                {
                    var time = Time.realtimeSinceStartup;
                    if (time - lastCheckTime > interval)
                    {
                        lastCheckTime = time;
                        if (!targets.Any() || supportMultiple)
                        {
                            targets.Clear();
                            targets.AddRange(
                                // includeInactive==false でも Behaviour.enabled==falseなものは含まれるので注意
                                Object.FindObjectsOfType(type, includeInactive)
                                    .OrderBy(o => o.name)
                            );

                        }
                    }
                    
                    targets.RemoveAll(t => t == null || (!includeInactive && t is Behaviour {isActiveAndEnabled: false}));
                    
                    var hash =  targets.Aggregate(0, (hash, t) => hash ^ t.GetHashCode());
                    return hash;
                },
                build: _ => buildFunc()
            );

            
            
            Element BuildFirstTarget()
            {
                var target = targets.FirstOrDefault();
                return target != null ? build(target) : null;
            }
            
            Element BuildMultipleTarget()
            {
                if (!targets.Any()) return null;

                var options = targets.Select(obj => obj.name).ToList();
                var contents = targets.Select(build).ToList();

                var currentIdx = 0;
                if (currentObjectHash != 0)
                {
                    var hash = currentObjectHash;
                    var idx = targets.FindIndex(t => t.GetHashCode() == hash);
                    if (idx >= 0)
                    {
                        currentIdx = idx;
                    }
                    else
                    {
                        currentObjectHash = 0;
                    }
                }

                UpdateContentsEnable();

                return Column(
                    Row(Label(type.Name),
                        Space(),
                        Dropdown(null, () => currentIdx, options)
                            .RegisterValueChangeCallback(UpdateContentsEnable)
                    ),
                    Indent(contents)
                );

                void UpdateContentsEnable()
                {
                    for (var i = 0; i < contents.Count; ++i)
                    {
                        contents[i].Enable = i == currentIdx;
                    }

                    currentObjectHash = targets[currentIdx].GetHashCode();
                }
            }
        }
        #endregion
    }
}