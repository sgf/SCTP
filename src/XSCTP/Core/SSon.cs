using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace XSCTP-
{
    struct SRemote
    {
        //mulCalli

        /// <summary>
        /// 远程端点（Address&Port）
        /// </summary>
        IPPort end;

        /// <summary>
        /// 流
        /// </summary>
        uint[] Streams;
        uint Id;

        //其余的属性可以放在 特定的列表中 从而仿照ECS架构

        public void Read()
        {
            //Pipe pipe = new Pipe();
            //pipe.Writer.WriteAsync()
        }
    }
}
