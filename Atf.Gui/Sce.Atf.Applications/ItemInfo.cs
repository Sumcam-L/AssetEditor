using System.Drawing;

namespace Sce.Atf.Applications;

public abstract class ItemInfo
{
	private string m_label = string.Empty;

	private FontStyle m_fontStyle = FontStyle.Regular;

	private string m_description = string.Empty;

	private int m_imageIndex = -1;

	private int m_stateImageIndex = -1;

	private object[] m_properties = EmptyArray<object>.Instance;

	private bool m_hasCheck;

	private bool m_isLeaf;

	private bool m_allowLabelEdit = true;

	private bool m_allowSelect = true;

	private bool m_isExpandedInView;

	private string m_hoverText = string.Empty;

	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			m_label = value;
		}
	}

	public FontStyle FontStyle
	{
		get
		{
			return m_fontStyle;
		}
		set
		{
			m_fontStyle = value;
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

	public bool HasCheck
	{
		get
		{
			return m_hasCheck;
		}
		set
		{
			m_hasCheck = value;
		}
	}

	public abstract bool Checked { get; set; }

	public bool IsLeaf
	{
		get
		{
			return m_isLeaf;
		}
		set
		{
			m_isLeaf = value;
		}
	}

	public bool AllowLabelEdit
	{
		get
		{
			return m_allowLabelEdit;
		}
		set
		{
			m_allowLabelEdit = value;
		}
	}

	public bool AllowSelect
	{
		get
		{
			return m_allowSelect;
		}
		set
		{
			m_allowSelect = value;
		}
	}

	public bool IsExpandedInView
	{
		get
		{
			return m_isExpandedInView;
		}
		set
		{
			m_isExpandedInView = value;
		}
	}

	public int ImageIndex
	{
		get
		{
			return m_imageIndex;
		}
		set
		{
			m_imageIndex = value;
		}
	}

	public int StateImageIndex
	{
		get
		{
			return m_stateImageIndex;
		}
		set
		{
			m_stateImageIndex = value;
		}
	}

	public object[] Properties
	{
		get
		{
			return m_properties;
		}
		set
		{
			m_properties = value;
		}
	}

	public string HoverText
	{
		get
		{
			return m_hoverText;
		}
		set
		{
			m_hoverText = value;
		}
	}

	public ItemInfo()
	{
	}
}
