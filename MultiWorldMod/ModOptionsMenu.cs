using ItemChanger.Internal.Menu;
using MultiWorldMod.Menu;

namespace MultiWorldMod
{
    internal class ModOptionsMenu
    {
        public static MenuScreen GetMultiWorldMenuScreen(MenuScreen modListMenu, List<Modding.IMenuMod.MenuEntry> entries)
        {
            ModMenuScreenBuilder builder = new("MultiWorld", modListMenu);
            entries.ForEach(builder.AddHorizontalOption);
            SelfEjectButton.AddEjectButton(builder, modListMenu);
            MultiWorldMod.VoteEjectMenuInstance.AddThirdPartyEjectMenu(builder, modListMenu);
            return builder.CreateMenuScreen();
        }
    }
}
