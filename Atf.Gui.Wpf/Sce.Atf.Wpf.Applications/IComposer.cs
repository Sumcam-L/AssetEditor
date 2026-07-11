using System.ComponentModel.Composition.Hosting;

namespace Sce.Atf.Wpf.Applications;

public interface IComposer
{
	CompositionContainer Container { get; }
}
