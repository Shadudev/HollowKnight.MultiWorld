using ItemSyncMod.Menu;
using MenuChanger.MenuElements;
using MenuChanger;
using ItemChangerDataLoader;

namespace ItemSyncMod.ICDL
{
    internal static class ICDLInterop
    {
        internal static void Hook(Dictionary<MenuPage, ItemSyncMenu> menuInstances)
        {
            ICDLMenuAPI.AddStartGameOverride(
                page => menuInstances[page] = new(page),
                (ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button) =>
                {
                    var menu = menuInstances[landingPage];
                    ItemSyncMod.Controller = new ItemSyncICDLController(data, menu);
                    return menu.GetMenuButton(out button);
                });
        }
    }
}