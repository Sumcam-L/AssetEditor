using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

internal static class Mro
{
	public static List<PythonType> Calculate(PythonType startingType, IList<PythonType> bases)
	{
		return Calculate(startingType, new List<PythonType>(bases), forceNewStyle: false);
	}

	public static List<PythonType> Calculate(PythonType startingType, IList<PythonType> baseTypes, bool forceNewStyle)
	{
		List<PythonType> list = new List<PythonType>();
		foreach (PythonType baseType in baseTypes)
		{
			list.Add(baseType);
		}
		if (list.Contains(startingType))
		{
			throw PythonOps.TypeError("a __bases__ item causes an inheritance cycle ({0})", startingType.Name);
		}
		List<PythonType> list2 = new List<PythonType>();
		list2.Add(startingType);
		if (list.Count != 0)
		{
			List<IList<PythonType>> list3 = new List<IList<PythonType>>();
			int num = 0;
			foreach (PythonType item in list)
			{
				if (item.IsOldClass)
				{
					num++;
				}
			}
			foreach (PythonType item2 in list)
			{
				if (!item2.IsOldClass)
				{
					list3.Add(TupleToList(item2.ResolutionOrder));
				}
				else if (num == 1 && !forceNewStyle)
				{
					list3.Add(GetOldStyleMro(item2));
				}
				else
				{
					list3.Add(GetNewStyleMro(item2));
				}
			}
			list3.Add(TupleToList(list));
			while (true)
			{
				bool flag = false;
				bool flag2 = false;
				PythonType pythonType = null;
				for (int i = 0; i < list3.Count; i++)
				{
					if (list3[i].Count == 0)
					{
						continue;
					}
					flag2 = true;
					PythonType pythonType2 = (pythonType = list3[i][0]);
					bool flag3 = false;
					for (int j = 0; j < list3.Count; j++)
					{
						if (list3[j].Count != 0 && !list3[j][0].Equals(pythonType2) && list3[j].Contains(pythonType2))
						{
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						continue;
					}
					if (list2.Contains(pythonType2))
					{
						throw PythonOps.TypeError("a __bases__ item causes an inheritance cycle");
					}
					list2.Add(pythonType2);
					for (int k = 0; k < list3.Count; k++)
					{
						list3[k].Remove(pythonType2);
					}
					flag = true;
					break;
				}
				if (!flag2)
				{
					break;
				}
				if (flag)
				{
					continue;
				}
				PythonType pythonType3 = null;
				string text = $"Cannot create a consistent method resolution\norder (MRO) for bases {pythonType.Name}";
				for (int l = 0; l < list3.Count; l++)
				{
					if (list3[l].Count != 0 && !list3[l][0].Equals(pythonType))
					{
						pythonType3 = list3[l][0];
						text += ", ";
						text += pythonType3.Name;
					}
				}
				throw PythonOps.TypeError(text);
			}
		}
		return list2;
	}

	private static IList<PythonType> TupleToList(IList<PythonType> t)
	{
		return new List<PythonType>(t);
	}

	private static IList<PythonType> GetOldStyleMro(PythonType oldStyleType)
	{
		List<PythonType> list = new List<PythonType>();
		GetOldStyleMroWorker(oldStyleType, list);
		return list;
	}

	private static void GetOldStyleMroWorker(PythonType curType, List<PythonType> res)
	{
		if (res.Contains(curType))
		{
			return;
		}
		res.Add(curType);
		foreach (PythonType baseType in curType.BaseTypes)
		{
			GetOldStyleMroWorker(baseType, res);
		}
	}

	private static IList<PythonType> GetNewStyleMro(PythonType oldStyleType)
	{
		List<PythonType> list = new List<PythonType>();
		list.Add(oldStyleType);
		foreach (PythonType baseType in oldStyleType.BaseTypes)
		{
			list.AddRange(TupleToList(Calculate(baseType, baseType.BaseTypes, forceNewStyle: true)));
		}
		return list;
	}
}
