namespace Firaxis.ATF;

public interface IFramedOutputWriter
{
	void StartFrame(string message);

	void SetErrorDetails(string errorDetails);

	void EndFrame(string message);
}
