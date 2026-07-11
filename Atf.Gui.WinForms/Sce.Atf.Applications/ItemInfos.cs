using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class ItemInfos
{
	public static ImageList GetImageList(this ItemInfo info)
	{
		return GetWinFormsItemInfo(info).ImageList;
	}

	public static int GetImageIndex(this ItemInfo info, string name)
	{
		int result = -1;
		ImageList imageList = info.GetImageList();
		if (imageList != null)
		{
			result = imageList.Images.IndexOfKey(name);
		}
		return result;
	}

	public static ImageList GetStateImageList(this ItemInfo info)
	{
		return GetWinFormsItemInfo(info).StateImageList;
	}

	public static int GetStateImageIndex(this ItemInfo info, string name)
	{
		int result = -1;
		ImageList imageList = info.GetImageList();
		if (imageList != null)
		{
			result = imageList.Images.IndexOfKey(name);
		}
		return result;
	}

	public static CheckState GetCheckState(this ItemInfo info)
	{
		return GetWinFormsItemInfo(info).CheckState;
	}

	public static void SetCheckState(this ItemInfo info, CheckState checkState)
	{
		GetWinFormsItemInfo(info).CheckState = checkState;
	}

	private static WinFormsItemInfo GetWinFormsItemInfo(ItemInfo itemInfo)
	{
		WinFormsItemInfo winFormsItemInfo = (WinFormsItemInfo)itemInfo;
		if (winFormsItemInfo == null)
		{
			throw new InvalidOperationException("ItemInfo is not an instance of, or derived from, WinFormsItemInfo");
		}
		return winFormsItemInfo;
	}
}
