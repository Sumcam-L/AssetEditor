using System;

namespace Firaxis.Utility;

public interface IDataTypeProvider
{
	Type DataType { get; }
}
