namespace MultiWorldLib
{
    public class Consts
    {

#if (DEBUG)
        public static readonly int DEFAULT_PORT = 38283;
#else
        public static readonly int DEFAULT_PORT = 38282;
#endif
        public static readonly string PUBLIC_SERVER_URL = "18.189.16.129";

        public static readonly int SERVER_GENERIC_ITEM_ID = -4196;
        public static readonly int TO_ALL_MAGIC = -2;
    }
}
