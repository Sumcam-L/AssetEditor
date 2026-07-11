using System;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramHitEventArgs : EventArgs
{
	public readonly DiagramHitRecord HitRecord;

	public DiagramHitEventArgs(DiagramHitRecord hitRecord)
	{
		HitRecord = hitRecord;
	}
}
