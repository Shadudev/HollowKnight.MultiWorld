using MenuChanger.MenuElements;

namespace MultiWorldMod.MenuExtensions
{
    internal class SettableFormatter : MenuItemFormatter
    {
        public string Text { get; set; }

        public SettableFormatter(string text)
        {
            Text = text;
        }

        public override string GetText(string prefix, object value)
        {
            return Text;
        }
    }
}
