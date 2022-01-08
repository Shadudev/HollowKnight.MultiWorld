using MenuChanger.MenuElements;

namespace ItemSyncMod.MenuExtensions
{
    internal class SimpleToggleButtonFormatter : MenuItemFormatter
    {
        private readonly string trueText, falseText;

        public SimpleToggleButtonFormatter(string trueText, string falseText)
        {
            this.trueText = trueText;
            this.falseText = falseText;
        }

        public override string GetText(string prefix, object value)
        {
            return (bool) value ? trueText : falseText;
        }
    }
}
