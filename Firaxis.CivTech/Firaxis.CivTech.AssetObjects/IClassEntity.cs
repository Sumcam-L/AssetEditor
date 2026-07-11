using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IClassEntity : ICloudEntity, INameProvider, IVersionedData
{
	InstanceType InstanceTypeEnum { get; }

	ClassType ClassTypeEnum { get; }

	IParameterSet CookParameters { get; }

	string PreviewModuleName { get; set; }

	IEnumerable<IClassDataFile> DataFiles { get; }

	bool DeserializeFromXML(string XmlText);

	IClassDataFile AddDataFile(string sID, string sExtension);

	void RemoveDataFile(IClassDataFile df);

	void DisallowClass(ClassType eTypeEnum, string name);
}
