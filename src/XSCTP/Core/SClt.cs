using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace XSCTP
{
    public class SClt
    {
        public SClt()
        {
            Pipe pipe = new Pipe();
        }

        /// <summary>
        /// 连接
        /// </summary>
        public ValueTask<bool> ConnectAsync(string ipport)
        {
            return new ValueTask<bool>(true);
        }


    }
}
