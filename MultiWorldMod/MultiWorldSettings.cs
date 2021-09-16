using SereCore;

namespace MultiWorldMod
{
    public class MultiWorldSettings : BaseSettings
	{
        public string URL
        {
            get => GetString("18.189.16.129");
            set => SetString(value);
        }

        public int LastReadyID
        {
            get => GetInt(-1);
            set => SetInt(value);
        }

        public string UserName
        {
            get => GetString("WhoAmI");
            set => SetString(value);
        }
    }
}
