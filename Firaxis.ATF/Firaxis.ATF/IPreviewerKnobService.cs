namespace Firaxis.ATF;

public interface IPreviewerKnobService : ISequencedProjectChangeWatcher
{
	void SetActivePreviewModule(string modeulName);

	void ClearIfActive(IPreviewContext previewContext);

	void SetActiveEntity(IPreviewContext previewContext);
}
