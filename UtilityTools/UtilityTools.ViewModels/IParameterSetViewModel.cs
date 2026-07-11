using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public interface IParameterSetViewModel
{
	IParameterSet Parameters { get; set; }

	IValueSet Values { get; set; }

	IEnumerable<IParameterViewModel> ParameterList { get; }

	bool IsImporting { get; }

	event EventHandler<ParameterChangedEventArgs> ParameterChangedEvent;

	event EventHandler<EntityReimportedEventArgs> EntityReimportedEvent;

	void RevertTemporaryHiding();
}
