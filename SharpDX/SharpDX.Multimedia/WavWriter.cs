using System;
using System.Collections.Generic;
using System.IO;
using SharpDX.Serialization;

namespace SharpDX.Multimedia;

public class WavWriter
{
	private readonly BinarySerializer serializer;

	private bool isBegin;

	public WavWriter(Stream outputStream)
	{
		serializer = new BinarySerializer(outputStream, SerializerMode.Write);
	}

	public void Begin(WaveFormat waveFormat)
	{
		if (isBegin)
		{
			throw new InvalidOperationException("Cannot begin a new WAV while another begin has not been closed");
		}
		serializer.BeginChunk("RIFF");
		FourCC value = new FourCC("WAVE");
		serializer.Serialize(ref value);
		serializer.BeginChunk("fmt ");
		serializer.Serialize(ref waveFormat);
		serializer.EndChunk();
		serializer.BeginChunk("data");
		isBegin = true;
	}

	public void AppendData(IEnumerable<DataPointer> dataPointers)
	{
		CheckBegin();
		foreach (DataPointer dataPointer in dataPointers)
		{
			AppendData(dataPointer);
		}
	}

	public void AppendData<T>(IEnumerable<T[]> dataBuffers) where T : struct
	{
		CheckBegin();
		foreach (T[] dataBuffer in dataBuffers)
		{
			AppendData(dataBuffer);
		}
	}

	public void AppendData(DataPointer dataPointer)
	{
		CheckBegin();
		serializer.SerializeMemoryRegion(dataPointer);
	}

	public unsafe void AppendData<T>(T[] buffer) where T : struct
	{
		CheckBegin();
		fixed (T* ptr = &buffer[0])
		{
			AppendData(new DataPointer((IntPtr)ptr, Utilities.SizeOf(buffer)));
		}
	}

	public void End()
	{
		CheckBegin();
		serializer.EndChunk();
		serializer.EndChunk();
		isBegin = false;
	}

	private void CheckBegin()
	{
		if (!isBegin)
		{
			throw new InvalidOperationException("Begin was not called");
		}
	}
}
