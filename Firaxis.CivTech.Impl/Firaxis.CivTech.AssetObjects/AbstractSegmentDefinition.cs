using System;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.ReflectionHelperEx;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public abstract class AbstractSegmentDefinition : INativeReflection, ICurveSegmentDefinition
{
	public abstract ICurveSegment Curve { get; }

	public abstract float StartingPoint { get; set; }

	public AbstractSegmentDefinition()
	{
	}

	private void _007EAbstractSegmentDefinition()
	{
	}

	public unsafe abstract void AddToCurve(global::AssetObjects.Curve* Curve);

	public unsafe abstract TypeInfo* GetInstanceTypeInfo();

	public unsafe abstract void SetNativeData(SegmentDefinition* segment);

	public abstract void CopyFrom(AbstractSegmentDefinition otherSegment);

	public virtual int CompareTo(ICurveSegmentDefinition Other)
	{
		return StartingPoint.CompareTo(Other.StartingPoint);
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (!A_0)
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
