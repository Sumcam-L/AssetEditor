using System;

namespace Firaxis.Validation;

public class ValidatorSourceEventArgs : EventArgs
{
	public string Name { get; private set; }

	public ValidatorSourceEventArgs(string name)
	{
		Name = name;
	}
}
