using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Sce.Atf.Wpf.Models;

public class StandardViewModels
{
	public static ComposablePartCatalog Catalog => new TypeCatalog(typeof(ViewModelRepository), typeof(MainMenuViewModel), typeof(ToolBarViewModel));
}
