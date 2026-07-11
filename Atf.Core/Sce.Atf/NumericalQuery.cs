namespace Sce.Atf;

public enum NumericalQuery
{
	None = 0,
	Equals = 1,
	Lesser = 2,
	LesserEqual = 4,
	Equal = 8,
	GreaterEqual = 16,
	Greater = 32,
	Between = 64,
	All = 255
}
