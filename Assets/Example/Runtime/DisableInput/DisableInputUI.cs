using System.Linq;
using UnityEngine;

namespace RosettaUI.Examples
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class DisableInputUI : MonoBehaviour
    {
        [Multiline]
        public string typingTest;

        public float dragTest;
        
        private void Start()
        {
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement(root));
        }

        private Element CreateElement(RosettaUIRoot rosettaUIRoot)
        {
            return UI.Window(nameof(DisableInputUI),
                UI.Page(
#if ENABLE_INPUT_SYSTEM
                    UI.HelpBox("Disables in-game input while operating the UI.", HelpBoxType.Info),
                    UI.Box(
                        UI.Toggle(() => rosettaUIRoot.disableKeyboardInputWhileUITyping),
                        UI.Toggle(() => rosettaUIRoot.disablePointerInputOverUI),
                        UI.Toggle(() => rosettaUIRoot.disableMouseInputOverUI)
                    ),
                    UI.Space().SetHeight(20f),
                    UI.TextArea(() => typingTest),
                    UI.Slider(() => dragTest),
                    UI.ScrollViewVertical(200f,
                        Enumerable.Range(0, 100).Select(i => UI.Label($"Mouse Scroll Test {i}"))
                    ).SetBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
#else
                    UI.HelpBox("This feature is only available when using the New Input System.", HelpBoxType.Warning)
#endif
                )
            ).SetClosable(false);
        }
    }
}