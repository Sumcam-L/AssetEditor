using System.Windows.Data;

namespace Sce.Atf.Wpf.Controls;

public class AncestorTypeBinding<T> : Binding where T : class
{
	private AncestorTypeBinding()
	{
	}

	public AncestorTypeBinding(string path)
		: this(path, 1)
	{
	}

	public AncestorTypeBinding(string path, int ancestorLevel)
		: base(path)
	{
		base.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(T), ancestorLevel);
	}
}
