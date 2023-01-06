namespace MultiWorldLib
{
    public class Consts
    {

#if (DEBUG)
        public const int DEFAULT_PORT = 38283;
#else
        public const int DEFAULT_PORT = 38281;
#endif
        public const string PUBLIC_SERVER_URL = "18.189.16.129";

        public const int SERVER_GENERIC_ITEM_ID = -4196;
        public const int TO_ALL_MAGIC = -2;

        public const string ITEMSYNC_ITEM_MESSAGE_LABEL = "ItemSync-Item";
        public const string MULTIWORLD_ITEM_MESSAGE_LABEL = "MultiWorld-Item";

        public const int DEFAULT_TTL = 30;
    }
}
