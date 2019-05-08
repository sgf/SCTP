using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore
{
    interface Interface1
    {
        //偶联 启动与关闭
        //流内顺序传递 SSN
        //用户数据分片 SCTP通过对传送路径上最大PMTU的检测，实现在SCTP层，将超大用户数据分片打包，避免在IP层的多次分片，重组，可以减少路由器上IP层负担。
        //证实和避免拥塞
        //块绑定 多个用户数据绑定在一个包中传输
        //报文验证 ADLER-16 或CRC32算法 算出一个32位的校验和，带在数据报中，这里使用XXHash替换
        //路径管理 通过心跳，累计重传次数，SCTP将目的地址，端点的可达性好好的管理了起来。

    }
}
