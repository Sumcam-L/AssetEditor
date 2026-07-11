using System;

namespace Firaxis.IO;

public interface IPath : ICloneable, IEquatable<string>
{
	void SetPath(string path);
}
