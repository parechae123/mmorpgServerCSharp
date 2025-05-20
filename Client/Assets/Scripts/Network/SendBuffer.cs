using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ServerCore
{
    /// <summary>
    /// 읽기만 하기 때문에 다른 쓰레드와의 충돌을 일으킬 가능성 없음
    /// </summary>
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer  = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535*100;

        
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value == null)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            if(CurrentBuffer.Value.FreeSize < reserveSize)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SendBuffer
    {
        //[u][][][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }
        /// <summary>
        /// 공간을 체크하여 사용할 수 있는 공간을 리턴
        /// </summary>
        /// <param name="reserveSize"> 사용 할 공간을 기입</param>
        /// <returns></returns>
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;

            //버퍼에 남은 공간이 있으면 사용할 수 있는 공간을 리턴
            return new ArraySegment<byte>(_buffer,_usedSize,reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
