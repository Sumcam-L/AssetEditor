using System;

namespace Bespoke.Common.Osc;

public class OscMessageReceivedEventArgs : EventArgs
{
	private OscMessage mMessage;

	public OscMessage Message => mMessage;

	public OscMessageReceivedEventArgs(OscMessage message)
	{
		Assert.ParamIsNotNull(message);
		mMessage = message;
	}
}
