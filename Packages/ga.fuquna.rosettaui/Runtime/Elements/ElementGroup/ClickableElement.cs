using System;
using UnityEngine;

namespace RosettaUI
{
    public interface IClickEvent
    {
        int Button { get; }
        Vector2 Position { get; }
    }

    public class ClickableElement : ElementGroup
    {
        private readonly Action<IClickEvent> _onClick;

        public ClickableElement(Element element, Action<IClickEvent> onClick) : base(new[] { element })
        {
            _onClick = onClick;
        }
        
        public void OnClick(IClickEvent clickEvent) => _onClick?.Invoke(clickEvent);
    }
}