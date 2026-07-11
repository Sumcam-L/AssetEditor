using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class ToolStripSpringComboBox : ToolStripComboBox
{
	public override Size GetPreferredSize(Size constrainingSize)
	{
		if (base.IsOnOverflow || base.Owner.Orientation == Orientation.Vertical)
		{
			return DefaultSize;
		}
		int num = base.Owner.DisplayRectangle.Width;
		if (base.Owner.OverflowButton.Visible)
		{
			num = num - base.Owner.OverflowButton.Width - base.Owner.OverflowButton.Margin.Horizontal;
		}
		int num2 = 0;
		foreach (ToolStripItem item in base.Owner.Items)
		{
			if (!item.IsOnOverflow)
			{
				if (item is ToolStripSpringTextBox || item is ToolStripSpringComboBox)
				{
					num2++;
					num -= item.Margin.Horizontal;
				}
				else
				{
					num = num - item.Width - item.Margin.Horizontal;
				}
			}
		}
		if (num2 > 1)
		{
			num /= num2;
		}
		if (num < DefaultSize.Width)
		{
			num = DefaultSize.Width;
		}
		Size preferredSize = base.GetPreferredSize(constrainingSize);
		preferredSize.Width = num;
		return preferredSize;
	}
}
