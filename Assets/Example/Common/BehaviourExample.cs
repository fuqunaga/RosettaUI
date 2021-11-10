using UnityEngine;


namespace RosettaUI.Example
{
    public class BehaviourExample : MonoBehaviour
    {
        public string classNameIs = nameof(BehaviourExample);
        public int intValue;
        [SerializeField]
        protected int protectedValue;
    }
}