namespace ItemSyncMod.Items
{
    internal class SimpleKeysUsages
    {
        private static readonly string SIMPLE_KEY_USAGE_MAGIC = "SIMPLE_KEY_USED_";
        private enum SimpleKeyUsageLocation
        {
            Waterways = 0,
            Jiji,
            PleasureHouse,
            Godhome,
        }

        internal static void Hook()
        {
            // TODO add hook to determine when keys are used
            // Can either be by FSM (dirty) or by variable change (is it possible? can we easily determine what door what opened?)
            ItemManager.OnItemReceived += HandleSimpleKeyUsageEvent;
            Modding.ModHooks.SetPlayerVariableHook += SyncSimpleKeysRelatedVariables;
        }

        

        internal static void Unhook()
        {
            ItemManager.OnItemReceived -= HandleSimpleKeyUsageEvent;
            Modding.ModHooks.SetPlayerVariableHook -= SyncSimpleKeysRelatedVariables;
        }

        // TODO add logic for door opening via variables change (remove simple key and update door opened)
        // Make sure this change isn't caught by our own hooks and transmits another meesage
        // To also support the above statement, always check if the door is already open on the client side
        // Later version: Go through FSMs if the player is at the room to remove the fsm action that deducts a key and animate the door opening
        // Race; how do you treat two people running the same dialogue?
        private static void HandleSimpleKeyUsageEvent(ItemManager.ItemReceivedEvent itemReceivedEvent)
        {
            if (string.IsNullOrEmpty(itemReceivedEvent.ItemId) || !itemReceivedEvent.ItemId.StartsWith(SIMPLE_KEY_USAGE_MAGIC)) return;

            switch ((SimpleKeyUsageLocation)int.Parse(itemReceivedEvent.ItemId.Substring(SIMPLE_KEY_USAGE_MAGIC.Length)))
            {
                case SimpleKeyUsageLocation.Waterways:
                    if (!PlayerData.instance.openedWaterwaysManhole)
                    {
                        PlayerData.instance.openedWaterwaysManhole = true;
                        // TODO door unlocking animation if current scene matches
                        PlayerData.instance.simpleKeys--;
                    }
                    break;
                case SimpleKeyUsageLocation.Jiji:
                    if (!PlayerData.instance.jijiDoorUnlocked)
                    {
                        PlayerData.instance.jijiDoorUnlocked = true;
                        // TODO door unlocking animation if current scene matches
                        PlayerData.instance.simpleKeys--;
                    }
                    break;
                case SimpleKeyUsageLocation.PleasureHouse:
                    if (!PlayerData.instance.bathHouseOpened)
                    {
                        PlayerData.instance.bathHouseOpened = true;
                        // TODO door unlocking animation if current scene matches
                        PlayerData.instance.simpleKeys--;
                    }
                    break;
                case SimpleKeyUsageLocation.Godhome:
                    // Just drain a key, items from godhome location should be received on a separate message
                    // Yes, this is a race.
                    PlayerData.instance.simpleKeys--;
                    break;
            }
        }

        private static object SyncSimpleKeysRelatedVariables(Type type, string name, object value)
        {
            if (type == typeof(bool) && (bool)value)
            {
                // If relevant, key has already been deducted locally so just sync to others
                switch (name)
                {
                    case "openedWaterwaysManhole":
                        ItemSyncMod.Connection.SendItemToAll(SIMPLE_KEY_USAGE_MAGIC + SimpleKeyUsageLocation.Waterways);
                        break;
                    case "jijiDoorUnlocked":
                        ItemSyncMod.Connection.SendItemToAll(SIMPLE_KEY_USAGE_MAGIC + SimpleKeyUsageLocation.Jiji);
                        break;
                    case "bathHouseOpened":
                        ItemSyncMod.Connection.SendItemToAll(SIMPLE_KEY_USAGE_MAGIC + SimpleKeyUsageLocation.PleasureHouse);
                        break;
                    case "godseekerUnlocked":
                        ItemSyncMod.Connection.SendItemToAll(SIMPLE_KEY_USAGE_MAGIC + SimpleKeyUsageLocation.Godhome);
                        break;
                }
            }

            return value;
        }
    }
}
