using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServerCore
{
    /// <summary>
    /// 네트워크 패킷을 받을 때, 데이터의 읽기/쓰기 위치를 관리하며
    /// 누락된 패킷을 조립하는 역할을 하는 버퍼 클래스
    /// </summary>
    class ReceiveBuffer
    {
        // [R] [] [] [] [] [W] [] [] [] []
        // 버퍼의 내부 배열 (고정 크기 할당)
        ArraySegment<byte> _buffer;

        // 데이터를 읽는 위치 (현재 읽기 커서, 마우스 커서처럼 생각 가능)
        int _readPos;

        // 데이터를 쓰는 위치 (현재 쓰기 커서, CMD 입력 커서처럼 생각 가능)
        int _writePos;

        // 버퍼 초기화 (버퍼 크기를 지정하여 생성)
        public ReceiveBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize]);
        }

        /// <summary>
        /// 현재 버퍼에 저장된 유효 데이터 크기 (읽을 수 있는 데이터 크기)
        /// </summary>
        public int DataSize { get { return _writePos - _readPos; } }

        /// <summary>
        /// 현재 버퍼에서 새로운 데이터를 쓸 수 있는 여유 공간 크기
        /// </summary>
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        /// <summary>
        /// 현재 읽을 수 있는 데이터의 범위를 반환 (읽기 전용 세그먼트)
        /// </summary>
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        /// <summary>
        /// 현재 데이터를 쓸 수 있는 버퍼 영역을 반환 (쓰기 전용 세그먼트)
        /// </summary>
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        /// <summary>
        /// 사용한 데이터 정리 (버퍼 공간 재사용)
        /// 읽기 커서가 진행된 경우, 남은 데이터를 앞으로 이동시켜 공간을 확보함
        /// </summary>
        public void Clean()
        {
            int dataSize = DataSize; // 현재 남아 있는 데이터 크기
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면, 단순히 커서 위치를 리셋하여 초기 상태로 복원
                _readPos = _writePos = 0;
            }
            else
            {
                // 읽지 않은 데이터가 남아 있으면, 데이터를 앞으로 복사하여 버퍼 공간 확보
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);

                // 복사 후 커서 위치 업데이트
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        /// <summary>
        /// 데이터를 읽은 후, 읽기 커서를 이동
        /// </summary>
        /// <param name="numOfBytes">읽을 데이터 크기</param>
        /// <returns>읽기 성공 여부</returns>
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
            {
                // 요청한 크기만큼 읽을 데이터가 부족하면 실패
                return false;
            }

            // 읽기 커서 이동
            _readPos += numOfBytes;
            return true;
        }

        /// <summary>
        /// 데이터를 쓴 후, 쓰기 커서를 이동
        /// </summary>
        /// <param name="numOfBytes">쓰기 데이터 크기</param>
        /// <returns>쓰기 성공 여부</returns>
        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
            {
                // 남은 공간보다 큰 데이터를 쓰려고 하면 실패
                return false;
            }

            // 쓰기 커서 이동
            _writePos += numOfBytes;
            return true;
        }
    }
}
