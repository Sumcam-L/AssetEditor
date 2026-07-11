using System;
using Sce.Atf.Controls.Adaptable;

namespace Sce.Atf.Controls;

public class HoverEventArgs<TObject, TPart> : EventArgs
{
	public readonly TObject Object;

	public readonly TPart Part;

	public readonly TObject SubObject;

	public readonly TPart SubPart;

	public readonly AdaptableControl AdaptableControl;

	public HoverEventArgs(TObject obj, TPart part)
	{
		Object = obj;
		Part = part;
		AdaptableControl = null;
	}

	public HoverEventArgs(TObject obj, TPart part, AdaptableControl adaptableControl)
	{
		Object = obj;
		Part = part;
		AdaptableControl = adaptableControl;
	}

	public HoverEventArgs(TObject obj, TPart part, TObject subobj, TPart subpart, AdaptableControl adaptableControl)
	{
		Object = obj;
		Part = part;
		SubObject = subobj;
		SubPart = subpart;
		AdaptableControl = adaptableControl;
	}
}
