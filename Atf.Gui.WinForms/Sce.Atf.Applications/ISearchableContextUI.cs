using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface ISearchableContextUI
{
	Control Control { get; }

	event EventHandler UIChanged;
}
