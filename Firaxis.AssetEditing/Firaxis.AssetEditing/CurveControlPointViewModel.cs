using System;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Utility.Converters;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class CurveControlPointViewModel
{
	private Func<PointF> GetLocationFunction { get; set; }

	private Action<PointF> SetLocationFunction { get; set; }

	private Func<PointF, PointF> SetLocationTransformFunction { get; set; }

	[IgnoreChildren(true)]
	[TypeConverter(typeof(PointFConverter))]
	public PointF Location
	{
		get
		{
			return GetLocationFunction();
		}
		set
		{
			PointF pointF = SetLocationTransformFunction(value);
			if (!(pointF == Location))
			{
				SetLocationFunction(pointF);
			}
		}
	}

	public CurveControlPointViewModel(Func<PointF> getLocationFunc, Action<PointF> setLocationFunc, Func<PointF, PointF> setLocationTransformFunc)
	{
		GetLocationFunction = getLocationFunc;
		SetLocationFunction = setLocationFunc;
		SetLocationTransformFunction = setLocationTransformFunc;
	}

	public PointF GetActualLocationDelta(PointF locationDelta)
	{
		PointF arg = new PointF(Location.X + locationDelta.X, Location.Y + locationDelta.Y);
		PointF pointF = SetLocationTransformFunction(arg);
		return new PointF(pointF.X - Location.X, pointF.Y - Location.Y);
	}
}
