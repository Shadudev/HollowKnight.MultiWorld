using MenuChanger.MenuElements;
using UnityEngine.UI;

namespace ItemSyncMod.MenuExtensions
{
    internal class DynamicLabel : MenuLabel
    {
        public DynamicLabel(MenuChanger.MenuPage menuPage, string text, Style style) 
            : base(menuPage, text, style)
        {
            Text.alignment = UnityEngine.TextAnchor.MiddleCenter;
        }

        public void SetText(params string[] text)
        {
            Text.text = Align(text);
        }

        private string Align(string[] text)
        {
            string aligned = text[0];
            int accumulatedLength = aligned.Length;
            for (int i = 1; i < text.Length; i++)
            {
                if (accumulatedLength + text.Length + 2 > 32)
                {
                    aligned += ",\n" + text[i];
                    accumulatedLength = text[i].Length;
                }
                else
                {
                    aligned += ", " + text[i];
                    accumulatedLength += 2 + text[i].Length;
                }
            }

            return aligned;
        }
    }
}
