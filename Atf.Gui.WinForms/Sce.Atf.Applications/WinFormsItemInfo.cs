using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class WinFormsItemInfo : ItemInfo
{
	private readonly ImageList m_imageList;

	private readonly ImageList m_stateImageList;

	private CheckState m_checkState = CheckState.Unchecked;

	private static readonly ImageList s_emptyImageList = new ImageList();

	public override bool Checked
	{
		get
		{
			return m_checkState == CheckState.Checked;
		}
		set
		{
			m_checkState = (value ? CheckState.Checked : CheckState.Unchecked);
		}
	}

	public CheckState CheckState
	{
		get
		{
			return m_checkState;
		}
		set
		{
			m_checkState = value;
		}
	}

	public ImageList ImageList => m_imageList;

	public ImageList StateImageList => m_stateImageList;

	public WinFormsItemInfo()
		: this(null, null)
	{
	}

	public WinFormsItemInfo(ImageList imageList)
		: this(imageList, null)
	{
	}

	public WinFormsItemInfo(ImageList imageList, ImageList stateImageList)
	{
		if (imageList == null)
		{
			imageList = s_emptyImageList;
		}
		if (stateImageList == null)
		{
			stateImageList = imageList;
		}
		m_imageList = imageList;
		m_stateImageList = stateImageList;
	}
}
