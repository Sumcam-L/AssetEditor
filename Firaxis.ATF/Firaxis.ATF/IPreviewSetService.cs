namespace Firaxis.ATF;

public interface IPreviewSetService
{
	void ApplyPreviewSetData(PreviewSetData data);

	PreviewSetData GeneratePreviewSetData();
}
