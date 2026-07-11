using System;

namespace Sce.Atf.Rendering;

[Flags]
public enum TransformationTypes
{
	Translation = 1,
	Scale = 2,
	Rotation = 4,
	ScalePivot = 8,
	ScalePivotTranslation = 0x10,
	RotatePivot = 0x20,
	RotatePivotTranslation = 0x40,
	UniformScale = 0x80
}
