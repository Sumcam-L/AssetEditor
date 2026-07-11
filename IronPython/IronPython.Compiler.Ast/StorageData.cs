using System;
using IronPython.Runtime;

namespace IronPython.Compiler.Ast;

public static class StorageData
{
	public const int StaticFields = 50;

	public const int ContextTypes = 1;

	public const int ConstantTypes = 5;

	public const int SymbolTypes = 7;

	public const int GlobalTypes = 20;

	public const int SiteTypes = 30;

	internal static int ContextCount;

	internal static int ConstantCount;

	internal static int SymbolCount;

	internal static int GlobalCount;

	public static CodeContext[] Contexts = new CodeContext[64];

	public static object[] Constants = new object[64];

	public static PythonGlobal[] Globals = new PythonGlobal[64];

	public static readonly object SiteLockObj = new object();

	public static Type ContextStorageType(int index)
	{
		if (index < 50)
		{
			return typeof(ContextStorage000);
		}
		int num = checked(index - 50 + 1);
		if (Contexts.Length < num)
		{
			int num2;
			for (num2 = Contexts.Length; num2 < num; num2 *= 2)
			{
			}
			Array.Resize(ref Contexts, num2);
		}
		return typeof(StorageData);
	}

	public static Type ConstantStorageType(int index)
	{
		switch (index / 50)
		{
		case 0:
			return typeof(ConstantStorage000);
		case 1:
			return typeof(ConstantStorage001);
		case 2:
			return typeof(ConstantStorage002);
		case 3:
			return typeof(ConstantStorage003);
		case 4:
			return typeof(ConstantStorage004);
		default:
		{
			int num = checked(index - 250 + 1);
			if (Constants.Length < num)
			{
				int num2;
				for (num2 = Constants.Length; num2 < num; num2 *= 2)
				{
				}
				Array.Resize(ref Constants, num2);
			}
			return typeof(StorageData);
		}
		}
	}

	public static Type GlobalStorageType(int index)
	{
		switch (index / 50)
		{
		case 0:
			return typeof(GlobalStorage000);
		case 1:
			return typeof(GlobalStorage001);
		case 2:
			return typeof(GlobalStorage002);
		case 3:
			return typeof(GlobalStorage003);
		case 4:
			return typeof(GlobalStorage004);
		case 5:
			return typeof(GlobalStorage005);
		case 6:
			return typeof(GlobalStorage006);
		case 7:
			return typeof(GlobalStorage007);
		case 8:
			return typeof(GlobalStorage008);
		case 9:
			return typeof(GlobalStorage009);
		case 10:
			return typeof(GlobalStorage010);
		case 11:
			return typeof(GlobalStorage011);
		case 12:
			return typeof(GlobalStorage012);
		case 13:
			return typeof(GlobalStorage013);
		case 14:
			return typeof(GlobalStorage014);
		case 15:
			return typeof(GlobalStorage015);
		case 16:
			return typeof(GlobalStorage016);
		case 17:
			return typeof(GlobalStorage017);
		case 18:
			return typeof(GlobalStorage018);
		case 19:
			return typeof(GlobalStorage019);
		default:
		{
			int num = checked(index - 1000 + 1);
			if (Globals.Length < num)
			{
				int num2;
				for (num2 = Globals.Length; num2 < num; num2 *= 2)
				{
				}
				Array.Resize(ref Globals, num2);
			}
			return typeof(StorageData);
		}
		}
	}
}
