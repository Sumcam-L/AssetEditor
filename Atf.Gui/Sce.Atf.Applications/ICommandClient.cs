namespace Sce.Atf.Applications;

public interface ICommandClient
{
	bool CanDoCommand(object commandTag);

	void DoCommand(object commandTag);

	void UpdateCommand(object commandTag, CommandState commandState);
}
