using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IInstanceEntity : ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	InstanceType Type { get; }

	string XMLExtension { get; }

	string ClassName { get; set; }

	IValueSet CookParameters { get; }

	IEnumerable<IInstanceDataFile> DataFiles { get; }

	bool IsLocked { get; set; }

	void PublishStats(IDictionary<string, int> stats);

	void AddDataFile(string ID, string relativePath);

	void UpdateDataFile(string ID, string relativePath);

	void PopulateDataFiles(IClassEntity classEntity);

	void RemoveDataFile(string ID);

	IInstanceDataFile FindDataFileByID(string id);

	void ClearDataFiles();

	void CrawlCooktimeDependencies(Action<InstanceType, string> action);

	string GetXMLPath();

	string GetDataFilePath(string dataFileRelativePath);

	string GetActiveProjectXMLPath();

	IEnumerable<string> GetEntityPaths();

	new string ToString();

	string DetailString();
}
