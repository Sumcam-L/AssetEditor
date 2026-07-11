using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public abstract class ImportedEntity : InstanceEntity, IImportedEntity
{
	private DateTime m_LocalExportTime;

	private DateTime m_LocalImportTime;

	private bool m_NewEntity;

	public virtual bool NewEntity
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_NewEntity;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			m_NewEntity = value;
		}
	}

	public unsafe virtual DateTime ExportedTime
	{
		get
		{
			return m_LocalExportTime;
		}
		set
		{
			m_LocalExportTime = value;
			global::_003CModule_003E.AssetObjects_002EImportedEntity_002ESetExportedTime((global::AssetObjects.ImportedEntity*)m_pkEntity, (ulong)value.ToFileTime());
		}
	}

	public unsafe virtual DateTime ImportedTime
	{
		get
		{
			return m_LocalImportTime;
		}
		set
		{
			m_LocalImportTime = value;
			global::_003CModule_003E.AssetObjects_002EImportedEntity_002ESetImportedTime((global::AssetObjects.ImportedEntity*)m_pkEntity, (ulong)value.ToFileTime());
		}
	}

	public unsafe virtual string SourceObjectName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EImportedEntity_002EGetSourceObjectName((global::AssetObjects.ImportedEntity*)m_pkEntity));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EImportedEntity_002ESetSourceObjectName((global::AssetObjects.ImportedEntity*)m_pkEntity, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string SourceFilePath
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EImportedEntity_002EGetSourceFilePath((global::AssetObjects.ImportedEntity*)m_pkEntity));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EImportedEntity_002ESetSourceFilePath((global::AssetObjects.ImportedEntity*)m_pkEntity, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool ReadyForExport()
	{
		int num = ((!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrEmpty(ClassName)) ? 1 : 0);
		return (byte)num != 0;
	}

	public virtual void UpdateImportedTime()
	{
		DateTime now = DateTime.Now;
		ImportedTime = now;
		NewEntity = false;
	}

	public virtual void UpdateExportedTime()
	{
		DateTime now = DateTime.Now;
		ExportedTime = now;
	}

	public virtual void ResetImportedTime()
	{
		DateTime importedTime = new DateTime(1989, 1, 22);
		ImportedTime = importedTime;
	}

	public virtual void ResetExportedTime()
	{
		DateTime exportedTime = new DateTime(1989, 1, 22);
		ExportedTime = exportedTime;
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		DateTime.FromFileTime((long)global::_003CModule_003E.AssetObjects_002EImportedEntity_002EGetExportedTime((global::AssetObjects.ImportedEntity*)m_pkEntity));
		DateTime.FromFileTime((long)global::_003CModule_003E.AssetObjects_002EImportedEntity_002EGetImportedTime((global::AssetObjects.ImportedEntity*)m_pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		base.RemoveReferences(bDisposing);
	}

	protected unsafe ImportedEntity(global::AssetObjects.InstanceEntity* pkInstanceEntity, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base(pkInstanceEntity, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}
}
