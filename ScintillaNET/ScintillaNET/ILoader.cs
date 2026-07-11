namespace ScintillaNET;

public interface ILoader
{
	bool AddData(char[] data, int length);

	Document ConvertToDocument();

	int Release();
}
