using Sce.Atf;

namespace Firaxis.ATF;

public interface ICategorizedOutputWriter
{
	void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message);

	void Clear();
}
