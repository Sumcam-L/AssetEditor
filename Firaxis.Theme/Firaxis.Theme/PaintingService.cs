using System.Collections.Generic;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class PaintingService : IPaintingService
{
	private IDictionary<KeyValuePair<int, int>, Pen> _penCache = new Dictionary<KeyValuePair<int, int>, Pen>();

	private IDictionary<int, SolidBrush> _brushCache = new Dictionary<int, SolidBrush>();

	public SolidBrush GetBrush(Color color)
	{
		int key = color.ToArgb();
		if (_brushCache.TryGetValue(key, out var brush))
		{
			return brush;
		}
		SolidBrush solidBrush = new SolidBrush(color);
		_brushCache.Add(key, solidBrush);
		return solidBrush;
	}

	public Pen GetPen(Color color, int thickness)
	{
		KeyValuePair<int, int> key = new KeyValuePair<int, int>(color.ToArgb(), thickness);
		if (_penCache.TryGetValue(key, out var pen))
		{
			return pen;
		}
		Pen pen2 = new Pen(color, thickness);
		_penCache.Add(key, pen2);
		return pen2;
	}

	public void CleanUp()
	{
		foreach (KeyValuePair<KeyValuePair<int, int>, Pen> item in _penCache)
		{
			item.Value.Dispose();
		}
		_penCache.Clear();
		foreach (KeyValuePair<int, SolidBrush> item2 in _brushCache)
		{
			item2.Value.Dispose();
		}
		_brushCache.Clear();
	}
}
