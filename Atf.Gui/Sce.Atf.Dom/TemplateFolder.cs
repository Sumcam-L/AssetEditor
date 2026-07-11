using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public abstract class TemplateFolder : DomNodeAdapter
{
	public abstract string Name { get; set; }

	public abstract IList<Template> Templates { get; }

	public abstract IList<TemplateFolder> Folders { get; }

	public abstract Uri Url { get; set; }
}
