using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace XSCTP.Core
{
    public class SSvr
    {
        private async void Run()
        {
            Pipe pipe = new Pipe();
            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);
            await Task.WhenAll(reading, writing);
        }

        //写入循环
        private async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            //数据流量比较大，用8k作为buffer
            const int minimumBufferSize = 1024 * 8;

            while (running)
            {
                try
                {
                    //从writer中，获得一段不少于指定大小的内存空间
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                    await socket.ReceiveAsync(memory, SocketFlags.None);
                    //将内存空间变成ArraySegment，提供给socket使用
                    if (!MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)memory, out ArraySegment<byte> arraySegment))
                    {
                        throw new InvalidOperationException("Buffer backed by array was expected");
                    }
                    //接受数据
                    int bytesRead = await SocketTaskExtensions.ReceiveAsync(socket, arraySegment, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    //一次接受完毕，数据已经在pipe中，告诉pipe已经给它写了多少数据。
                    writer.Advance(bytesRead);
                }
                catch
                {
                    break;
                }

                // 提示reader可以进行读取数据，reader可以继续执行readAsync()方法
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // 告诉pipe完事了
            writer.Complete();
        }

        //读取循环
        private async Task ReadPipeAsync(Socket socket, PipeReader reader)
        {
            while (running)
            {
                //等待writer写数据
                ReadResult result = await reader.ReadAsync();
                //获得内存区域
                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    //寻找head的第一个字节所在的位置
                    position = buffer.PositionOf((byte)0x75);
                    if (position != null)
                    {
                        //由于是连续四个字节作为head，需要进行比对，我这里直接使用了ToArray方法，还是有了内存拷贝动作，不是很理想，但是写起来很方便。
                        //对性能有更高要求的场景，可以进行slice操作后的单独比对，这样不需要内存拷贝动作
                        var headtoCheck = buffer.Slice(position.Value, 4).ToArray();
                        //SequenceEqual需要引用System.Linq
                        MemoryExtensions.SequenceEqual
                        if (headtoCheck.SequenceEqual(new byte[] { 0x75, 0xbd, 0x7e, 0x97 }))
                        {
                            //到这里，认为找到包开头了（从position.value开始），接下来需要从开头处截取整包的长度，需要先判断长度是否足够
                            if (buffer.Slice(position.Value).Length >= 2400)
                            {
                                //长度足够，那么取出ReadOnlySequence，进行操作
                                var mes = buffer.Slice(position.Value, 2400);
                                //这里是数据处理的函数，可以参考官方文档对ReadOnlySequence进行操作，文档里面使用了span，那样性能会好一些。我这里简单实用ToArray()操作，这样也有了内存拷贝的问题，但是处理的直接是byte数组了。
                                await ProcessMessage(mes.ToArray());
                                //这一段就算是完成了，从开头位置，一整个包的长度就算完成了
                                var next = buffer.GetPosition(2400, position.Value);
                                //将buffer处理过的舍弃，替换为剩余的buffer引用
                                buffer = buffer.Slice(next);
                            }
                            else
                            {
                                //长度不够，说明数据包不完整，等下一波数据进来再拼接，跳出循环。
                                break;
                            }
                        }
                        else
                        {
                            //第一个是0x75但是后面不匹配，可能有数据传输问题，那么需要舍弃第一个，0x75后面的字节开始再重新找0x75
                            var next = buffer.GetPosition(1, position.Value);
                            buffer = buffer.Slice(next);
                        }
                    }
                }
                while (position != null);

                //数据处理完毕，告诉pipe还剩下多少数据没有处理（数据包不完整的数据，找不到head）
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }

    }
}
