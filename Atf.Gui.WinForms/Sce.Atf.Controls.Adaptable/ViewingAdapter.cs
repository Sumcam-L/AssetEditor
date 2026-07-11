using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class ViewingAdapter : ControlAdapter, IViewingContext
{
	private readonly ITransformAdapter m_transformAdapter;

	public Size MarginSize { get; set; }

	public ViewingAdapter(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
	}

	public bool CanFrame(IEnumerable<object> items)
	{
		if (base.AdaptedControl.HasKeyboardFocus)
		{
			return false;
		}
		return IsBounded(items);
	}

	public void Frame(IEnumerable<object> items)
	{
		Rectangle bounds = GetBounds(items);
		bounds.Inflate(MarginSize);
		m_transformAdapter.Frame(bounds);
	}

	public bool CanEnsureVisible(IEnumerable<object> items)
	{
		return IsBounded(items);
	}

	public void EnsureVisible(IEnumerable<object> items)
	{
		m_transformAdapter.EnsureVisible(GetBounds(items));
	}

	private bool IsBounded(IEnumerable<object> items)
	{
		object[] array = new object[1];
		foreach (IPickingAdapter item in base.AdaptedControl.AsAll<IPickingAdapter>())
		{
			foreach (object item2 in items)
			{
				array[0] = item2;
				if (!item.GetBounds(array).IsEmpty)
				{
					return true;
				}
			}
		}
		foreach (IPickingAdapter2 item3 in base.AdaptedControl.AsAll<IPickingAdapter2>())
		{
			foreach (object item4 in items)
			{
				array[0] = item4;
				if (!item3.GetBounds(array).IsEmpty)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected Rectangle GetBounds(IEnumerable<object> items)
	{
		Rectangle rectangle = default(Rectangle);
		foreach (IPickingAdapter item in base.AdaptedControl.AsAll<IPickingAdapter>())
		{
			Rectangle bounds = item.GetBounds(items);
			if (!bounds.IsEmpty)
			{
				rectangle = (rectangle.IsEmpty ? bounds : Rectangle.Union(rectangle, bounds));
			}
		}
		foreach (IPickingAdapter2 item2 in base.AdaptedControl.AsAll<IPickingAdapter2>())
		{
			Rectangle bounds2 = item2.GetBounds(items);
			if (!bounds2.IsEmpty)
			{
				rectangle = (rectangle.IsEmpty ? bounds2 : Rectangle.Union(rectangle, bounds2));
			}
		}
		rectangle.Inflate(MarginSize);
		return rectangle;
	}
}
