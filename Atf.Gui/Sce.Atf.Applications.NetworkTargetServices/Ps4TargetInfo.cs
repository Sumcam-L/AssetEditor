namespace Sce.Atf.Applications.NetworkTargetServices;

[Group("Ps4TargetInfo", Header = "TCP/IP Targets", ReadOnlyProperties = "Protocol")]
public class Ps4TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
{
	public const string PlatformName = "Ps4";

	public Ps4TargetInfo()
	{
		base.Name = "Ps4Host";
		base.Platform = "Ps4";
		base.Endpoint = "10.89.0.0:1338";
	}
}
