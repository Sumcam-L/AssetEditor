using System;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

internal class PsdBlockLengthWriter : IDisposable
{
	private bool disposed = false;

	private long lengthPosition;

	private long startPosition;

	private PsdBinaryWriter writer;

	public PsdBlockLengthWriter(PsdBinaryWriter writer)
	{
		this.writer = writer;
		lengthPosition = writer.BaseStream.Position;
		writer.Write(4277010157u);
		startPosition = writer.BaseStream.Position;
	}

	public void Write()
	{
		long position = writer.BaseStream.Position;
		writer.BaseStream.Position = lengthPosition;
		long num = position - startPosition;
		writer.Write((uint)num);
		writer.BaseStream.Position = position;
	}

	public void Dispose()
	{
		if (!disposed)
		{
			Write();
			disposed = true;
		}
	}
}
