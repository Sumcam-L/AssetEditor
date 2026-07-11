using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Applications;

public interface IViewingContext : Sce.Atf.Applications.IViewingContext
{
	IAdaptable Adaptable { get; set; }

	IViewingAdapter ViewingAdapter { get; set; }

	IEnumerable<IPickingAdapter> PickingAdapters { get; set; }

	IEnumerable<ILayoutConstraint> LayoutConstraints { get; set; }
}
