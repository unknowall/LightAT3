using LightCodec;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScePSPUtils
{
    public sealed class ConsumerBufferStream : Stream
    {
        private MemoryStream _memoryStream;
        private long _consumePosition;
        private long _totalProducePosition = 0;
        private long _totalConsumePosition;

        public ConsumerBufferStream()
        {
            _memoryStream = new MemoryStream();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override void Flush()
        {
        }

        public override long Length => _memoryStream.Length;

        public override long Position
        {
            get => _consumePosition;
            set => throw new NotImplementedException();
        }

        private void TryReduceMemorySize()
        {
            if (_readTransactionStack.Count == 0)
            {
                if (_consumePosition >= 512 * 1024 || _consumePosition >= Length / 2)
                {
                    var newMemoryStream = new MemoryStream();
                    _memoryStream.SliceWithLength(_consumePosition).CopyToFast(newMemoryStream);
                    _consumePosition = 0;
                    _memoryStream = newMemoryStream;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            _memoryStream.Position = _consumePosition;
            var readed = _memoryStream.Read(buffer, offset, count);
            _consumePosition = _memoryStream.Position;
            _totalConsumePosition += readed;
            TryReduceMemorySize();
            return readed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _memoryStream.Position = _memoryStream.Length;
            _memoryStream.Write(buffer, offset, count);
            _totalProducePosition += count;
        }

        internal class ReadTransactionState
        {
            internal long ConsumePosition;
            internal long TotalConsumePosition;
        }

        private readonly Stack<ReadTransactionState> _readTransactionStack = new Stack<ReadTransactionState>();

        /// <summary>
        /// 
        /// </summary>
        public void TransactionBegin()
        {
            _readTransactionStack.Push(new ReadTransactionState()
            {
                ConsumePosition = _consumePosition,
                TotalConsumePosition = _totalConsumePosition,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void TransactionCommit()
        {
            _readTransactionStack.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        public void TransactionRevert()
        {
            var readTransactionState = _readTransactionStack.Pop();
            _consumePosition = readTransactionState.ConsumePosition;
            _totalConsumePosition = readTransactionState.TotalConsumePosition;
        }
    }
}