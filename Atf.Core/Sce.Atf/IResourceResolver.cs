using System;

namespace Sce.Atf;

public interface IResourceResolver
{
	IResource Resolve(Uri uri);
}
