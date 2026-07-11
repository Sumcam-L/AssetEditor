using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class ArrayOps
{
	[SpecialName]
	public static Array Add(Array data1, Array data2)
	{
		if (data1 == null)
		{
			throw PythonOps.TypeError("expected array for 1st argument, got None");
		}
		if (data2 == null)
		{
			throw PythonOps.TypeError("expected array for 2nd argument, got None");
		}
		if (data1.Rank > 1 || data2.Rank > 1)
		{
			throw new NotImplementedException("can't add multidimensional arrays");
		}
		Type type = data1.GetType();
		Type type2 = data2.GetType();
		Type elementType = ((type == type2) ? type.GetElementType() : typeof(object));
		Array array = Array.CreateInstance(elementType, data1.Length + data2.Length);
		Array.Copy(data1, 0, array, 0, data1.Length);
		Array.Copy(data2, 0, array, data1.Length, data2.Length);
		return array;
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType pythonType, ICollection items)
	{
		Type elementType = pythonType.UnderlyingSystemType.GetElementType();
		Array array = Array.CreateInstance(elementType, items.Count);
		int num = 0;
		foreach (object item in items)
		{
			array.SetValue(Converter.Convert(item, elementType), num++);
		}
		return array;
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType pythonType, object items)
	{
		Type elementType = pythonType.UnderlyingSystemType.GetElementType();
		if (!PythonOps.TryGetBoundAttr(items, "__len__", out var ret))
		{
			throw PythonOps.TypeErrorForBadInstance("expected object with __len__ function, got {0}", items);
		}
		int length = PythonContext.GetContext(context).ConvertToInt32(PythonOps.CallWithContext(context, ret));
		Array array = Array.CreateInstance(elementType, length);
		IEnumerator enumerator = PythonOps.GetEnumerator(items);
		int num = 0;
		while (enumerator.MoveNext())
		{
			array.SetValue(Converter.Convert(enumerator.Current, elementType), num++);
		}
		return array;
	}

	[SpecialName]
	public static Array Multiply(Array data, int count)
	{
		if (data.Rank > 1)
		{
			throw new NotImplementedException("can't multiply multidimensional arrays");
		}
		Type elementType = data.GetType().GetElementType();
		if (count <= 0)
		{
			return Array.CreateInstance(elementType, 0);
		}
		int num = data.Length * count;
		Array array = Array.CreateInstance(elementType, num);
		Array.Copy(data, 0, array, 0, data.Length);
		int num2 = data.Length;
		int num3 = data.Length;
		while (num3 < num)
		{
			Array.Copy(array, 0, array, num3, Math.Min(num2, num - num3));
			num3 += num2;
			num2 *= 2;
		}
		return array;
	}

	[SpecialName]
	public static object GetItem(Array data, int index)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected Array, got None");
		}
		return data.GetValue(PythonOps.FixIndex(index, data.Length) + data.GetLowerBound(0));
	}

	[SpecialName]
	public static object GetItem(Array data, Slice slice)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected Array, got None");
		}
		return GetSlice(data, data.Length, slice);
	}

	[SpecialName]
	public static object GetItem(Array data, params object[] indices)
	{
		if (indices == null || indices.Length < 1)
		{
			throw PythonOps.TypeError("__getitem__ requires at least 1 parameter");
		}
		if (indices.Length == 1 && Converter.TryConvertToInt32(indices[0], out var result))
		{
			return GetItem(data, result);
		}
		data.GetType();
		int[] array = TupleToIndices(data, indices);
		if (data.Rank != indices.Length)
		{
			throw PythonOps.ValueError("bad dimensions for array, got {0} expected {1}", indices.Length, data.Rank);
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] += data.GetLowerBound(i);
		}
		return data.GetValue(array);
	}

	[SpecialName]
	public static void SetItem(Array data, int index, object value)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected Array, got None");
		}
		data.SetValue(Converter.Convert(value, data.GetType().GetElementType()), PythonOps.FixIndex(index, data.Length) + data.GetLowerBound(0));
	}

	[SpecialName]
	public static void SetItem(Array a, params object[] indexAndValue)
	{
		if (indexAndValue == null || indexAndValue.Length < 2)
		{
			throw PythonOps.TypeError("__setitem__ requires at least 2 parameters");
		}
		if (indexAndValue.Length == 2 && Converter.TryConvertToInt32(indexAndValue[0], out var result))
		{
			SetItem(a, result, indexAndValue[1]);
			return;
		}
		a.GetType();
		object[] array = ArrayUtils.RemoveLast(indexAndValue);
		int[] array2 = TupleToIndices(a, array);
		if (a.Rank != array.Length)
		{
			throw PythonOps.ValueError("bad dimensions for array, got {0} expected {1}", array.Length, a.Rank);
		}
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] += a.GetLowerBound(i);
		}
		a.SetValue(indexAndValue[indexAndValue.Length - 1], array2);
	}

	[SpecialName]
	public static void SetItem(Array a, Slice index, object value)
	{
		if (a.Rank != 1)
		{
			throw PythonOps.NotImplementedError("slice on multi-dimensional array");
		}
		Type elm = a.GetType().GetElementType();
		index.DoSliceAssign(delegate(int idx, object val)
		{
			a.SetValue(Converter.Convert(val, elm), idx + a.GetLowerBound(0));
		}, a.Length, value);
	}

	public static string __repr__(CodeContext context, [NotNull] Array self)
	{
		List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(self);
		if (andCheckInfinite == null)
		{
			return "...";
		}
		int count = andCheckInfinite.Count;
		andCheckInfinite.Add(self);
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (self.Rank == 1)
			{
				stringBuilder.Append("Array[");
				Type elementType = self.GetType().GetElementType();
				stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(elementType).Name);
				stringBuilder.Append("]");
				stringBuilder.Append("((");
				for (int i = 0; i < self.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(PythonOps.Repr(context, self.GetValue(i + self.GetLowerBound(0))));
				}
				stringBuilder.Append("))");
			}
			else
			{
				stringBuilder.Append("<");
				stringBuilder.Append(self.Rank);
				stringBuilder.Append(" dimensional Array[");
				Type elementType2 = self.GetType().GetElementType();
				stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(elementType2).Name);
				stringBuilder.Append("] at ");
				stringBuilder.Append(PythonOps.HexId(self));
				stringBuilder.Append(">");
			}
			return stringBuilder.ToString();
		}
		finally
		{
			andCheckInfinite.RemoveAt(count);
		}
	}

	internal static object[] Multiply(object[] data, int size, int count)
	{
		int num = checked(size * count);
		object[] array = CopyArray(data, num);
		if (count > 0)
		{
			int num2 = size;
			int num3 = size;
			while (num3 < num)
			{
				Array.Copy(array, 0, array, num3, Math.Min(num2, num - num3));
				num3 += num2;
				num2 *= 2;
			}
		}
		return array;
	}

	internal static object[] Add(object[] data1, int size1, object[] data2, int size2)
	{
		object[] array = CopyArray(data1, size1 + size2);
		Array.Copy(data2, 0, array, size1, size2);
		return array;
	}

	internal static object[] GetSlice(object[] data, int start, int stop)
	{
		if (stop <= start)
		{
			return ArrayUtils.EmptyObjects;
		}
		object[] array = new object[stop - start];
		int num = 0;
		for (int i = start; i < stop; i++)
		{
			array[num++] = data[i];
		}
		return array;
	}

	internal static object[] GetSlice(object[] data, int start, int stop, int step)
	{
		if (step == 1)
		{
			return GetSlice(data, start, stop);
		}
		int sliceSize = GetSliceSize(start, stop, step);
		if (sliceSize <= 0)
		{
			return ArrayUtils.EmptyObjects;
		}
		object[] array = new object[sliceSize];
		int num = 0;
		int num2 = start;
		while (num < array.Length)
		{
			array[num] = data[num2];
			num++;
			num2 += step;
		}
		return array;
	}

	internal static object[] GetSlice(object[] data, Slice slice)
	{
		slice.indices(data.Length, out var ostart, out var ostop, out var ostep);
		return GetSlice(data, ostart, ostop, ostep);
	}

	internal static Array GetSlice(Array data, int size, Slice slice)
	{
		if (data.Rank != 1)
		{
			throw PythonOps.NotImplementedError("slice on multi-dimensional array");
		}
		slice.indices(size, out var ostart, out var ostop, out var ostep);
		if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
		{
			if (data.GetType().GetElementType() == typeof(object))
			{
				return ArrayUtils.EmptyObjects;
			}
			return Array.CreateInstance(data.GetType().GetElementType(), 0);
		}
		if (ostep == 1)
		{
			int length = ostop - ostart;
			Array array = Array.CreateInstance(data.GetType().GetElementType(), length);
			Array.Copy(data, ostart + data.GetLowerBound(0), array, 0, length);
			return array;
		}
		int sliceSize = GetSliceSize(ostart, ostop, ostep);
		Array array2 = Array.CreateInstance(data.GetType().GetElementType(), sliceSize);
		int num = 0;
		int num2 = 0;
		int num3 = ostart;
		while (num2 < sliceSize)
		{
			array2.SetValue(data.GetValue(num3 + data.GetLowerBound(0)), num++);
			num2++;
			num3 += ostep;
		}
		return array2;
	}

	private static int GetSliceSize(int start, int stop, int step)
	{
		if (step <= 0)
		{
			return (stop - start + step + 1) / step;
		}
		return (stop - start + step - 1) / step;
	}

	internal static object[] CopyArray(object[] data, int newSize)
	{
		if (newSize == 0)
		{
			return ArrayUtils.EmptyObjects;
		}
		object[] array = new object[newSize];
		if (data.Length < 20)
		{
			for (int i = 0; i < data.Length && i < newSize; i++)
			{
				array[i] = data[i];
			}
		}
		else
		{
			Array.Copy(data, array, Math.Min(newSize, data.Length));
		}
		return array;
	}

	private static int[] TupleToIndices(Array a, IList<object> tuple)
	{
		int[] array = new int[tuple.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PythonOps.FixIndex(Converter.ConvertToInt32(tuple[i]), a.GetUpperBound(i) + 1);
		}
		return array;
	}
}
