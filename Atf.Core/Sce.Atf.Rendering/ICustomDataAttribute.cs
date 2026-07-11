namespace Sce.Atf.Rendering;

public interface ICustomDataAttribute : INameable
{
	string DataType { get; set; }

	string ValueAttr { get; set; }

	string Default { get; set; }

	string Min { get; set; }

	string Max { get; set; }

	int Count { get; set; }

	int Index { get; set; }

	bool isArray { get; set; }

	object Value { get; set; }
}
