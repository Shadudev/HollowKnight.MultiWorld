using ItemSyncMod.Menu;
using MenuChanger.MenuElements;
using MenuChanger;
using ItemChangerDataLoader;

namespace ItemSyncMod.ICDL
{
    internal static class ICDLInterop
    {
        internal static void Hook(List<ItemSyncMenu> menuHolder)
        {
            ICDLMenuAPI.AddStartGameOverride(
                page => menuHolder.Add(new(page)),
                (ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button) =>
                {
                    ItemSyncMod.Controller = new ItemSyncICDLController(data, menuHolder[0]);
                    return menuHolder[0].GetMenuButton(out button);
                });
        }
    }
}