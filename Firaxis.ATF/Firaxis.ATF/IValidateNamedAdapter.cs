namespace Firaxis.ATF;

public interface IValidateNamedAdapter : INamedAdapter
{
	bool ValidateNameChange { get; set; }
}
