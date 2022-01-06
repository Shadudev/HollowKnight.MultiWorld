using MenuChanger;
using MenuChanger.MenuElements;

namespace MultiWorldMod.MenuExtensions
{
    internal class DynamicToggleButton : ToggleButton
    {
        public DynamicToggleButton(MenuPage page, string text) : base(page, text)
        {
            Formatter = new SettableFormatter(text);
        }

        public void SetText(string text)
        {
            (Formatter as SettableFormatter).Text = text;
            base.RefreshText();
        }
    }
}
