using System.Windows.Forms;

namespace Firaxis.Controls;

public interface IPropertyGridDescriptor
{
	void OnEnterDescriptor(PropertyGrid propertyGrid);

	void OnPropertyChanged(PropertyGrid propertyGrid, PropertyValueChangedEventArgs e);
}
