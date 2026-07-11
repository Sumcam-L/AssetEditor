using System;
using Firaxis.Validation;

namespace Firaxis.Asset;

public class AssetValidationFinishedEventArgs : EventArgs
{
	public ValidatorProvider.ResultInfoCollection Results { get; private set; }

	public AssetValidationFinishedEventArgs(ValidatorProvider.ResultInfoCollection results)
	{
		Results = results;
	}
}
