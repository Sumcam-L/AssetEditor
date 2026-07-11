using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Firaxis.Controls.Properties;

namespace Firaxis.Controls;

public sealed class CustomCursors
{
	private Dictionary<CustomCursor, Cursor> cursors;

	private static CustomCursors self;

	private CustomCursors()
	{
		cursors = new Dictionary<CustomCursor, Cursor>();
	}

	public Cursor GetCursor(CustomCursor cursor)
	{
		Cursor value = null;
		if (!cursors.TryGetValue(cursor, out value))
		{
			byte[] buffer = null;
			switch (cursor)
			{
			case CustomCursor.FingerPoint:
				buffer = Firaxis.Controls.Properties.Resources.fpoint;
				break;
			case CustomCursor.HandDrag:
				buffer = Firaxis.Controls.Properties.Resources.hand;
				break;
			case CustomCursor.LeftExtend:
				buffer = Firaxis.Controls.Properties.Resources.left_extend;
				break;
			case CustomCursor.RightExtend:
				buffer = Firaxis.Controls.Properties.Resources.right_extend;
				break;
			case CustomCursor.HSplit:
				buffer = Firaxis.Controls.Properties.Resources.splith;
				break;
			case CustomCursor.VSplit:
				buffer = Firaxis.Controls.Properties.Resources.splitv;
				break;
			}
			value = new Cursor(new MemoryStream(buffer));
			cursors.Add(cursor, value);
		}
		return value;
	}

	public static Cursor Get(CustomCursor cursor)
	{
		if (self == null)
		{
			self = new CustomCursors();
		}
		return self.GetCursor(cursor);
	}

	public static void FreeCursors()
	{
		if (self != null)
		{
			self.cursors.Clear();
		}
	}
}
