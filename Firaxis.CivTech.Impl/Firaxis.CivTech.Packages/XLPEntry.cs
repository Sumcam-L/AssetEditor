using System;
using AssetObjects;

namespace Firaxis.CivTech.Packages;

public class XLPEntry : IXLPEntry
{
	private string m_id = string.Empty;

	private string m_objectName = string.Empty;

	public virtual string ObjectName
	{
		get
		{
			return m_objectName;
		}
		set
		{
			m_objectName = value;
		}
	}

	public virtual string ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public override string ToString()
	{
		return $"ID: {ID}, Object: {ObjectName}";
	}

	public unsafe void ToNative(global::AssetObjects.XLP* xlp)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(ID);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(ObjectName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.AssetObjects_002EXLP_002EAddEntry(xlp, standardStringWrapper.Value, standardStringWrapper2.Value);
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

	public unsafe void FromNative(global::AssetObjects.XLPEntry* entry)
	{
		ID = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLPEntry_002EGetID(entry));
		ObjectName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLPEntry_002EGetObjectName(entry));
	}
}
