namespace Firaxis.CivTech.AssetObjects;

public interface IClassDataFile
{
	string ID { get; set; }

	string Extension { get; set; }

	bool IsGenerated { get; set; }
}
