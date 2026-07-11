namespace Sce.Atf;

public interface IOutputWriter
{
	void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message);

	void Clear();
}
