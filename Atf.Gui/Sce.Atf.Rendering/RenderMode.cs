using System;

namespace Sce.Atf.Rendering;

[Flags]
public enum RenderMode
{
	Smooth = 1,
	Wireframe = 2,
	Textured = 4,
	Lit = 8,
	SolidColor = 0x10,
	WireframeColor = 0x20,
	WireframeThickness = 0x40,
	DisableZBuffer = 0x80,
	Alpha = 0x100,
	CullBackFace = 0x200,
	DisableZBufferWrite = 0x400,
	Max = 0x400
}
