using System;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class KnobChangedNotificationPacket
{
	private string m_pmKnobName;

	private string m_pmGroupName;

	public unsafe KnobChangedNotificationPacket(sbyte* knobName, sbyte* groupName)
	{
		m_pmKnobName = Marshal.PtrToStringAnsi(new IntPtr(knobName));
		m_pmGroupName = Marshal.PtrToStringAnsi(new IntPtr(groupName));
		base._002Ector();
	}

	public string GetKnobName()
	{
		return m_pmKnobName;
	}

	public string GetGroupName()
	{
		return m_pmGroupName;
	}
}
