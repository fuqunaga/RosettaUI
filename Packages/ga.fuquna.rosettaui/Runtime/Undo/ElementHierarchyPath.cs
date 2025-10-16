using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RosettaUI.Undo
{
    /// <summary>
    /// Undo/Redo用のElement階層上の位置情報
    ///
    /// 保存されたElementがすでに削除済みだが同じ階層構造位置に同じ型のElementがある場合はUndoできるようにする
    /// 主にListで要素の値を変更し要素削除されたあとUndoを想定しており、
    /// １回目のUndoで要素を追加した際Elementが再生成されてしまい次のUndoで値を元に戻すことができなくなってしまう問題を解決する
    /// </summary>
    public class ElementHierarchyPath
    {
        private readonly struct ElementAndIndex
        {
            public readonly Element element;
            public readonly int childIndex; // 親のChildrenにおけるインデックス。ルートの場合は-1
            
            public ElementAndIndex(Element element, int childIndex)
            {
                this.element = element;
                this.childIndex = childIndex;
            }

            
            public Element GetChild() => GetChild(element);
            
            public Element GetChild(Element e)
            {
                if (e?.Children == null) return null;
                if (childIndex < 0 || childIndex >= e.Children.Count) return null;

                return e.Children[childIndex];
            }
            
            [SuppressMessage("ReSharper", "ParameterHidesMember")]
            public void Deconstruct(out Element element, out int childIndex)
            {
                element = this.element;
                childIndex = this.childIndex;
            }
        }
        
        // ルートからターゲットまでのElementとその子インデックスのペア
        private readonly List<ElementAndIndex> _elementAndIndices = new();

        public Element TargetElement => _elementAndIndices.Last().element;
        
        public void Clear()
        {
            _elementAndIndices.Clear();
        }
        
        public void Initialize(Element element)
        {
            Clear();
            if (element == null) return;
            
            _elementAndIndices.Add(new ElementAndIndex(element, -1));
            
            for(var e = element; e.Parent != null; e = e.Parent)
            {
                var parent = e.Parent;
                var childIndex = ((List<Element>)parent.Children).IndexOf(e);
                _elementAndIndices.Add(new ElementAndIndex(parent, childIndex));
            }

            _elementAndIndices.Reverse();
        }
        
        /// <summary>
        /// 記録した位置に現在存在するElementを取得する
        /// 存在しない、または型が異なる場合はfalseを返す
        /// </summary>
        public bool TryGetExistingElement(out Element targetElement)
        {
            targetElement = null;
            if (_elementAndIndices.Count == 0) return false;
            
            // 保存されているElementが有効ならそのまま返す
            var lastElement = _elementAndIndices.Last().element;
            if (lastElement != null && lastElement.EnableInHierarchy())
            {
                targetElement = lastElement;
                return true;
            }
            
            // パスの最初が有効なルートじゃなかったら失敗
            var firstElement = _elementAndIndices.First().element;
            if (!firstElement.Enable || !RosettaUIRoot.IsRootElement(firstElement))
            {
                return false;
            }

            // デタッチされているかEnableではないElementのIndexを調べる
            var removedIndex = -1;
            for (var i = 1; i < _elementAndIndices.Count; i++)
            {
                var (element, _) = _elementAndIndices[i];
                if (!element.Enable || element.Parent == null)
                {
                    removedIndex = i;
                    break;
                }
            }

            if (removedIndex <= 0)
            {
                return false;
            }
            
            // 存在するエレメントから辿って現在のElementと記録されたElementの型が一致するか確認しながら辿る
            var existingElement = _elementAndIndices[removedIndex - 1].GetChild();
            
            for (var index = removedIndex; index < _elementAndIndices.Count; index++)
            {
                if (existingElement is not { Enable: true })
                {
                    return false;
                }
                
                var elementAndIndex = _elementAndIndices[index];
                if (existingElement.GetType() != elementAndIndex.element.GetType())
                {
                    return false;
                }

                if (index < _elementAndIndices.Count - 1)
                {
                    existingElement =　elementAndIndex.GetChild(existingElement);
                }
            }

            targetElement = existingElement;
            return targetElement != null;
        }
    }
}