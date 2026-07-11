using System;

namespace Bespoke.Common.Osc;

public class OscPacketReceivedEventArgs : EventArgs
{
	private OscPacket mPacket;

	public OscPacket Packet => mPacket;

	public OscPacketReceivedEventArgs(OscPacket packet)
	{
		Assert.ParamIsNotNull(packet);
		mPacket = packet;
	}
}
