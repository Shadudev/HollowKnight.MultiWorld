using ItemChanger.Internal.Menu;


namespace MultiWorldMod
{
    internal class ModOptionsMenu
    {
        public static MenuScreen GetMultiWorldMenuScreen(MenuScreen modListMenu, List<Modding.IMenuMod.MenuEntry> entries)
        {
            ModMenuScreenBuilder builder = new("MultiWorld", modListMenu);
            entries.ForEach(builder.AddHorizontalOption);
            EjectMenuHandler.AddEjectButton(builder, modListMenu);
            return builder.CreateMenuScreen();
        }
    }
}
