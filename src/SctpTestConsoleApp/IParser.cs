using System;
using System.Buffers;

namespace SctpTestConsoleApp
{
    public interface IParser
    {
        bool TryParse<TPack>(ReadOnlySequence<byte> buff, out TPack pack, out SequencePosition consumedTo) where TPack : IPack;
    }



    public interface IPack
    {


    }


    public class SctpPacker : IParser
    {
        public bool TryParse<TPack>(ReadOnlySequence<byte> buff, out TPack pack, out SequencePosition consumedTo) where TPack : IPack
        {
            pack = default;
            consumedTo = default;
            //buff.PositionOf()
            return true;
        }


    }

}