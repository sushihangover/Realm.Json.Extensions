using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Android.Util;

namespace RealmJson.Test
{
	public class SlidingStream : Stream
	{
		public SlidingStream()
		{
			ReadTimeout = -1;
		}

		private readonly object _writeSyncRoot = new object();
		private readonly object _readSyncRoot = new object();
		private readonly LinkedList<ArraySegment<byte>> _pendingSegments = new LinkedList<ArraySegment<byte>>();
		private readonly ManualResetEventSlim _dataAvailableResetEvent = new ManualResetEventSlim();

		public int ReadTimeout { get; set; }
		public bool Finished { get; set; }

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (_readSyncRoot)
			{
				if (_pendingSegments.Count == 0)
				{
					// Blocking read to allow time for stream.Write to take place...
					//Task.Run(() => { _dataAvailableResetEvent.Wait(ReadTimeout); }).Wait();
					_dataAvailableResetEvent.Wait(ReadTimeout);
				}
				if (_pendingSegments.Count == 0) // If we have timedout waiting and no data is available, stream is considered closed...
					return 0;

				count = count == 1024 ? 256 : count; // HACK: Prevent buffer overrun due to StreamReader requesting 1024 regardless of the count

				int currentCount = 0;
				int currentOffset = 0;

				while (currentCount < count)
				{
					ArraySegment<byte> segment = _pendingSegments.First.Value;
					_pendingSegments.RemoveFirst();

					int index = segment.Offset;
					for (; index < segment.Count; index++)
					{
						if (currentOffset < offset)
						{
							currentOffset++;
						}
						else
						{
							buffer[currentCount] = segment.Array[index];
							currentCount++;
						}
					}

					if (currentCount == count)
					{
						if (index < segment.Offset + segment.Count)
						{
							_pendingSegments.AddFirst(new ArraySegment<byte>(segment.Array, index, segment.Offset + segment.Count - index));
						}
					}

					if (_pendingSegments.Count == 0)
					{
						//s = Encoding.UTF8.GetString(buffer, offset, count);
						//Console.WriteLine($"READ: {s}");
						_dataAvailableResetEvent.Reset();
						return currentCount;
					}
				}
				//s = Encoding.UTF8.GetString(buffer, offset, count);
				//Console.WriteLine($"READ: {s}");
				_dataAvailableResetEvent.Reset();
				return currentCount;
			}
		}

		public override void WriteByte(byte value)
		{
			lock (_writeSyncRoot)
			{
				byte[] copy = new byte[1];
				copy[0] = value;
				_pendingSegments.AddLast(new ArraySegment<byte>(copy));

				_dataAvailableResetEvent.Set();
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 0)
				return;
			lock (_writeSyncRoot)
			{
				//var s = Encoding.UTF8.GetString(buffer, offset, count);
				//Console.WriteLine($"WRITE: {s}");
				byte[] copy = new byte[count];
				if (copy == null)
					throw new Exception("byte[] creation failure");
				Array.Copy(buffer, offset, copy, 0, count);
				try
				{
					var aSeg = new ArraySegment<byte>(copy);
					_pendingSegments.AddLast(aSeg);
					_dataAvailableResetEvent.Set();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		public override void Flush()
		{
			/* NOP */
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
	}
}
