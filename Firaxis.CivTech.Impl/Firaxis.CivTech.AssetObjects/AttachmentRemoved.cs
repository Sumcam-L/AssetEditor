using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentRemoved : EntityChangedEvent, IAttachmentRemoved
{
	private string m_attachmentName = string.Empty;

	public virtual string AttachmentName
	{
		get
		{
			return m_attachmentName;
		}
		set
		{
			m_attachmentName = value;
		}
	}

	public AttachmentRemoved()
	{
		SetChangeType(EntityChangeType.ECT_ATTACHMENT_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.AttachmentRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_attachmentName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAttachmentRemoved_002ESetAttachmentName(ptr, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}
}
