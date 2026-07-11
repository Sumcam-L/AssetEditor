using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable;

public static class TransformAdapters
{
	public static void SetTransform(this ITransformAdapter transformAdapter, Matrix transform)
	{
		float[] elements = transform.Elements;
		transformAdapter.SetTransform(elements[0], elements[3], elements[4], elements[5]);
	}

	public static Point ClientToTransform(this ITransformAdapter adapter, Point x)
	{
		return GdiUtil.InverseTransform(adapter.Transform, x);
	}

	public static PointF ClientToTransform(this ITransformAdapter adapter, PointF x)
	{
		return GdiUtil.InverseTransform(adapter.Transform, x);
	}

	public static Rectangle ClientToTransform(this ITransformAdapter adapter, Rectangle x)
	{
		return GdiUtil.InverseTransform(adapter.Transform, x);
	}

	public static RectangleF ClientToTransform(this ITransformAdapter adapter, RectangleF x)
	{
		return GdiUtil.InverseTransform(adapter.Transform, x);
	}

	public static Point TransformToClient(this ITransformAdapter adapter, Point x)
	{
		return GdiUtil.Transform(adapter.Transform, x);
	}

	public static PointF TransformToClient(this ITransformAdapter adapter, PointF x)
	{
		return GdiUtil.Transform(adapter.Transform, x);
	}

	public static Rectangle TransformToClient(this ITransformAdapter adapter, Rectangle x)
	{
		return GdiUtil.Transform(adapter.Transform, x);
	}

	public static RectangleF TransformToClient(this ITransformAdapter adapter, RectangleF x)
	{
		return GdiUtil.Transform(adapter.Transform, x);
	}

	public static PointF ConstrainTranslation(this ITransformAdapter adapter, PointF translation)
	{
		PointF minTranslation = adapter.MinTranslation;
		PointF maxTranslation = adapter.MaxTranslation;
		return new PointF(Math.Max(minTranslation.X, Math.Min(maxTranslation.X, translation.X)), Math.Max(minTranslation.Y, Math.Min(maxTranslation.Y, translation.Y)));
	}

	public static PointF ConstrainScale(this ITransformAdapter adapter, PointF scale)
	{
		PointF minScale = adapter.MinScale;
		PointF maxScale = adapter.MaxScale;
		if (adapter.UniformScale)
		{
			float x = (scale.Y = Math.Max(scale.X, scale.Y));
			scale.X = x;
		}
		return new PointF(Math.Max(minScale.X, Math.Min(maxScale.X, scale.X)), Math.Max(minScale.Y, Math.Min(maxScale.Y, scale.Y)));
	}

	public static void Frame(this ITransformAdapter adapter, RectangleF bounds)
	{
		if (!bounds.IsEmpty)
		{
			RectangleF rectangleF = GdiUtil.InverseTransform(adapter.Transform, bounds);
			RectangleF rectangleF2 = adapter.AdaptedControl.ClientRectangle;
			PointF scale = new PointF(Math.Abs(rectangleF2.Width / rectangleF.Width) * 0.86f, Math.Abs(rectangleF2.Height / rectangleF.Height) * 0.86f);
			if (adapter.UniformScale)
			{
				float x = (scale.Y = Math.Min(scale.X, scale.Y));
				scale.X = x;
			}
			scale = adapter.ConstrainScale(scale);
			PointF pointF = new PointF(rectangleF.X + rectangleF.Width / 2f, rectangleF.Y + rectangleF.Height / 2f);
			PointF pointF2 = new PointF(rectangleF2.Width / 2f - pointF.X * scale.X, rectangleF2.Height / 2f - pointF.Y * scale.Y);
			adapter.SetTransform(scale.X, scale.Y, pointF2.X, pointF2.Y);
		}
	}

	public static void EnsureVisible(this ITransformAdapter adapter, RectangleF bounds)
	{
		if (!((RectangleF)adapter.AdaptedControl.ClientRectangle).Contains(bounds))
		{
			adapter.Frame(bounds);
		}
	}

	public static void PanToRect(this ITransformAdapter adapter, RectangleF bounds)
	{
		RectangleF rectangleF = adapter.AdaptedControl.ClientRectangle;
		if (!rectangleF.IsEmpty)
		{
			float num = ((bounds.Right > rectangleF.Right) ? (bounds.Right - rectangleF.Right) : ((!(bounds.Left < rectangleF.Left)) ? 0f : (bounds.Left - rectangleF.Left)));
			float num2 = ((bounds.Bottom > rectangleF.Bottom) ? (bounds.Bottom - rectangleF.Bottom) : ((!(bounds.Top < rectangleF.Top)) ? 0f : (bounds.Top - rectangleF.Top)));
			if (num != 0f || num2 != 0f)
			{
				PointF translation = adapter.Translation;
				adapter.Translation = new PointF(translation.X - num, translation.Y - num2);
			}
		}
	}
}
