using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Comugi.UGUI.Builder
{
    [CreateAssetMenu(menuName = "UIIMirage/Create UGUISettings", order = 1000)]
    public class UGUISettings : ScriptableObject
    {
        public int fontSize = 14;

        public int paddingTopLevel = 8;
        public int paddingFoldIconLeft = 14;
        public int paddingIndent = 14;

        public int rowSpacing = 4;

        public Theme theme;
    }

    [System.Serializable]
    public class Theme
    {
        public Color labelColor;
        public Color textColor;
        public float textAlphaOnDisable = 0.7f;
        public Color foldArrowColor;

        [Header("Field")]
        [FormerlySerializedAs("colorBlock")]
        public ColorBlock fieldColors;
        [Tooltip("selection text background color on InputField")]
        public Color selectionColor;

        [Header("Slider")]
        public ColorBlock sliderColors;
        public Color sliderBackgroundColor;
        public Color sliderFillColor;

        [Header("Button")]
        public ColorBlock buttonColors;

        [Header("Dropdown")]
        public ColorBlock dropdownColors;

    }
}