namespace Sce.Atf.Applications.NetworkTargetServices;

[Group("Ps3TargetInfo", Header = "TCP/IP Targets", ReadOnlyProperties = "Protocol")]
public class Ps3TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
{
	public const string PlatformName = "Ps3";

	public Ps3TargetInfo()
	{
		base.Name = "Ps3Host";
		base.Platform = "Ps3";
		base.Endpoint = "10.89.0.0:1338";
	}
}
