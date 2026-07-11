using System.Collections.Generic;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public interface IPaletteService : Sce.Atf.Applications.IPaletteService
{
	IEnumerable<object> Convert(IEnumerable<object> items);
}
