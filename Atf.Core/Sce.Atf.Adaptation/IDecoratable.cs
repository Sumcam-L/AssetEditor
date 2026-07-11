using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation;

public interface IDecoratable
{
	IEnumerable<object> GetDecorators(Type type);
}
