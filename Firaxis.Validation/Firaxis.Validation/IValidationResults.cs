using Firaxis.Error;

namespace Firaxis.Validation;

public interface IValidationResults
{
	object Sender { get; set; }

	void AddFailure(OperationResultLevel level, string brief, object context);

	void AddSuccess(string brief, object context);

	void AddLog(string brief);
}
