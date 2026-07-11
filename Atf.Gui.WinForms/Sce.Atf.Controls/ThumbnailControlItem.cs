using System.Drawing;

namespace Sce.Atf.Controls;

public class ThumbnailControlItem
{
	internal ThumbnailControl Control;

	private Image m_image;

	private object m_tag;

	private string m_indicator;

	private string m_name;

	private string m_description;

	public Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
			InvalidateControl();
		}
	}

	public string Indicator
	{
		get
		{
			return m_indicator;
		}
		set
		{
			m_indicator = value;
			InvalidateControl();
		}
	}

	public object Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			InvalidateControl();
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
		}
	}

	public ThumbnailControlItem(Image image)
	{
		m_image = image;
	}

	private void InvalidateControl()
	{
		if (Control != null)
		{
			Control.Invalidate();
		}
	}
}
