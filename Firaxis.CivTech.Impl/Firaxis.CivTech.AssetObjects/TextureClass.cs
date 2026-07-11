using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TextureClass : ClassEntity, ITextureClass
{
	private TextureExportOptions m_pmExportOptions = null;

	public virtual ITextureExportOptions ExportOptions => m_pmExportOptions;

	public unsafe TextureClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ATextureClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe TextureClass(global::AssetObjects.TextureClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		m_pmExportOptions = new TextureExportOptions(global::_003CModule_003E.AssetObjects_002ETextureClass_002EGetExportOptions((global::AssetObjects.TextureClass*)m_pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmExportOptions.RemoveReferences();
		if (bDisposing)
		{
			m_pmExportOptions = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
