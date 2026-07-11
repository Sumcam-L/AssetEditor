using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

public interface IActivatablePropertyEditor
{
	bool IsInline { get; }

	bool Active { get; }

	Control PropertyControl { get; }

	event EventHandler ValueCommitted;

	Control ActivatePropertyControl(object component, PropertyDescriptor prop);

	void DeactivateControl();
}
