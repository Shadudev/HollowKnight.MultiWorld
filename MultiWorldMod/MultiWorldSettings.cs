using SereCore;

namespace MultiWorld
{
    public class MultiWorldSettings : BaseSettings
	{
        public string URL
        {
            get => GetString("127.0.0.1");
            set => SetString(value);
        }

        public int LastReadyID
        {
            get => GetInt(-1);
            set => SetInt(value);
        }

        public int Port
        {
            get => GetInt(38281);
            set => SetInt(value);
        }

        public string UserName
        {
            get => GetString("WhoAmI");
            set => SetString(value);
        }

        public string Token
        {
            get => GetString("");
            set => SetString(value);
        }
    }
}
