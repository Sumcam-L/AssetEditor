using System;

namespace Bespoke.Common.Osc;

public class OscBundleReceivedEventArgs : EventArgs
{
	private OscBundle mBundle;

	public OscBundle Bundle => mBundle;

	public OscBundleReceivedEventArgs(OscBundle bundle)
	{
		Assert.ParamIsNotNull(bundle);
		mBundle = bundle;
	}
}
