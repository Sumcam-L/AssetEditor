using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IInstanceEntityAdapter : INamedAdapter
{
	IInstanceEntity InstanceEntity { get; }

	InstanceType InstanceType { get; }

	string ClassName { get; set; }

	event EventHandler<DataFilesEventArgs> DataFilesChanging;

	event EventHandler<DataFilesEventArgs> DataFilesChanged;
}
