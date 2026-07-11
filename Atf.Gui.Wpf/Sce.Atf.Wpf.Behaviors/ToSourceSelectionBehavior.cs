using System.Collections;
using System.ComponentModel.Composition;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class ToSourceSelectionBehavior : SelectionBehaviorBase
{
	protected override void OnAttached()
	{
		base.OnAttached();
		Composer.Current.Container.SatisfyImportsOnce(this);
	}

	protected override void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
	{
		base.OnAssociatedObjectSelectionChanged(addeditems, removedItems);
		if (base.SelectionContext != null)
		{
			if (removedItems.Count > 0)
			{
				base.SelectionContext.RemoveRange(removedItems);
			}
			if (addeditems.Count > 0)
			{
				base.SelectionContext.AddRange(addeditems);
			}
		}
	}
}
