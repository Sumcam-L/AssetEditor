using Firaxis.Collections;

namespace Firaxis.Validation;

public interface IValidatorRegistry
{
	ListEvent<ValidatorProvider> Providers { get; }
}
