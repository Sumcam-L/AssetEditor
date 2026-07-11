namespace Firaxis.ATF;

public interface ISizableFieldContainerAdapter : IFieldContainerAdapter
{
	IFieldValueAdapter AddNamedValue(string name, int index);

	IFieldValueAdapter AddValue(int index);

	void RemoveValue(int index);
}
