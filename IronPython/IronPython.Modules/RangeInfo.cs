namespace IronPython.Modules;

internal class RangeInfo : CharInfo
{
	internal readonly int First;

	internal readonly int Last;

	internal RangeInfo(int first, int last, string[] info)
		: base(info)
	{
		First = first;
		Last = last;
	}
}
