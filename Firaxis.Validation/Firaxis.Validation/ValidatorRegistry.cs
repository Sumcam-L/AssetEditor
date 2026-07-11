using Firaxis.Collections;

namespace Firaxis.Validation;

public class ValidatorRegistry : IValidatorRegistry
{
	public ListEvent<ValidatorProvider> Providers { get; private set; }

	public ValidatorRegistry()
	{
		Providers = new ListEvent<ValidatorProvider>();
	}
}
