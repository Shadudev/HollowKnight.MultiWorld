using MenuChanger;
using MenuChanger.MenuElements;

namespace ItemSyncMod.MenuExtensions
{
    internal class LockableEntryField<T> : EntryField<T>, ILockable
    {
        public LockableEntryField(MenuPage page, string label, MenuLabel.Style style = MenuLabel.Style.Title) : base(page, label, style) { }

        public bool Locked { get; protected set; } = false;

        public void Lock()
        {
            Locked = true;
            InputField.readOnly = true;
        }

        public void Unlock()
        {
            Locked = false;
            InputField.readOnly = false;
        }
    }
}
