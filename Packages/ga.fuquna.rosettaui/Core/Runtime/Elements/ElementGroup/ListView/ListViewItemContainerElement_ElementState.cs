using System.Collections.Generic;
using System.Linq;
using RosettaUI.Utilities;

namespace RosettaUI
{
    public partial class ListViewItemContainerElement
    {
        /// <summary>
        /// 別のElementに引き継ぐElementの状態
        /// 現状FoldのOpen/Close情報のみ 
        /// </summary>
        public class ElementState : ObjectPoolItem<ElementState>
        {
            public static ElementState Create(Element element)
            {
                var pooled = GetPooled();
                pooled.Initialize(element);
                return pooled;
            }

            private readonly List<bool> _openList = new();

            private void Initialize(Element element)
            {
                _openList.Clear();
                _openList.AddRange(element.Query<FoldElement>().Select(fold => fold.IsOpen));
            }
            
            public void Apply(Element element)
            {
                if (element == null) return;
                
                foreach (var (fold, isOpen) in element.Query<FoldElement>().Zip(_openList, (f, o) => (f, o)))
                {
                    fold.IsOpen = isOpen;
                }
            }

            public override void Dispose()
            {
                _openList.Clear();
                base.Dispose();
            }
        }


    }
}