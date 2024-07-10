using UnityEngine;


namespace RosettaUI.UGUI.Builder
{
    [CreateAssetMenu(menuName = "UIIMirage/Create UGUIResource", order =1000)]
    public class UGUIResource : ScriptableObject
    {
        public GameObject window;
        public GameObject panel;
        public GameObject label;
        public GameObject fixedSizeLabel;
        public GameObject button;
        public GameObject inputField;
        public GameObject toggle;
        public GameObject dropdown;
        public GameObject slider;
        public GameObject fold;
    }
}