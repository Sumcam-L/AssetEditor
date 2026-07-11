namespace IronPython.Runtime.Types;

public interface IPythonObject
{
	PythonDictionary Dict { get; }

	PythonType PythonType { get; }

	PythonDictionary SetDict(PythonDictionary dict);

	bool ReplaceDict(PythonDictionary dict);

	void SetPythonType(PythonType newType);

	object[] GetSlots();

	object[] GetSlotsCreate();
}
