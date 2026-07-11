using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public interface IParameterViewModel : INotifyPropertyChanged
{
	string Name { get; }

	string Description { get; }

	IParameter Parameter { get; set; }

	IValue Value { get; set; }

	bool IsImporting { get; }
}
