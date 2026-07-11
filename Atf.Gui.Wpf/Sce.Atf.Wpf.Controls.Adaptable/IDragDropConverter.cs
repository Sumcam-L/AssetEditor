using System.Collections.Generic;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IDragDropConverter
{
	IEnumerable<object> Convert(IEnumerable<object> items);
}
