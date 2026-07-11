namespace IronPython.Modules;

internal class CharInfo
{
	private static class PropertyIndex
	{
		internal const int Name = 0;

		internal const int General_Category = 1;

		internal const int Canonical_Combining_Class = 2;

		internal const int Bidi_Class = 3;

		internal const int Decomposition_Type = 4;

		internal const int Numeric_Value_Decimal = 5;

		internal const int Numeric_Value_Digit = 6;

		internal const int Numeric_Value_Numeric = 7;

		internal const int Bidi_Mirrored = 8;

		internal const int East_Asian_Width = 9;
	}

	internal readonly string Name;

	internal readonly string General_Category;

	internal readonly int Canonical_Combining_Class;

	internal readonly string Bidi_Class;

	internal readonly string Decomposition_Type;

	internal readonly int? Numeric_Value_Decimal;

	internal readonly int? Numeric_Value_Digit;

	internal readonly double? Numeric_Value_Numeric;

	internal readonly int Bidi_Mirrored;

	internal readonly string East_Asian_Width;

	internal CharInfo(string[] info)
	{
		Name = info[0].ToUpperInvariant();
		General_Category = info[1];
		Canonical_Combining_Class = int.Parse(info[2]);
		Bidi_Class = info[3];
		Decomposition_Type = info[4];
		string text = info[5];
		Numeric_Value_Decimal = ((text != "") ? new int?(int.Parse(text)) : ((int?)null));
		string text2 = info[6];
		Numeric_Value_Digit = ((text2 != "") ? new int?(int.Parse(text2)) : ((int?)null));
		string text3 = info[7];
		if (text3 != "")
		{
			string[] array = text3.Split('/');
			double num = double.Parse(array[0]);
			if (array.Length > 1)
			{
				double num2 = double.Parse(array[1]);
				num /= num2;
			}
			Numeric_Value_Numeric = num;
		}
		else
		{
			Numeric_Value_Numeric = null;
		}
		Bidi_Mirrored = ((info[8] == "Y") ? 1 : 0);
		East_Asian_Width = info[9];
	}
}
