using System;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public static class TransformAdapters
{
	public static void SetTransform(this ITransformAdapter transformAdapter, Matrix transform)
	{
		transformAdapter.SetTransform(transform.M11, transform.M22, transform.OffsetX, transform.OffsetY);
	}

	public static Rect ClientToTransform(this ITransformAdapter adapter, Rect x)
	{
		return MathUtil.InverseTransform(adapter.Transform, x);
	}

	public static Rect TransformToClient(this ITransformAdapter adapter, Rect x)
	{
		return MathUtil.Transform(adapter.Transform, x);
	}

	public static Point ClientToTransform(this ITransformAdapter adapter, Point x)
	{
		return MathUtil.InverseTransform(adapter.Transform, x);
	}

	public static Point TransformToClient(this ITransformAdapter adapter, Point x)
	{
		return MathUtil.Transform(adapter.Transform, x);
	}

	public static Point ConstrainTranslation(this ITransformAdapter adapter, Point translation)
	{
		Point minTranslation = adapter.MinTranslation;
		Point maxTranslation = adapter.MaxTranslation;
		return new Point(Math.Max(minTranslation.X, Math.Min(maxTranslation.X, translation.X)), Math.Max(minTranslation.Y, Math.Min(maxTranslation.Y, translation.Y)));
	}

	public static Point ConstrainScale(this ITransformAdapter adapter, Point scale)
	{
		Point minScale = adapter.MinScale;
		Point maxScale = adapter.MaxScale;
		if (adapter.UniformScale)
		{
			double x = (scale.Y = Math.Max(scale.X, scale.Y));
			scale.X = x;
		}
		return new Point(Math.Max(minScale.X, Math.Min(maxScale.X, scale.X)), Math.Max(minScale.Y, Math.Min(maxScale.Y, scale.Y)));
	}

	public static void Frame(this ITransformAdapter adapter, Rect itemBounds, Rect clientBounds)
	{
		if (adapter != null && !itemBounds.IsEmpty && adapter.Transform.HasInverse)
		{
			Rect rect = MathUtil.InverseTransform(adapter.Transform, itemBounds);
			Point scale = new Point(Math.Abs(clientBounds.Width / rect.Width) * 0.86, Math.Abs(clientBounds.Height / rect.Height) * 0.86);
			if (adapter.UniformScale)
			{
				double x = (scale.Y = Math.Min(scale.X, scale.Y));
				scale.X = x;
			}
			scale = adapter.ConstrainScale(scale);
			Point point = new Point(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0);
			Point point2 = new Point(clientBounds.Width / 2.0 - point.X * scale.X, clientBounds.Height / 2.0 - point.Y * scale.Y);
			adapter.SetTransform(scale.X, scale.Y, point2.X, point2.Y);
		}
	}

	public static void EnsureVisible(this ITransformAdapter adapter, Rect itemBounds, Rect clientBounds)
	{
		if (!clientBounds.Contains(itemBounds))
		{
			adapter.Frame(itemBounds, clientBounds);
		}
	}

	public static void ZoomAboutCenter(this ITransformAdapter adapter, Point scale, Rect itemBounds, Rect clientBounds)
	{
		if (adapter != null && adapter.Transform.HasInverse && !itemBounds.IsEmpty)
		{
			Rect rect = MathUtil.InverseTransform(adapter.Transform, itemBounds);
			if (adapter.UniformScale)
			{
				double x = (scale.Y = Math.Min(scale.X, scale.Y));
				scale.X = x;
			}
			scale = adapter.ConstrainScale(scale);
			Point point = new Point(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0);
			Point point2 = new Point(clientBounds.Width / 2.0 - point.X * scale.X, clientBounds.Height / 2.0 - point.Y * scale.Y);
			adapter.SetTransform(scale.X, scale.Y, point2.X, point2.Y);
		}
	}

	public static void PanToRect(this ITransformAdapter adapter, Rect bounds, Rect clientRect)
	{
		if (!clientRect.IsEmpty)
		{
			double num = ((bounds.Right > clientRect.Right) ? (bounds.Right - clientRect.Right) : ((!(bounds.Left < clientRect.Left)) ? 0.0 : (bounds.Left - clientRect.Left)));
			double num2 = ((bounds.Bottom > clientRect.Bottom) ? (bounds.Bottom - clientRect.Bottom) : ((!(bounds.Top < clientRect.Top)) ? 0.0 : (bounds.Top - clientRect.Top)));
			if (num != 0.0 || num2 != 0.0)
			{
				Point translation = adapter.Translation;
				adapter.Translation = new Point(translation.X - num, translation.Y - num2);
			}
		}
	}
}
