using Newtonsoft.Json;

namespace ItemSyncMod.Menu
{
    /// <summary>
    /// Providing API for sharing settings with other players in a readied room.
    /// </summary>
    public class SettingsSharer
    {
        #region Simplified Shared Settings
        public delegate string GetValue();
        public delegate void SetValue(string value);
        public delegate void OnValueChanged();
        /// <summary>
        /// Register a setting to be shared with all players in a room. 
        /// </summary>
        /// <param name="settingID">Unique name for the setting</param>
        /// <param name="getCallback">Function that returns the current setting value</param>
        /// <param name="setCallback">Function that can set the setting value</param>
        /// <returns>A callback to be invoked when the setting value is changed</returns>
        public OnValueChanged RegisterSharedSetting(string settingID, GetValue getCallback, SetValue setCallback)
        {
            void changedCallback() => ShareChangedSetting(settingID, getCallback());

            settingsCallbacks[settingID] = new()
            {
                getCallback = getCallback,
                setCallback = setCallback,
                changedCallback = changedCallback
            };

            return changedCallback;
        }

        /// <summary>
        /// Unregister a setting from being shared.
        /// </summary>
        /// <param name="settingID"></param>
        /// <returns>If a setting with the given ID was removed</returns>
        public bool UnregisterSharedSetting(string settingID) => settingsCallbacks.Remove(settingID);

        private void ShareChangedSetting(string key, string value)
        {
            // Prevents sharing setting when ValueChanged is invoked after setting was unregistered.
            if (settingsCallbacks.ContainsKey(key))
                ShareSetting(key, value);
        }
        #endregion

        #region Custom Shared Settings
        public delegate void SettingReceived(string key, string value);
        /// <summary>
        /// Invoked when a setting was received. Value has to be deserialized in the same manner it is serialized when sent.
        /// Register to this event if you wish to listen to keys in a more complex manner.
        /// </summary>
        /// <param name="key">Received setting key</param>
        /// <param name="value">Received setting serialized value</param>
        public event SettingReceived OnSettingReceived;

        public delegate void SettingsRequested(Dictionary<string, string> settings);
        /// <summary>
        /// Invoked when settings are to be sent. Register to this event if you wish to add keys in a more complex manner.
        /// </summary>
        /// <param name="settings">Settings dictionary to be updated by each callback</param>
        public event SettingsRequested OnSettingsRequested;
        #endregion

        /// <summary>
        /// Broadcast a setting with a given value to everyone in the room.
        /// The setting won't be sent if the player is not ready or when the setting was updated due to being received.
        /// </summary>
        public void ShareSetting(string key, string value)
        {
            if (!MenuHolder.MenuInstance.IsReadied || CurrentlyUpdatingKey == key) return;

            ItemSyncMod.Connection.SendSettings(JsonConvert.SerializeObject(
                new Dictionary<string, string>() { [key] = value }));
        }

        record struct SettingCallbacks
        {
            public GetValue getCallback;
            public SetValue setCallback;
            public OnValueChanged changedCallback;
        }
        private readonly Dictionary<string, SettingCallbacks> settingsCallbacks;
        private string CurrentlyUpdatingKey;

        internal SettingsSharer()
        {
            settingsCallbacks = new();
            CurrentlyUpdatingKey = string.Empty;

            OnSettingsRequested = FillRegisteredSettings;
            OnSettingReceived = UpdateReceivedSetting;
        }

        internal void BroadcastReceivedSettings(string settingsJson)
        {
            Dictionary<string, string> settings = JsonConvert.DeserializeObject<
                Dictionary<string, string>>(settingsJson);

            settings.ToList().ForEach(setting => OnSettingReceived(setting.Key, setting.Value));
        }

        internal string GetSerializedSettings()
        {
            Dictionary<string, string> settings = new();
            OnSettingsRequested(settings);
            return JsonConvert.SerializeObject(settings);
        }

        private void FillRegisteredSettings(Dictionary<string, string> settings)
        {
            foreach (string key in settingsCallbacks.Keys)
                settings[key] = settingsCallbacks[key].getCallback();
        }

        private void UpdateReceivedSetting(string key, string value)
        {
            if (settingsCallbacks.ContainsKey(key))
            {
                CurrentlyUpdatingKey = key;
                settingsCallbacks[key].setCallback(value);
                CurrentlyUpdatingKey = string.Empty;
            }
        }
    }
}
