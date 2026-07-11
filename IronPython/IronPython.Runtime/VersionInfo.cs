namespace IronPython.Runtime;

[PythonType("sys.version_info")]
public class VersionInfo : PythonTuple
{
	public readonly int major;

	public readonly int minor;

	public readonly int micro;

	public readonly string releaselevel;

	public readonly int serial;

	private VersionInfo(int major, int minor, int micro, string releaselevel, int serial)
		: base(new object[5] { major, minor, micro, releaselevel, serial })
	{
		this.major = major;
		this.minor = minor;
		this.micro = micro;
		this.releaselevel = releaselevel;
		this.serial = serial;
	}

	internal VersionInfo()
		: this(2, 7, 3, "final", 0)
	{
	}

	public override string __repr__(CodeContext context)
	{
		return $"sys.version_info(major={major}, minor={minor}, micro={micro}, releaselevel='{releaselevel}', serial={serial})";
	}

	internal int GetHexVersion()
	{
		int num = 0;
		switch (releaselevel)
		{
		case "alpha":
			num = 10;
			break;
		case "beta":
			num = 11;
			break;
		case "candidate":
			num = 12;
			break;
		case "final":
			num = 15;
			break;
		}
		return (major << 24) | (minor << 16) | (micro << 8) | (num << 4) | serial;
	}

	internal string GetVersionString(string _initialVersionString)
	{
		return string.Format("{0}.{1}.{2}{4}{5} ({3})", major, minor, micro, _initialVersionString, (releaselevel != "final") ? "f" : "", (releaselevel != "final") ? serial.ToString() : "");
	}
}
