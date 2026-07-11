using Firaxis.Error;

namespace Firaxis.CivTech.AssetObjects;

public interface ISerializable
{
	string SerializeIntoXML();

	bool DeserializeFromXML(string xmlText);

	bool SerializeIntoFile(string filename);

	ResultCode DeserializeFromFile(string filename);
}
