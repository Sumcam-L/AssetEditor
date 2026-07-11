using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Firaxis.CivTech.AssetObjects;

[Serializable]
[XmlRoot("EntityInfo")]
[DataContract(Name = "EntityInfo")]
public struct EntityFileInfo
{
	[DataMember]
	public InstanceType instanceType { get; }

	[DataMember]
	public string name { get; }

	[DataMember]
	public string status { get; }

	[DataMember]
	public DateTime lastModified { get; }

	public EntityFileInfo(InstanceType instanceType, string name)
		: this(instanceType, name, "Unknown", DateTime.MinValue)
	{
	}

	public EntityFileInfo(InstanceType instanceType, string name, string status, DateTime lastModified)
	{
		this.instanceType = instanceType;
		this.name = name;
		this.status = status;
		this.lastModified = lastModified;
	}

	public static int GenerateHashCode(InstanceType instType, string name)
	{
		if (name != null)
		{
			return instType.GetHashCode() ^ name.GetHashCode();
		}
		BugSubmitter.SilentReport("EntityFileInfo name is null when trying to get the hash code.");
		return instType.GetHashCode();
	}

	public override int GetHashCode()
	{
		return GenerateHashCode(instanceType, name);
	}
}
