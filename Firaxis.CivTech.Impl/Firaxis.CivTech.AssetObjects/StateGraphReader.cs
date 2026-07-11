using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using DDT.AnimationGraph;
using DDT.StateGraph;
using Platform;
using Reflection;
using Serialization;
using String;

namespace Firaxis.CivTech.AssetObjects;

public class StateGraphReader : IStateGraphReader
{
	private unsafe DDT.StateGraph.Root* m_pkRoot;

	private IStateGraphContainer m_pmContainer;

	private AnimationGraphReader m_pmAnimationGraphReader;

	public unsafe StateGraphReader()
	{
		//IL_0029: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		DDT.StateGraph.Root* ptr = (DDT.StateGraph.Root*)global::_003CModule_003E.@new(48uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040EOCOOGMP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 11, 23, 0);
		DDT.StateGraph.Root* pkRoot;
		try
		{
			pkRoot = ((ptr == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002ERoot_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040EOCOOGMP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 11, 23, 0);
			throw;
		}
		m_pkRoot = pkRoot;
		m_pmContainer = null;
		m_pmAnimationGraphReader = new AnimationGraphReader();
		base._002Ector();
		global::_003CModule_003E.DDT_002ERegisterTypeInfo();
	}

	private void _007EStateGraphReader()
	{
		_0021StateGraphReader();
	}

	private unsafe void _0021StateGraphReader()
	{
		//IL_0025: Expected I, but got I8
		//IL_0014: Expected I, but got I8
		DDT.StateGraph.Root* pkRoot = m_pkRoot;
		if (pkRoot != null)
		{
			global::_003CModule_003E.String_002EGlobal_002E_007Bdtor_007D((Global*)((ulong)(nint)pkRoot + 16uL));
			global::_003CModule_003E.delete(pkRoot, 48uL);
		}
		m_pkRoot = null;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool Load(string pmFilename, IStateGraphContainer pmContainer)
	{
		//IL_0075: Expected I, but got I8
		//IL_009c: Expected I, but got I8
		//IL_00fc: Expected I, but got I8
		//IL_0114: Expected I, but got I8
		m_pmContainer = pmContainer;
		if (string.IsNullOrEmpty(pmFilename))
		{
			return false;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FileIStream fileIStream);
		global::_003CModule_003E.Serialization_002EFileIStream_002E_007Bctor_007D(&fileIStream);
		bool result;
		try
		{
			char* ptr = (char*)Marshal.StringToHGlobalUni(pmFilename).ToPointer();
			bool flag = global::_003CModule_003E.Serialization_002EFileIStream_002EOpen(&fileIStream, ptr) == (IO_RESULT)0;
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (!flag)
			{
				result = false;
			}
			else
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out Context context);
				global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D(&context);
				try
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Global global);
					Global* ptr2 = &global;
					Global* ptr3 = global::_003CModule_003E.String_002EGlobal_002E_007Bctor_007D(&global, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04OJAHODMC_0040Text_003F_0024AA_0040));
					global::_003CModule_003E.Serialization_002EContext_002EAddContext(&context, ptr3, (void*)1);
					global::_003CModule_003E.XMLSerialization_002E_003FA0xe03e37de_002EDeserialize_003Cclass_0020DDT_003A_003AStateGraph_003A_003ARoot_003E(m_pkRoot, (IStream*)(&fileIStream), &context);
					global::_003CModule_003E.Serialization_002EFileIStream_002EClose(&fileIStream);
					IntPtr ptr4 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)m_pkRoot + 16uL)));
					DDT.StateGraph.Root* pkRoot = m_pkRoot;
					m_pmContainer.AddRoot(*(int*)((ulong)(nint)m_pkRoot + 24uL), Marshal.PtrToStringAnsi(ptr4), *(int*)((ulong)(nint)pkRoot + 32uL), *(int*)((ulong)(nint)pkRoot + 36uL));
					m_pmContainer.AddOrphanRoot(*(int*)((ulong)(nint)m_pkRoot + 28uL));
					pkRoot = m_pkRoot;
					PublishGraphNode(*(int*)((ulong)(nint)pkRoot + 24uL), (Node*)(*(ulong*)pkRoot));
					DDT.StateGraph.Root* pkRoot2 = m_pkRoot;
					PublishGraphNode(*(int*)((ulong)(nint)pkRoot2 + 28uL), (Node*)(*(ulong*)((ulong)(nint)pkRoot2 + 8uL)));
					global::_003CModule_003E.DDT_002EReleaseStateGraph(m_pkRoot);
					m_pmContainer = null;
					result = true;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &context);
					throw;
				}
				global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D(&context);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FileIStream*, void>)(&global::_003CModule_003E.Serialization_002EFileIStream_002E_007Bdtor_007D), &fileIStream);
			throw;
		}
		global::_003CModule_003E.Serialization_002EFileIStream_002E_007Bdtor_007D(&fileIStream);
		return result;
	}

	private unsafe void PublishGraphNode(int iParentID, Node* pkNode)
	{
		//IL_0012: Expected I, but got I8
		//IL_0055: Expected I, but got I8
		//IL_00a9: Expected I, but got I8
		//IL_018b: Expected I, but got I8
		//IL_01e5: Expected I, but got I8
		//IL_019a: Expected I, but got I8
		//IL_01f6: Expected I, but got I8
		if (pkNode == null)
		{
			return;
		}
		IntPtr ptr = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)pkNode + 16uL)));
		string sText = Marshal.PtrToStringAnsi(ptr);
		switch (*(int*)((ulong)(nint)pkNode + 24uL))
		{
		case 2:
		{
			m_pmContainer.AddSource(iParentID, *(int*)((ulong)(nint)pkNode + 28uL), sText, *(int*)((ulong)(nint)pkNode + 32uL), *(int*)((ulong)(nint)pkNode + 36uL));
			IntPtr ptr4 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)pkNode + 48uL)));
			m_pmContainer.SetSourceStateName(*(int*)((ulong)(nint)pkNode + 28uL), Marshal.PtrToStringAnsi(ptr4));
			break;
		}
		case 3:
		{
			m_pmContainer.AddDestination(iParentID, *(int*)((ulong)(nint)pkNode + 28uL), sText, *(int*)((ulong)(nint)pkNode + 32uL), *(int*)((ulong)(nint)pkNode + 36uL));
			IntPtr ptr5 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)pkNode + 48uL)));
			m_pmContainer.SetDestinationStateName(*(int*)((ulong)(nint)pkNode + 28uL), Marshal.PtrToStringAnsi(ptr5));
			m_pmContainer.SetDestinationLoop(*(int*)((ulong)(nint)pkNode + 28uL), *(bool*)((ulong)(nint)pkNode + 64uL));
			m_pmContainer.SetDestinationBlendDuration(*(int*)((ulong)(nint)pkNode + 28uL), *(float*)((ulong)(nint)pkNode + 60uL));
			m_pmContainer.SetDestinationRandomOffset(*(int*)((ulong)(nint)pkNode + 28uL), *(bool*)((ulong)(nint)pkNode + 65uL));
			m_pmContainer.SetDestinationContinueOffset(*(int*)((ulong)(nint)pkNode + 28uL), *(bool*)((ulong)(nint)pkNode + 66uL));
			break;
		}
		case 4:
			m_pmContainer.AddAnimationGraph(iParentID, *(int*)((ulong)(nint)pkNode + 28uL), sText, *(int*)((ulong)(nint)pkNode + 32uL), *(int*)((ulong)(nint)pkNode + 36uL));
			m_pmContainer.SetAnimationGraphPercentChance(*(int*)((ulong)(nint)pkNode + 28uL), *(float*)((ulong)(nint)pkNode + 440uL) * 100f);
			m_pmAnimationGraphReader.Load((DDT.AnimationGraph.Root*)((ulong)(nint)pkNode + 48uL), m_pmContainer.GetAnimationGraph(*(int*)((ulong)(nint)pkNode + 28uL)));
			break;
		case 1:
		{
			TypeInfo* ptr2 = (TypeInfo*)(*(ulong*)((ulong)(nint)pkNode + 48uL));
			if (ptr2 != null)
			{
				IntPtr ptr3 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)ptr2));
				m_pmContainer.AddData(iParentID, *(int*)((ulong)(nint)pkNode + 28uL), Marshal.PtrToStringAnsi(ptr3), sText, *(int*)((ulong)(nint)pkNode + 32uL), *(int*)((ulong)(nint)pkNode + 36uL));
			}
			break;
		}
		}
		ulong num = *(ulong*)pkNode;
		if (num != 0L)
		{
			PublishGraphNode(*(int*)((ulong)(nint)pkNode + 28uL), (Node*)num);
		}
		ulong num2 = *(ulong*)((ulong)(nint)pkNode + 8uL);
		if (num2 != 0L)
		{
			PublishGraphNode(iParentID, (Node*)num2);
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021StateGraphReader();
			return;
		}
		try
		{
			_0021StateGraphReader();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~StateGraphReader()
	{
		Dispose(A_0: false);
	}
}
