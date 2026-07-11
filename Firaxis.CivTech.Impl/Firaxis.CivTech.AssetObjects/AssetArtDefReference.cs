using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AssetArtDefReference : IAssetArtDefReference
{
	private unsafe global::AssetObjects.AssetArtDefReference* m_pkArtDefReference;

	public unsafe virtual IEnumerable<string> Tags
	{
		get
		{
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Ebegin_tags(m_pkArtDefReference, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Eend_tags(m_pkArtDefReference, &iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Eend_tags(m_pkArtDefReference, &iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual string CollectionName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002EGetCollectionName(m_pkArtDefReference));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			IntPtr hglobal = Marshal.StringToHGlobalAnsi(value);
			sbyte* ptr = (sbyte*)hglobal.ToPointer();
			global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002ESetCollectionName(m_pkArtDefReference, ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string TemplateName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002EGetTemplateName(m_pkArtDefReference));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			IntPtr hglobal = Marshal.StringToHGlobalAnsi(value);
			sbyte* ptr = (sbyte*)hglobal.ToPointer();
			global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002ESetTemplateName(m_pkArtDefReference, ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe AssetArtDefReference(global::AssetObjects.AssetArtDefReference* pkAssetArtDefReference)
	{
		m_pkArtDefReference = pkAssetArtDefReference;
		base._002Ector();
	}

	public unsafe virtual void AddTag(string tag)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(tag).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002EAddTag(m_pkArtDefReference, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void RemoveTag(string tag)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(tag).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002ERemoveTag(m_pkArtDefReference, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void ClearTags()
	{
		global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002EClearTags(m_pkArtDefReference);
	}

	public unsafe virtual string FlattenTagsToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Ebegin_tags(m_pkArtDefReference, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Eend_tags(m_pkArtDefReference, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				string text = Marshal.PtrToStringAnsi(ptr);
				stringBuilder.Append(text + ", ");
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetArtDefReference_002Eend_tags(m_pkArtDefReference, &iterator2)));
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 2, 2);
		}
		return stringBuilder.ToString();
	}

	public virtual void SetTagsFromString(string tags)
	{
		string[] array = tags.Split(',');
		ClearTags();
		int num = 0;
		if (0 < (nint)array.LongLength)
		{
			do
			{
				string text = array[num];
				AddTag(text.Trim());
				num++;
			}
			while (num < (nint)array.LongLength);
		}
	}

	public unsafe global::AssetObjects.AssetArtDefReference* GetAssetObject()
	{
		return m_pkArtDefReference;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkArtDefReference = null;
	}

	internal unsafe global::AssetObjects.AssetArtDefReference* GetUnmanaged()
	{
		return m_pkArtDefReference;
	}

	internal unsafe void SetUnmanaged(global::AssetObjects.AssetArtDefReference* p)
	{
		m_pkArtDefReference = p;
	}
}
