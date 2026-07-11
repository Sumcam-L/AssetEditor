using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering;

public interface IDataSet : IAdaptable, INameable
{
	float[] Data { get; set; }

	int ElementSize { get; set; }
}
