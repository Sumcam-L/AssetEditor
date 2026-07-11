using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyTexture : GrannyBaseObjectContext, IGrannyTexture
{
	private unsafe granny_texture* m_pkTexture = null;

	public unsafe virtual string FromFileName
	{
		get
		{
            //IL_001d->IL001d: Incompatible stack types: I8 vs Ref
            if (m_pkTexture == null) return null;
            ulong ptr = *(ulong*)m_pkTexture;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
		set
		{
			//IL_0025: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_pkTexture, (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr));
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe GrannyTexture(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyTexture()
	{
	}

	private void _0021GrannyTexture()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		m_pkTexture = (granny_texture*)GetGrannyObject();
		return result;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyTextureType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkTexture;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_texture* pkTexture)
	{
		m_pkTexture = pkTexture;
		return true;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyTexture();
			return;
		}
		try
		{
			_0021GrannyTexture();
		}
		finally
		{
			
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyTexture()
	{
		Dispose(A_0: false);
	}
}
