using System;
using System.Drawing;

namespace Sce.Atf.Controls;

[Obsolete("Is a temporary example. Will be removed or relocated.")]
public class ChristmasTreeRenderer : TreeItemRenderer
{
	private static readonly Brush[] s_coloredBrushes = new Brush[3]
	{
		Brushes.Red,
		Brushes.Green,
		Brushes.Blue
	};

	private static readonly Pen s_hierarchyPen = Pens.Green;

	private static readonly Pen s_expanderPen = Pens.Red;

	public ChristmasTreeRenderer()
	{
		base.ExpanderSize = new Size(16, 8);
		base.CheckBoxSize = new Size(40, 32);
	}

	public override Size MeasureLabel(TreeControl.Node node, Graphics g)
	{
		Size result = base.MeasureLabel(node, g);
		result.Width += node.Label.Length;
		return result;
	}

	public override void DrawLabel(TreeControl.Node node, Graphics g, int x, int y, int fullWidth)
	{
		RectangleF rect = new RectangleF(0f, y, fullWidth, node.LabelHeight);
		RectangleF layoutRect = new RectangleF(x, y, node.LabelWidth, node.LabelHeight);
		Font defaultFont = GetDefaultFont(node, g);
		if (node.Selected)
		{
			Brush brush = SystemBrushes.Highlight;
			if (!base.Owner.ContainsFocus)
			{
				brush = SystemBrushes.ButtonHighlight;
			}
			g.FillRectangle(brush, rect);
		}
		CharacterRange[] array = new CharacterRange[node.Label.Length];
		for (int i = 0; i < node.Label.Length; i++)
		{
			array[i] = new CharacterRange(i, 1);
		}
		StringFormat stringFormat = new StringFormat();
		stringFormat.Trimming = StringTrimming.None;
		stringFormat.FormatFlags |= StringFormatFlags.NoClip;
		stringFormat.SetMeasurableCharacterRanges(array);
		Region[] array2 = g.MeasureCharacterRanges(node.Label, defaultFont, layoutRect, stringFormat);
		int num = 0;
		for (int j = 0; j < node.Label.Length; j++)
		{
			RectangleF bounds = array2[j].GetBounds(g);
			bounds.Width += 1f;
			bounds.X += j;
			string s = new string(node.Label[j], 1);
			Brush brush2 = s_coloredBrushes[num++];
			if (num == s_coloredBrushes.Length)
			{
				num = 0;
			}
			g.DrawString(s, defaultFont, brush2, bounds, stringFormat);
		}
	}

	public override void DrawExpander(TreeControl.Node node, Graphics g, int x, int y)
	{
		g.DrawRectangle(s_expanderPen, x, y, base.ExpanderSize.Width, base.ExpanderSize.Height);
		Size size = new Size(base.ExpanderSize.Width - 2, base.ExpanderSize.Height - 2);
		Point point = new Point(base.ExpanderSize.Width / 2, base.ExpanderSize.Height / 2);
		g.DrawLine(s_expanderPen, x + 1, y + point.Y, x + 1 + size.Width, y + point.Y);
		if (!node.Expanded)
		{
			g.DrawLine(s_expanderPen, x + point.X, y + 1, x + point.X, y + 1 + size.Height);
		}
	}

	public override void DrawHierarchyLine(Graphics g, Point a, Point b)
	{
		g.DrawLine(s_hierarchyPen, a.X, a.Y, b.X, b.Y);
	}
}
