namespace Sce.Atf;

public static class OutputWriters
{
	public static void Write(this IOutputWriter writer, OutputMessageType type, OutputMessageVerbosity verbosity, string formatString, params object[] args)
	{
		writer.Write(type, verbosity, string.Format(formatString, args));
	}
}
