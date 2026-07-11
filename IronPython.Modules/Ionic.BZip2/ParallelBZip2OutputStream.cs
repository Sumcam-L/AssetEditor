using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Ionic.BZip2;

public class ParallelBZip2OutputStream : Stream
{
	[Flags]
	private enum TraceBits : uint
	{
		None = 0u,
		Crc = 1u,
		Write = 2u,
		All = uint.MaxValue
	}

	private static readonly int BufferPairsPerCore = 4;

	private int _maxWorkers;

	private bool firstWriteDone;

	private int lastFilled;

	private int lastWritten;

	private int latestCompressed;

	private int currentlyFilling;

	private volatile Exception pendingException;

	private bool handlingException;

	private bool emitting;

	private Queue<int> toWrite;

	private Queue<int> toFill;

	private List<WorkItem> pool;

	private object latestLock = new object();

	private object eLock = new object();

	private object outputLock = new object();

	private AutoResetEvent newlyCompressedBlob;

	private long totalBytesWrittenIn;

	private long totalBytesWrittenOut;

	private bool leaveOpen;

	private uint combinedCRC;

	private Stream output;

	private BitWriter bw;

	private int blockSize100k;

	private TraceBits desiredTrace = TraceBits.Crc | TraceBits.Write;

	public int MaxWorkers
	{
		get
		{
			return _maxWorkers;
		}
		set
		{
			if (value < 4)
			{
				throw new ArgumentException("MaxWorkers", "Value must be 4 or greater.");
			}
			_maxWorkers = value;
		}
	}

	public int BlockSize => blockSize100k;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite
	{
		get
		{
			if (output == null)
			{
				throw new ObjectDisposedException("BZip2Stream");
			}
			return output.CanWrite;
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
			return totalBytesWrittenIn;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public long BytesWrittenOut => totalBytesWrittenOut;

	public ParallelBZip2OutputStream(Stream output)
		: this(output, BZip2.MaxBlockSize, leaveOpen: false)
	{
	}

	public ParallelBZip2OutputStream(Stream output, int blockSize)
		: this(output, blockSize, leaveOpen: false)
	{
	}

	public ParallelBZip2OutputStream(Stream output, bool leaveOpen)
		: this(output, BZip2.MaxBlockSize, leaveOpen)
	{
	}

	public ParallelBZip2OutputStream(Stream output, int blockSize, bool leaveOpen)
	{
		if (blockSize < BZip2.MinBlockSize || blockSize > BZip2.MaxBlockSize)
		{
			string message = $"blockSize={blockSize} is out of range; must be between {BZip2.MinBlockSize} and {BZip2.MaxBlockSize}";
			throw new ArgumentException(message, "blockSize");
		}
		this.output = output;
		if (!this.output.CanWrite)
		{
			throw new ArgumentException("The stream is not writable.", "output");
		}
		bw = new BitWriter(this.output);
		blockSize100k = blockSize;
		this.leaveOpen = leaveOpen;
		combinedCRC = 0u;
		MaxWorkers = 16;
		EmitHeader();
	}

	private void InitializePoolOfWorkItems()
	{
		toWrite = new Queue<int>();
		toFill = new Queue<int>();
		pool = new List<WorkItem>();
		int val = BufferPairsPerCore * Environment.ProcessorCount;
		val = Math.Min(val, MaxWorkers);
		for (int i = 0; i < val; i++)
		{
			pool.Add(new WorkItem(i, blockSize100k));
			toFill.Enqueue(i);
		}
		newlyCompressedBlob = new AutoResetEvent(initialState: false);
		currentlyFilling = -1;
		lastFilled = -1;
		lastWritten = -1;
		latestCompressed = -1;
	}

	public override void Close()
	{
		if (pendingException != null)
		{
			handlingException = true;
			Exception ex = pendingException;
			pendingException = null;
			throw ex;
		}
		if (!handlingException && output != null)
		{
			Stream stream = output;
			try
			{
				FlushOutput(lastInput: true);
			}
			finally
			{
				output = null;
				bw = null;
			}
			if (!leaveOpen)
			{
				stream.Close();
			}
		}
	}

	private void FlushOutput(bool lastInput)
	{
		if (!emitting)
		{
			if (currentlyFilling >= 0)
			{
				WorkItem wi = pool[currentlyFilling];
				CompressOne(wi);
				currentlyFilling = -1;
			}
			if (lastInput)
			{
				EmitPendingBuffers(doAll: true, mustWait: false);
				EmitTrailer();
			}
			else
			{
				EmitPendingBuffers(doAll: false, mustWait: false);
			}
		}
	}

	public override void Flush()
	{
		if (output != null)
		{
			FlushOutput(lastInput: false);
			bw.Flush();
			output.Flush();
		}
	}

	private void EmitHeader()
	{
		byte[] array = new byte[4] { 66, 90, 104, 0 };
		array[3] = (byte)(48 + blockSize100k);
		byte[] array2 = array;
		output.Write(array2, 0, array2.Length);
	}

	private void EmitTrailer()
	{
		bw.WriteByte(23);
		bw.WriteByte(114);
		bw.WriteByte(69);
		bw.WriteByte(56);
		bw.WriteByte(80);
		bw.WriteByte(144);
		bw.WriteInt(combinedCRC);
		bw.FinishAndPad();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		bool mustWait = false;
		if (output == null)
		{
			throw new IOException("the stream is not open");
		}
		if (pendingException != null)
		{
			handlingException = true;
			Exception ex = pendingException;
			pendingException = null;
			throw ex;
		}
		if (offset < 0)
		{
			throw new IndexOutOfRangeException($"offset ({offset}) must be > 0");
		}
		if (count < 0)
		{
			throw new IndexOutOfRangeException($"count ({count}) must be > 0");
		}
		if (offset + count > buffer.Length)
		{
			throw new IndexOutOfRangeException($"offset({offset}) count({count}) bLength({buffer.Length})");
		}
		if (count == 0)
		{
			return;
		}
		if (!firstWriteDone)
		{
			InitializePoolOfWorkItems();
			firstWriteDone = true;
		}
		int num = 0;
		int num2 = count;
		do
		{
			EmitPendingBuffers(doAll: false, mustWait);
			mustWait = false;
			int num3 = -1;
			if (currentlyFilling >= 0)
			{
				num3 = currentlyFilling;
			}
			else
			{
				if (toFill.Count == 0)
				{
					mustWait = true;
					continue;
				}
				num3 = toFill.Dequeue();
				lastFilled++;
			}
			WorkItem workItem = pool[num3];
			workItem.ordinal = lastFilled;
			int num4 = workItem.Compressor.Fill(buffer, offset, num2);
			if (num4 != num2)
			{
				if (!ThreadPool.QueueUserWorkItem(CompressOne, workItem))
				{
					throw new Exception("Cannot enqueue workitem");
				}
				currentlyFilling = -1;
				offset += num4;
			}
			else
			{
				currentlyFilling = num3;
			}
			num2 -= num4;
			num += num4;
		}
		while (num2 > 0);
		totalBytesWrittenIn += num;
	}

	private void EmitPendingBuffers(bool doAll, bool mustWait)
	{
		if (emitting)
		{
			return;
		}
		emitting = true;
		if (doAll || mustWait)
		{
			newlyCompressedBlob.WaitOne();
		}
		do
		{
			int num = -1;
			int num2 = (doAll ? 200 : (mustWait ? (-1) : 0));
			int num3 = -1;
			do
			{
				if (Monitor.TryEnter(toWrite, num2))
				{
					num3 = -1;
					try
					{
						if (toWrite.Count > 0)
						{
							num3 = toWrite.Dequeue();
						}
					}
					finally
					{
						Monitor.Exit(toWrite);
					}
					if (num3 < 0)
					{
						continue;
					}
					WorkItem workItem = pool[num3];
					if (workItem.ordinal != lastWritten + 1)
					{
						lock (toWrite)
						{
							toWrite.Enqueue(num3);
						}
						if (num == num3)
						{
							newlyCompressedBlob.WaitOne();
							num = -1;
						}
						else if (num == -1)
						{
							num = num3;
						}
						continue;
					}
					num = -1;
					BitWriter bitWriter = workItem.bw;
					bitWriter.Flush();
					MemoryStream ms = workItem.ms;
					ms.Seek(0L, SeekOrigin.Begin);
					long num4 = 0L;
					byte[] array = new byte[1024];
					int num5;
					while ((num5 = ms.Read(array, 0, array.Length)) > 0)
					{
						for (int i = 0; i < num5; i++)
						{
							bw.WriteByte(array[i]);
						}
						num4 += num5;
					}
					if (bitWriter.NumRemainingBits > 0)
					{
						bw.WriteBits(bitWriter.NumRemainingBits, bitWriter.RemainingBits);
					}
					combinedCRC = (combinedCRC << 1) | (combinedCRC >> 31);
					combinedCRC ^= workItem.Compressor.Crc32;
					totalBytesWrittenOut += num4;
					bitWriter.Reset();
					lastWritten = workItem.ordinal;
					workItem.ordinal = -1;
					toFill.Enqueue(workItem.index);
					if (num2 == -1)
					{
						num2 = 0;
					}
				}
				else
				{
					num3 = -1;
				}
			}
			while (num3 >= 0);
		}
		while (doAll && lastWritten != latestCompressed);
		emitting = false;
	}

	private void CompressOne(object wi)
	{
		WorkItem workItem = (WorkItem)wi;
		try
		{
			workItem.Compressor.CompressAndWrite();
			lock (latestLock)
			{
				if (workItem.ordinal > latestCompressed)
				{
					latestCompressed = workItem.ordinal;
				}
			}
			lock (toWrite)
			{
				toWrite.Enqueue(workItem.index);
			}
			newlyCompressedBlob.Set();
		}
		catch (Exception ex)
		{
			lock (eLock)
			{
				if (pendingException != null)
				{
					pendingException = ex;
				}
			}
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	[Conditional("Trace")]
	private void TraceOutput(TraceBits bits, string format, params object[] varParams)
	{
		if ((bits & desiredTrace) != TraceBits.None)
		{
			lock (outputLock)
			{
				int hashCode = Thread.CurrentThread.GetHashCode();
				Console.ForegroundColor = (ConsoleColor)(hashCode % 8 + 10);
				Console.Write("{0:000} PBOS ", hashCode);
				Console.WriteLine(format, varParams);
				Console.ResetColor();
			}
		}
	}
}
