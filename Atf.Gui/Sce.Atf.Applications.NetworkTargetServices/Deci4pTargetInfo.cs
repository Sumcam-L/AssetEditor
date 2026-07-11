namespace Sce.Atf.Applications.NetworkTargetServices;

[Group("Deci4pTargetInfo", Header = "Vita Targets", ExternalEditorProperties = "Name,Platform,Endpoint,Protocol,Scope")]
public class Deci4pTargetInfo : TargetInfo
{
	public const string PlatformName = "Vita";

	public const string ProtocolName = "Deci4p";
}
