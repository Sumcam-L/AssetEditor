using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public interface IDynamicTypeDescriptor : ICustomTypeDescriptor
{
	bool CacheableProperties { get; }
}
