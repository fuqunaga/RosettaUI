using System;
using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// 動的に内容が変化するElement
    /// UI実装側はbuildの結果をいれるプレースホルダー的な役割
    /// </summary>
    public class DynamicElement : ElementGroup
    {
        public static DynamicElement Create<T>(Func<T> readStatus, Func<T, Element> buildWithStatus, string displayName = null)
            where T : IEquatable<T>
        {
            T status = readStatus();

            return new DynamicElement(
                build: () => buildWithStatus(status),
                rebuildIf: (e) =>
                {
                    var newStatus = readStatus();
                    var rebuild = !newStatus.Equals(status);
                    if (rebuild)
                    {
                        status = newStatus;
                    }
                    return rebuild;
                },
                displayName
                );
        }

        readonly Func<Element> build;
        readonly Func<DynamicElement, bool> rebuildIf;
        readonly string _displayName;

        public override string DisplayName => string.IsNullOrEmpty(_displayName) ? base.DisplayName : _displayName;

        public Element element => elements?.FirstOrDefault();


        public DynamicElement(Func<Element> build, Func<DynamicElement, bool> rebuildIf, string displayName = null)
        {
            this.build = build;
            this.rebuildIf = rebuildIf;
            this._displayName = displayName;

            BuildElement();
        }

        void BuildElement()
        {
            SetElements(new[] { build?.Invoke() });
        }

        public override void Update()
        {
            if (rebuildIf?.Invoke(this) ?? false)
            {
                element?.Destroy();
                BuildElement();
                RebuildChildren();
            }
            else
            {
                if (Enable) UpdateInternal();
            }
        }
    }
}