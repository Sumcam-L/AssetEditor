namespace Sce.Atf.Applications.NetworkTargetServices;

[Group("X86TargetInfo", Header = "TCP/IP Targets", ReadOnlyProperties = "Protocol")]
public class X86TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
{
	public const string PlatformName = "x86";

	public X86TargetInfo()
	{
		base.Name = "localhost";
		base.Platform = "x86";
		base.Endpoint = "127.0.0.1:1338";
	}
}
