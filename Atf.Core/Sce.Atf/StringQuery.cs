namespace Sce.Atf;

public enum StringQuery
{
	None = 0,
	Matches = 1,
	Contains = 2,
	BeginsWith = 4,
	EndsWith = 8,
	RegularExpression = 16,
	All = 255
}
