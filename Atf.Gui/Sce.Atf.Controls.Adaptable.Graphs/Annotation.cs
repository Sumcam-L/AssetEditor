using System.Drawing;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Annotation : DomNodeAdapter, IAnnotation
{
	protected abstract AttributeInfo TextAttribute { get; }

	protected abstract AttributeInfo XAttribute { get; }

	protected abstract AttributeInfo YAttribute { get; }

	protected abstract AttributeInfo WidthAttribute { get; }

	protected abstract AttributeInfo HeightAttribute { get; }

	protected abstract AttributeInfo BackColorAttribute { get; }

	protected abstract AttributeInfo ForeColorAttribute { get; }

	public string Text
	{
		get
		{
			return (string)base.DomNode.GetAttribute(TextAttribute);
		}
		set
		{
			base.DomNode.SetAttribute(TextAttribute, value);
		}
	}

	public virtual Point Location
	{
		get
		{
			return new Point(GetAttribute<int>(XAttribute), GetAttribute<int>(YAttribute));
		}
		set
		{
			SetAttribute(XAttribute, value.X);
			SetAttribute(YAttribute, value.Y);
		}
	}

	public virtual Size Size
	{
		get
		{
			return new Size(GetAttribute<int>(WidthAttribute), GetAttribute<int>(HeightAttribute));
		}
		set
		{
			SetAttribute(WidthAttribute, value.Width);
			SetAttribute(HeightAttribute, value.Height);
		}
	}

	public Rectangle Bounds
	{
		get
		{
			return new Rectangle(Location, Size);
		}
		set
		{
			Location = value.Location;
			Size = value.Size;
		}
	}

	public Color BackColor
	{
		get
		{
			return (BackColorAttribute == null) ? SystemColors.Info : Color.FromArgb((int)base.DomNode.GetAttribute(BackColorAttribute));
		}
		set
		{
			base.DomNode.SetAttribute(BackColorAttribute, value.ToArgb());
		}
	}

	public Color ForeColor
	{
		get
		{
			return (ForeColorAttribute == null) ? SystemColors.WindowText : Color.FromArgb((int)base.DomNode.GetAttribute(ForeColorAttribute));
		}
		set
		{
			base.DomNode.SetAttribute(ForeColorAttribute, value.ToArgb());
		}
	}

	public void SetTextSize(Size size)
	{
		Size = size;
	}
}
