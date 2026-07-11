using System;

namespace Sce.Atf;

public interface IResourceReference : IReference<IResource>
{
	Uri Uri { get; set; }
}
