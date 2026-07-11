using System;

namespace Firaxis.AssetEditing;

public class CurveSegmentDefinitionEventArgs : EventArgs
{
	public readonly CurveSegmentDefinitionAdapter CurveSegmentDefinition;

	public CurveSegmentDefinitionEventArgs(CurveSegmentDefinitionAdapter definition)
	{
		CurveSegmentDefinition = definition;
	}
}
