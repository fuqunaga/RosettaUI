using UnityEngine;

namespace RosettaUI
{
    public class ImageElement : ReadOnlyValueElement<Texture>
    {
        public ImageElement(IGetter<Texture> getter) : base(getter)
        {
        }
    }
}