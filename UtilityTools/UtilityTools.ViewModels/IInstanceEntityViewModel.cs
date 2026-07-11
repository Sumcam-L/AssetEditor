using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace UtilityTools.ViewModels;

public interface IInstanceEntityViewModel : INotifyPropertyChanged
{
	IEnumerable<string> AvailableClasses { get; }

	string ClassName { get; set; }

	bool HasClass { get; }

	string TagsAsString { get; }

	ICommand AddTagCommand { get; }

	string TagToAdd { get; set; }

	string Description { get; set; }

	IParameterSetViewModel ParametersViewModel { get; set; }

	ICommand ReimportCommand { get; }

	ICommand SaveCommand { get; }

	ICommand CancelCommand { get; }
}
