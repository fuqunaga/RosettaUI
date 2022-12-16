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
                // .Select(i => i.ToString())
                .Select(i => new DiverseClass {id = i})
                .ToList();

            return UI.List(() => _diverseClassList
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
        public int value;
        private float? _height;
        
        public Element CreateElement(LabelElement _)
        {
            var labelStr = $"Id[{id}]";
            _height ??= 30f + Random.value * 50f;
            
            return (id % 4) switch
            {
                0 => UI.Field(labelStr, () => value).SetHeight(_height),
                1 => UI.Slider(labelStr, () => value),
                2 => UI.Fold(labelStr, UI.Field("", () => value)),
                3 => UI.Fold(labelStr, UI.Slider("", () => value)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
