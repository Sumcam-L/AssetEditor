using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering;

public interface IBinding : IAdaptable
{
	string BindingType { get; set; }

	object Source { get; set; }
}
