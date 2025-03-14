using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServerCore
{
    /// <summary>
    /// 누락된 패킷을 조립하는 예제
    /// </summary>
    class ReceiveBuffer
    {
        //[R] [] [] [] [] [W] [] [] [] []
        //버퍼에 기록 된
        ArraySegment<byte> _buffer;
        //읽는 위치, 마우스 커서라고 생각
        int _readPos;
        //CMD에서 깜박거리는 위치,입력위치
        int _writePos;
        public ReceiveBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize]);
        }
        //새로받은 패킷의 크기를 리턴
        public int DataSize { get { return _writePos - _readPos; } }
        //패킷이 받을 수 있는 여유공간을 리턴
        public int FreeSize { get { return _buffer.Count - _writePos; } }
        //데이터의 유효범위
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        //
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }
        
        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                //남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                //잔여 데이터가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }

        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
            {
                return false;
            }
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
            {
                return false;
            }
            _writePos += numOfBytes;
            return true;
        }

    }
}
