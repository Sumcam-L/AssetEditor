namespace Firaxis.CivTech.FireFX;

public class CompileIssue
{
	public readonly CompileIssueType Type;

	public readonly string File;

	public readonly ulong LineNo;

	public readonly string Message;

	public CompileIssue(CompileIssueType t, string f, ulong ln, string m)
	{
		Type = t;
		File = f;
		LineNo = ln;
		Message = m;
	}
}
