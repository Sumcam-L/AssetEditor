using System;

namespace Sce.Atf.Rendering;

[Flags]
public enum AnimDataInterpType
{
	kTimeStep = 0,
	kLinear = 1,
	kFlat = 2,
	kSmooth = 3,
	kTangent = 4,
	kWTangent = 5,
	kSplitTangent = 6,
	kWSplitTangent = 7,
	kATGCurve = 8,
	kATGHermite = 9,
	kNumInterpTypes = 0xA
}
