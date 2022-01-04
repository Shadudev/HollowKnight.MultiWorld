using MenuChanger.MenuElements;
using UnityEngine.UI;

namespace MultiWorldMod.MenuExtensions
{
    internal class DynamicLabel : MenuLabel
    {
        public DynamicLabel(MenuChanger.MenuPage menuPage, string text, Style style) : base(menuPage, text, style) { }

        public void SetText(string text)
        {
            GameObject.GetComponent<Text>().text = text;
        }
    }
}
