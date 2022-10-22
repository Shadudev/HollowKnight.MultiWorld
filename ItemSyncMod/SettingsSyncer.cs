using Newtonsoft.Json;

namespace ItemSyncMod
{
    public class SettingsSyncer
    {
        public enum SettingKey
        {
            SyncVanillaItems = 0,
            SyncSimpleKeysUsages
        }

        public void SyncSetting(SettingKey key, bool value)
        {
            ItemSyncMod.Connection.SendSettings(JsonConvert.SerializeObject(
                new Dictionary<SettingKey, bool>() { [key] = value }));
        }

        public void SetSettings(string settingsJson)
        {
            LogHelper.Log("Applying received settings: " + settingsJson);
            Dictionary<SettingKey, bool> settings = JsonConvert.DeserializeObject<
                Dictionary<SettingKey, bool>>(settingsJson);

            settings.ToList().ForEach(setting => SetSetting(setting.Key, setting.Value));
        }

        public void SetSetting(SettingKey key, bool value) // Make this generic if ever needed
        {
            switch (key)
            {
                case SettingKey.SyncVanillaItems:
                    MenuHolder.MenuInstance.SetSyncVanillaItems(value);
                    break;
                case SettingKey.SyncSimpleKeysUsages:
                    MenuHolder.MenuInstance.SetSyncSimpleKeysUsages(value);
                    break;
            }
        }

        internal string GetSerializedSettings()
        {
            Dictionary<SettingKey, bool> settings = new() 
            {
                [SettingKey.SyncVanillaItems] = ItemSyncMod.GS.SyncVanillaItems,
                [SettingKey.SyncSimpleKeysUsages] = ItemSyncMod.GS.SyncSimpleKeysUsages,
            };
            return JsonConvert.SerializeObject(settings);
        }
    }
}
