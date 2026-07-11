using System;
using System.Collections.Generic;

namespace Firaxis.Collections;

public class DictionaryTesting
{
	private HashSet<int> hash1 = new HashSet<int>();

	private HashSet<int> hash2 = new HashSet<int>();

	public DictionaryTesting()
	{
		hash1.Add(1);
		hash1.Add(2);
		hash1.Add(3);
		hash2.Add(11);
		hash2.Add(22);
		hash2.Add(33);
	}

	public void TestMultiDictionary()
	{
		Console.WriteLine("TESTING CLASS MultiDictionary\n");
		MultiDictionary<string, int> multiDictionary = new MultiDictionary<string, int>();
		Console.WriteLine("MultiDictionary.Add(TKey key, TValue value)");
		multiDictionary.Add("H", 1);
		multiDictionary.Add("U", 2);
		multiDictionary.Add("A", 3);
		multiDictionary.Add("H", 5);
		multiDictionary.Add("H", 1);
		Console.WriteLine(multiDictionary.ToString());
		Console.WriteLine("\nMultiDictionary.AddRange(TKey key, HashSet<TValue> newValues)");
		multiDictionary.AddRange("S", hash1);
		multiDictionary.AddRange("H", hash2);
		multiDictionary.AddRange("U", hash1);
		Console.WriteLine(multiDictionary.ToString());
		Console.WriteLine("\nMultiDictionary.Contains(TKey key, TValue value)");
		Console.WriteLine("Contains H 3: " + multiDictionary.Contains("H", 1));
		Console.WriteLine("Contains H 28: " + multiDictionary.Contains("H", 28));
		Console.WriteLine("Contains F 3: " + multiDictionary.Contains("F", 1));
		Console.WriteLine("Contains F 28: " + multiDictionary.Contains("F", 28));
		Console.WriteLine("\nMultiDictionary.ContainsValue(TValue value)");
		Console.WriteLine("Contains 3: " + multiDictionary.ContainsValue(3));
		Console.WriteLine("Contains 28: " + multiDictionary.ContainsValue(28));
		Console.WriteLine("\nMultiDictionary.Remove(TKey key, TValue value)");
		multiDictionary.Remove("H", 1);
		multiDictionary.Remove("H", 28);
		multiDictionary.Remove("F", 3);
		multiDictionary.Remove("F", 28);
		Console.WriteLine(multiDictionary.ToString());
		Console.WriteLine(multiDictionary.Contains("H", 1));
		Console.WriteLine("\nMultiDictionary.Remove(TValue value)");
		multiDictionary.Remove(3);
		multiDictionary.Remove(28);
		Console.WriteLine(multiDictionary.ToString());
		Console.WriteLine(multiDictionary.ContainsValue(3));
		Console.WriteLine("\nMultiDictionary.IEnumerable<TValue> GetValues()");
		Console.Write("Values: ");
		foreach (int value in multiDictionary.GetValues())
		{
			Console.Write(value + ", ");
		}
	}

	public void TestUniqueMultiDictionary()
	{
		Console.WriteLine("\n\n\nTESTING CLASS UniqueMultiDictionary\n");
		UniqueMultiDictionary<string, int> uniqueMultiDictionary = new UniqueMultiDictionary<string, int>();
		Console.WriteLine("UniqueMultiDictionary.Add(TKey key, TValue value)");
		uniqueMultiDictionary.Add("H", 1);
		uniqueMultiDictionary.Add("U", 2);
		uniqueMultiDictionary.Add("A", 3);
		uniqueMultiDictionary.Add("H", 5);
		Console.WriteLine(uniqueMultiDictionary.ToString());
		Console.WriteLine("\nUniqueMultiDictionary.AddRange(TKey key, HashSet<TValue> newValues)");
		uniqueMultiDictionary.AddRange("H", hash2);
		Console.WriteLine(uniqueMultiDictionary.ToString());
		Console.WriteLine("\nUniqueMultiDictionary.Contains(TKey key, TValue value)");
		Console.WriteLine("Contains H 3: " + uniqueMultiDictionary.Contains("H", 1));
		Console.WriteLine("Contains H 28: " + uniqueMultiDictionary.Contains("H", 28));
		Console.WriteLine("Contains F 28: " + uniqueMultiDictionary.Contains("F", 28));
		Console.WriteLine("\nUniqueMultiDictionary.ContainsValue(TValue value)");
		Console.WriteLine("Contains 3: " + uniqueMultiDictionary.ContainsValue(3));
		Console.WriteLine("Contains 28: " + uniqueMultiDictionary.ContainsValue(28));
		Console.WriteLine("\nUniqueMultiDictionary.Remove(TKey key, TValue value)");
		uniqueMultiDictionary.Remove("H", 1);
		uniqueMultiDictionary.Remove("H", 28);
		uniqueMultiDictionary.Remove("F", 3);
		uniqueMultiDictionary.Remove("F", 28);
		Console.WriteLine(uniqueMultiDictionary.ToString());
		Console.WriteLine(uniqueMultiDictionary.Contains("H", 1));
		Console.WriteLine("\nUniqueMultiDictionary.Remove(TValue value)");
		uniqueMultiDictionary.Remove(3);
		uniqueMultiDictionary.Remove(28);
		Console.WriteLine(uniqueMultiDictionary.ToString());
		Console.WriteLine(uniqueMultiDictionary.ContainsValue(3));
		Console.WriteLine("\nUniqueMultiDictionary.IEnumerable<TValue> GetValues()");
		Console.Write("Values: ");
		foreach (int value in uniqueMultiDictionary.GetValues())
		{
			Console.Write(value + ", ");
		}
	}
}
