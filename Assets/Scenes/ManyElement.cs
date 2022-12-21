using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RosettaUIRoot))]
public class ManyElement : MonoBehaviour
{
    public int elementCount = 30;
    public bool rebuild;
    
    public List<DiverseClass> _diverseClassList;
    public DiverseClass[] _diverseClassArray;

    private RosettaUIRoot _root;

    private void Start()
    {
        _root = GetComponent<RosettaUIRoot>();
        var window = UI.Window(nameof(ManyElement), CreateElement());

        _root.Build(window);
    }

    private Element CreateElement()
    {
        // return UI.DynamicElementOnTrigger(
        //     _ => rebuild,
        //     () =>
        //     {
        //         rebuild = false;
        //         return CreateHeavyElement();
        //     });

        return CreateHeavyElement();
        

        Element CreateHeavyElement()
        {
            _diverseClassList = Enumerable.Range(0, elementCount)
                .Select(i => new DiverseClass {id = i})
                .ToList();
            
            _diverseClassArray = Enumerable.Range(0, elementCount)
                .Select(i => new DiverseClass {id = i})
                .ToArray();
            
            return UI.List(() => _diverseClassList
            // return UI.List(() => _diverseClassArray
                // ,createItemElement: (binder, idx) => UI.Label(((IBinder<string>)binder).Get())
                // ,createItemElement: (binder, idx) => UI.Fold($"Item{idx}", UI.Field(null, binder))
                , createItemElement: (binder, _) => UI.Field(null, binder)
            ).SetHeight(500f);


            return UI.ScrollViewVertical(500f,
                Enumerable.Range(0, elementCount).Select(i =>
                {
                    var list = new List<int>();
                    return UI.List(i.ToString(), () => list);
                })
            );
        }
    }

    [Serializable]
    public class DiverseClass : IElementCreator
    {
        public int id;
        [Range(0,50)]
        public int value;
        public float height;

        public DiverseClass()
        {
        }

        public DiverseClass(DiverseClass other)
        {
            id = other.id;
            value = other.value;
            height = other.height;
        }

        public Element CreateElement(LabelElement _)
        {
            var labelStr = $"Id[{id}]";

            if (height <= 0f)
                height = (30f + Random.value * 50f);
            
            // Debug.Log($"Id[{id}] height[{_height.Value}] hash:[{GetHashCode()}]");
            
            return (id % 4) switch
            {
                0 => UI.Field(labelStr, () => value).SetHeight(height),
                1 => UI.Slider(labelStr, () => value),
                2 => UI.Fold(labelStr, UI.Field("", () => value)),
                3 => UI.Fold(labelStr, UI.Slider("", () => value)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
