using System.ComponentModel;

namespace Sce.Atf.Applications;

public class OscMessageReceivedArgs : HandledEventArgs
{
	public readonly string Address;

	public readonly object Data;

	public OscMessageReceivedArgs(string address, object data)
		: base(defaultHandledValue: false)
	{
		Address = address;
		Data = data;
	}
}
