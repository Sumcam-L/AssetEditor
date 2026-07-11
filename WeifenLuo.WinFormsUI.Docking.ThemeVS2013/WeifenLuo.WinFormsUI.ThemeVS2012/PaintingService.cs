using System.Collections.Generic;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

public class PaintingService : IPaintingService
{
	private IDictionary<KeyValuePair<int, int>, Pen> _penCache = new Dictionary<KeyValuePair<int, int>, Pen>();

	private IDictionary<int, SolidBrush> _brushCache = new Dictionary<int, SolidBrush>();

	public SolidBrush GetBrush(Color color)
	{
		int key = color.ToArgb();
		if (_brushCache.ContainsKey(key))
		{
			return _brushCache[key];
		}
		SolidBrush solidBrush = new SolidBrush(color);
		_brushCache.Add(key, solidBrush);
		return solidBrush;
	}

	public Pen GetPen(Color color, int thickness)
	{
		KeyValuePair<int, int> key = new KeyValuePair<int, int>(color.ToArgb(), thickness);
		if (_penCache.ContainsKey(key))
		{
			return _penCache[key];
		}
		Pen pen = new Pen(color, thickness);
		_penCache.Add(key, pen);
		return pen;
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
