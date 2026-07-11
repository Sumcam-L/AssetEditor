using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DependencyAttribute : Attribute
{
	public string[] DependencyDescriptors { get; set; }

	public string[] DependencyDescriptor { get; set; }

	public DependencyAttribute(string dependencyDescriptor)
	{
		DependencyDescriptor = new string[1] { dependencyDescriptor };
	}

	public DependencyAttribute(string[] dependencyDescriptors)
	{
		DependencyDescriptors = dependencyDescriptors;
	}
}
