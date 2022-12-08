using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.Assertions;

namespace RosettaUI.Example
{
    public class TestBind : MonoBehaviour
    {
        private List<List<Element>> _elementSets = new();
        public List<Texture> textures;
        
        private void Start()
        {
            var root = GetComponent<RosettaUIRoot>();

            var window = UI.Window(CreateElement());
            root.Build(window);
        }

        private Element CreateElement()
        {
            const int setsCount = 10;
            for (var i = 0; i < setsCount; ++i)
            {
                _elementSets.Add(CreateElements(i).ToList());
            }

            var build =true;
            var bindCount = 0;

            return //UI.ScrollViewVerticalAndHorizontal(1000f, 1200f,
                UI.DynamicElementOnTrigger(
                    _ => build,
                    () =>
                    {
                        build = false;
                        bindCount = 0;

                        for (var i = 0; i < setsCount; ++i)
                        {
                            var enable = i == 0;
                            foreach (var e in _elementSets[i])
                            {
                                e.DetachParent();
                                e.Enable = enable;
                            }
                        }

                        return UI.Column(
                            new[]
                                {
                                    UI.Button("Build", () => build = true),
                                    UI.Button("Bind", () => Bind(++bindCount))
                                }
                                .Concat(_elementSets.SelectMany(sets => sets)) // 今回非表示のElementもBindするとき親が指定されててほしいので非表示状態だけど登録しておく
                        );
                    }
                );
        }

        IEnumerable<Element> CreateElements(int id)
        {
            // return Enumerable.Range(0, 1000).Select(i => UI.Field(() => intValue));

            int intValue = id;
            uint uintValue = (uint)id;
            float floatValue = id * 0.1f;
            string stringValue = id.ToString();
            Color colorValue = Color.HSVToRGB(id / 6f, 1f, 1f);
            bool boolValue = id % 2 == 1;
            Vector2 vector2Field = Vector2.one * id * 0.1f;
            Vector2Int vector2IntField = Vector2Int.one * id;
            int dropDownIdx = 0;

            int intMin = -id;
            int intMax = (id + 1) * 10;
            uint uintMin = (uint)id;
            uint uintMax = (uint)intMax;
            float floatMin = -(id + 1) * 0.1f;
            float floatMax = (id + 1) * 0.1f;

            var popupMenuItems = new[] { "one", "two", "three" }.Select(str =>
                new MenuItem(str, () => Debug.Log($"PopupMenuItem[{str}] clicked at id[{id}]")));

            // return new[]
            // {
            //     UI.Window($"Window[{id}]", CreateGroupContents(id))
            // };

            return new[]
            {
                UI.Fold("Elements",
                    UI.Label($"Label[{id}]"),
                    UI.Field($"{nameof(intValue)}[{id}]", () => intValue),
                    UI.Field($"{nameof(uintValue)}[{id}]", () => uintValue),
                    UI.Field($"{nameof(floatValue)}[{id}]", () => floatValue),
                    UI.Field($"{nameof(stringValue)}[{id}]", () => stringValue),
                    UI.Field($"{nameof(colorValue)}[{id}]", () => colorValue),
                    UI.Field($"{nameof(boolValue)}[{id}]", () => boolValue),
                    UI.Field($"{nameof(vector2Field)}[{id}]", () => vector2Field),
                    UI.Field($"{nameof(vector2IntField)}[{id}]", () => vector2IntField),
                    UI.Slider($"{nameof(intValue)}[{id}]", () => intValue, intMin, intMax),
                    UI.Slider($"{nameof(uintValue)}[{id}]", () => uintValue, uintMin, uintMax),
                    UI.Slider($"{nameof(floatValue)}[{id}]", () => floatValue, floatMin, floatMax),
                    UI.MinMaxSlider($"{nameof(vector2Field)}[{id}]", () => vector2Field,
                        new Vector2(floatMin, floatMax)),
                    UI.MinMaxSlider($"{nameof(vector2IntField)}[{id}]", () => vector2IntField,
                        new Vector2Int(intMin, intMax)),
                    UI.Toggle($"Toggle[{id}]", () => boolValue),
                    UI.HelpBox($"HelpBox[{id}]", (HelpBoxType)(id % 4)),
                    UI.Fold($"Fold[{id}]",
                        UI.Field($"{nameof(intValue)}[{id}]", () => intValue)
                    ),
                    UI.Dropdown($"Dropdown[{id}]", () => dropDownIdx, new[] { "one", "two", "three" }),
                    UI.Button($"Button[{id}]", () => Debug.Log($"On button clicked at id[{id}]")),
                    UI.Popup(UI.Label($"Popup[{id}]"), () => popupMenuItems),
                    UI.Label("Image"),
                    UI.Image(() => textures[id % textures.Count]),
                    UI.Label("Space"),
                    UI.Space().SetWidth((id + 1) * 100f).SetHeight(30f).SetBackgroundColor(Color.gray)
                ).Open(),

                UI.Fold("ElementGroups",
                    UI.Label("Row"),
                    UI.Row(CreateGroupContents(id)),

                    UI.Label("Column"),
                    UI.Column(CreateGroupContents(id)),

                    UI.Label("Box"),
                    UI.Box(CreateGroupContents(id)),

                    UI.Label("Indent"),
                    UI.Indent(CreateGroupContents(id)),

                    UI.Label("Page"),
                    UI.Page(CreateGroupContents(id)),

                    UI.Label("ScrollView"),
                    UI.ScrollViewVertical(200f,
                        Enumerable.Range(0, (id + 1) * 10).Select(i => UI.Field($"Item[{i}]_id[{id}]", () => i))),
                    
                    UI.Tabs(CreateTabContents(id))
                    
                    // UI.Window($"Window[{id}]", CreateGroupContents(id))
                ).Open()
            };

            IEnumerable<Element> CreateGroupContents(int id)
            {
                return (id % 5) switch
                {
                    0 => new[]
                    {
                        UI.Label($"0 Label{id}"),
                        UI.Field($"{nameof(intValue)}[{id}]", () => intValue)
                    },
                    1 => new[]
                    {
                        UI.Label($"1 Label{id}"),
                        UI.Field($"{nameof(intValue)}[{id}]", () => intValue)
                    },
                    2 => new[]
                    {
                        UI.Label($"2 Label{id}"),
                        UI.Field($"{nameof(intValue)}[{id}]", () => intValue),
                        UI.Field($"{nameof(floatValue)}[{id}]", () => floatValue)
                    },
                    3 => new[]
                    {
                        UI.Label($"3 Label{id}"),
                        UI.Field($"{nameof(intValue)}[{id}]", () => intValue)
                    },
                    4 => new[]
                    {
                        UI.Toggle($"4 Toggle[{id}]", () => boolValue)
                    },

                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            
            IEnumerable<(string, Element)> CreateTabContents(int id)
            {
                return Enumerable.Range(0, (id+2))
                    .Select(idx => ($"Tab[{id}-{idx}]", (Element)UI.Column(CreateGroupContents(idx))));
            }
        }

        private void Bind(int bindCount)
        {
            var prevSetIdx = (bindCount-1) % _elementSets.Count;
            var nextSetIdx = bindCount % _elementSets.Count;
            
            for(var i=0; i<_elementSets[prevSetIdx].Count; ++i)
            {
                var prev = _elementSets[prevSetIdx][i];
                var next = _elementSets[nextSetIdx][i];
                
                var ve = UIToolkitBuilder.Instance.GetUIObj(prev);
                // Debug.Log(ve.GetHashCode());

                var success = UIToolkitBuilder.Instance.Bind(next, ve);
                Assert.IsTrue(success, prev.FirstLabel());

                prev.Enable = false;
                next.Enable = true;
            }
        }
    }
}