using System;
using System.Drawing;

namespace Firaxis.Controls;

public interface IScrollableItem : IDisposable
{
	int ItemHeight { get; }

	object Tag { get; set; }

	bool Visible { get; set; }

	void CalcLayout(Graphics g, SizeF size);

	void PaintItem(object sender, ScrollableItemPaintEventArgs e);
}
