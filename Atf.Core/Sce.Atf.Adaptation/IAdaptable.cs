using System;

namespace Sce.Atf.Adaptation;

public interface IAdaptable
{
	object GetAdapter(Type type);
}
