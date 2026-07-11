using System;
using System.ComponentModel.Composition;

namespace Sce.Atf.Wpf;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ExportViewModelAttribute : ExportAttribute, IViewModelMetadata
{
	public string Name { get; private set; }

	public ExportViewModelAttribute(string name)
		: base("ViewModel")
	{
		Name = name;
	}
}
