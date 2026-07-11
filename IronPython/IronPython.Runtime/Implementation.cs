using System.Collections.Generic;
using System.Linq;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("sys.implementation")]
[PythonHidden]
public class Implementation
{
	internal static readonly string _Name = "IronPython";

	internal static readonly string _name = _Name.ToLowerInvariant();

	internal static readonly VersionInfo _version = new VersionInfo();

	internal static readonly int _hexversion = _version.GetHexVersion();

	public readonly string cache_tag;

	public readonly string name = _name;

	public readonly VersionInfo version = _version;

	public readonly int hexversion = _hexversion;

	public string __repr__(CodeContext context)
	{
		IEnumerable<string> source = from attr in PythonOps.GetAttrNames(context, this)
			where !attr.ToString().StartsWith("_")
			select $"{attr}={PythonOps.Repr(context, PythonOps.GetBoundAttr(context, this, attr.ToString()))}";
		return string.Format("{0}({1})", PythonOps.GetPythonTypeName(this), string.Join(",", source.ToArray()));
	}
}
