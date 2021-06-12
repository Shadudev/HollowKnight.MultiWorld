using System.IO;
using MultiWorldLib.Messaging.Definitions;

namespace MultiWorldLib.Messaging
{
    public interface IMWMessageEncoder
    {
        void Encode(BinaryWriter dataStream, IMWMessageProperty definition, MWMessage message);
        void Decode(BinaryReader dataStream, IMWMessageProperty definition, MWMessage message);
    }
}
