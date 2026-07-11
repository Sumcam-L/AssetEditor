using System;
using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentChanged : EntityChangedEvent, IAttachmentChanged
{
	private string m_oldAttachmentName = string.Empty;

	private string m_newAttachmentName = string.Empty;

	private string m_boneName = string.Empty;

	private string m_modelInstanceName = string.Empty;

	private float[] m_position;

	private float[] m_orientation;

	private float m_scale = 0f;

	public virtual float Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			m_scale = value;
		}
	}

	public virtual float[] Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			byte condition = (byte)(((nint)value.LongLength >= 3) ? 1 : 0);
			BugSubmitter.Assert(condition != 0, "Attachment points require orientation arrays of at least length 3!  @assign bwhitman");
			if ((nint)value.LongLength >= 3)
			{
				m_orientation = value;
			}
		}
	}

	public virtual float[] Position
	{
		get
		{
			return m_position;
		}
		set
		{
			byte condition = (byte)(((nint)value.LongLength >= 3) ? 1 : 0);
			BugSubmitter.Assert(condition != 0, "Attachment points require position arrays of at least length 3!  @assign bwhitman");
			if ((nint)value.LongLength >= 3)
			{
				m_position = value;
			}
		}
	}

	public virtual string ModelInstanceName
	{
		get
		{
			return m_modelInstanceName;
		}
		set
		{
			m_modelInstanceName = value;
		}
	}

	public virtual string BoneName
	{
		get
		{
			return m_boneName;
		}
		set
		{
			m_boneName = value;
		}
	}

	public virtual string NewAttachmentName
	{
		get
		{
			return m_newAttachmentName;
		}
		set
		{
			m_newAttachmentName = value;
		}
	}

	public virtual string OldAttachmentName
	{
		get
		{
			return m_oldAttachmentName;
		}
		set
		{
			m_oldAttachmentName = value;
		}
	}

	public AttachmentChanged()
	{
		SetChangeType(EntityChangeType.ECT_ATTACHMENT_CHANGED);
		m_position = new float[3] { 0f, 0f, 0f };
		m_orientation = new float[3] { 0f, 0f, 0f };
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		StandardStringWrapper standardStringWrapper4 = null;
		global::AssetObjects.AttachmentChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentChanged_003E(changeList);
		if (ptr == null)
		{
			return;
		}
		StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(m_oldAttachmentName);
		try
		{
			standardStringWrapper = standardStringWrapper5;
			StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(m_newAttachmentName);
			try
			{
				standardStringWrapper2 = standardStringWrapper6;
				StandardStringWrapper standardStringWrapper7 = new StandardStringWrapper(m_boneName);
				try
				{
					standardStringWrapper3 = standardStringWrapper7;
					StandardStringWrapper standardStringWrapper8 = new StandardStringWrapper(m_modelInstanceName);
					try
					{
						standardStringWrapper4 = standardStringWrapper8;
						float[] position = m_position;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
						global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, position[0], position[1], position[2]);
						float[] orientation = m_orientation;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector2);
						global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector2, orientation[0], orientation[1], orientation[2]);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetOldAttachmentName(ptr, standardStringWrapper.Value);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetNewAttachmentName(ptr, standardStringWrapper2.Value);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetBoneName(ptr, standardStringWrapper3.Value);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetModelInstanceName(ptr, standardStringWrapper4.Value);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetPosition(ptr, &fGXVector);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetOrientation(ptr, &fGXVector2);
						global::_003CModule_003E.AssetObjects_002EAttachmentChanged_002ESetScale(ptr, m_scale);
					}
					catch
					{
						//try-fault
						((IDisposable)standardStringWrapper4).Dispose();
						throw;
					}
					((IDisposable)standardStringWrapper4).Dispose();
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper3).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper3).Dispose();
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
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
