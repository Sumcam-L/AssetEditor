using System;

namespace Sce.Atf.Adaptation;

public interface IAdapterCreator
{
	bool CanAdapt(object adaptee, Type type);

	object GetAdapter(object adaptee, Type type);
}
