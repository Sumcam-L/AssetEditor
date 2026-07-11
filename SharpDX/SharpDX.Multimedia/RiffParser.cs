using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SharpDX.Multimedia;

public class RiffParser : IEnumerator<RiffChunk>, IDisposable, IEnumerator, IEnumerable<RiffChunk>, IEnumerable
{
	private readonly Stream input;

	private readonly long startPosition;

	private readonly BinaryReader reader;

	private readonly Stack<RiffChunk> chunckStack;

	private bool descendNext;

	private bool isEndOfRiff;

	private bool isErrorState;

	private RiffChunk current;

	public Stack<RiffChunk> ChunkStack => chunckStack;

	public RiffChunk Current
	{
		get
		{
			CheckState();
			return current;
		}
	}

	object IEnumerator.Current
	{
		get
		{
			CheckState();
			return Current;
		}
	}

	public RiffParser(Stream input)
	{
		this.input = input;
		startPosition = input.Position;
		reader = new BinaryReader(input);
		chunckStack = new Stack<RiffChunk>();
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		CheckState();
		if (current != null)
		{
			long num = current.DataPosition;
			if (descendNext)
			{
				descendNext = false;
			}
			else
			{
				num += current.Size;
				if ((num & 1) != 0)
				{
					num++;
				}
			}
			input.Position = num;
			RiffChunk riffChunk = chunckStack.Peek();
			long num2 = riffChunk.DataPosition + riffChunk.Size;
			if (input.Position >= num2)
			{
				chunckStack.Pop();
			}
			if (chunckStack.Count == 0)
			{
				isEndOfRiff = true;
				return false;
			}
		}
		FourCC fourCC = reader.ReadUInt32();
		bool flag = fourCC == (FourCC)"LIST";
		bool flag2 = fourCC == (FourCC)"RIFF";
		uint num3 = 0u;
		if (input.Position == startPosition + 4 && !flag2)
		{
			isErrorState = true;
			throw new InvalidOperationException("Invalid RIFF file format");
		}
		num3 = reader.ReadUInt32();
		if (flag || flag2)
		{
			if (flag2 && num3 > input.Length - 8)
			{
				isErrorState = true;
				throw new InvalidOperationException("Invalid RIFF file format");
			}
			num3 -= 4;
			fourCC = reader.ReadUInt32();
		}
		current = new RiffChunk(input, fourCC, num3, (uint)input.Position, flag, flag2);
		return true;
	}

	private void CheckState()
	{
		if (isEndOfRiff)
		{
			throw new InvalidOperationException("End of Riff. Cannot MoveNext");
		}
		if (isErrorState)
		{
			throw new InvalidOperationException("The enumerator is in an error state");
		}
	}

	public void Reset()
	{
		CheckState();
		current = null;
		input.Position = startPosition;
	}

	public void Ascend()
	{
		CheckState();
		RiffChunk riffChunk = chunckStack.Pop();
		input.Position = riffChunk.DataPosition + riffChunk.Size;
	}

	public void Descend()
	{
		CheckState();
		chunckStack.Push(current);
		descendNext = true;
	}

	public IList<RiffChunk> GetAllChunks()
	{
		List<RiffChunk> list = new List<RiffChunk>();
		using IEnumerator<RiffChunk> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			RiffChunk item = enumerator.Current;
			list.Add(item);
		}
		return list;
	}

	public IEnumerator<RiffChunk> GetEnumerator()
	{
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
