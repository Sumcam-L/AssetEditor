using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IPrimGroupStateInformation : IAssemblyInstance, IDisposable
{
	string MeshName { get; }

	string GroupName { get; }

	string StateName { get; }

	IValueSet Values { get; }

	void AssignFromPrimGroupState(IPrimGroupState state);
}
