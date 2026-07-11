using System.Collections;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class ListViewItemComparer : IComparer
{
	private int col = 0;

	private bool asc = false;

	private ListViewItemComparer()
	{
	}

	private ListViewItemComparer(int col, bool asc)
	{
		this.col = col;
		this.asc = asc;
	}

	public int Compare(object x, object y)
	{
		if (col >= ((ListViewItem)x).SubItems.Count || col >= ((ListViewItem)y).SubItems.Count)
		{
			return 0;
		}
		string text = ((ListViewItem)x).SubItems[col].Text;
		string text2 = ((ListViewItem)y).SubItems[col].Text;
		int num = string.Compare(text, text2);
		if (asc)
		{
			num = -num;
		}
		return num;
	}

	public static void Sort(ListView listView, int col, bool asc)
	{
		listView.ListViewItemSorter = new ListViewItemComparer(col, asc);
	}
}
