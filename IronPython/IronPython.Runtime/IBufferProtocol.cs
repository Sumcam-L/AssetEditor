using System.Collections.Generic;
using System.Numerics;

namespace IronPython.Runtime;

public interface IBufferProtocol
{
	int ItemCount { get; }

	string Format { get; }

	BigInteger ItemSize { get; }

	BigInteger NumberDimensions { get; }

	bool ReadOnly { get; }

	PythonTuple Strides { get; }

	object SubOffsets { get; }

	Bytes GetItem(int index);

	void SetItem(int index, object value);

	void SetSlice(Slice index, object value);

	IList<BigInteger> GetShape(int start, int? end);

	Bytes ToBytes(int start, int? end);

	List ToList(int start, int? end);
}
