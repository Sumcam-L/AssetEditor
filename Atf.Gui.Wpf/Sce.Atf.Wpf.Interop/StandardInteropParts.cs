using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Sce.Atf.Wpf.Interop;

public class StandardInteropParts
{
	public static ComposablePartCatalog Catalog => new TypeCatalog(typeof(MainWindowAdapter), typeof(ContextMenuService), typeof(DialogService), typeof(ControlHostServiceAdapter));
}
