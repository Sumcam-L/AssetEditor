using System;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Behaviors;

public class AdaptingTwoWaySelectionBehavior : TwoWaySelectionBehavior
{
	public Type AdapterType { get; set; }

	protected override object ConvertFromSelectionContext(object item)
	{
		if (AdapterType == null)
		{
			throw new InvalidOperationException("AdapterType must be set on AdaptingTwoWaySelectionBehavior");
		}
		return (item is IAdaptable reference) ? reference.As(AdapterType) : null;
	}
}
