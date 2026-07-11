using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IControlRegistry
{
	ControlInfo ActiveControl { get; set; }

	IEnumerable<ControlInfo> Controls { get; }

	event EventHandler ActiveControlChanging;

	event EventHandler ActiveControlChanged;

	event EventHandler<ItemInsertedEventArgs<ControlInfo>> ControlAdded;

	event EventHandler<ItemRemovedEventArgs<ControlInfo>> ControlRemoved;
}
