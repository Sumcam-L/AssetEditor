using System;
using SharpDX.Multimedia;

namespace SharpDX.Serialization;

public class InvalidChunkException : Exception
{
	public FourCC ChunkId { get; private set; }

	public FourCC ExpectedChunkId { get; private set; }

	public InvalidChunkException(FourCC chunkId, FourCC expectedChunkId)
		: base($"Unexpected chunk [{chunkId}/0x{(int)chunkId:X}] instead of [{expectedChunkId}/0x{(int)expectedChunkId:X}]")
	{
		ChunkId = chunkId;
		ExpectedChunkId = expectedChunkId;
	}
}
