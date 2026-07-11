using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class BindingAdapter : ObservableDomNodeAdapter
{
	private BindingAdapterObject m_adapterObject;

	private static bool s_enableOptimisation = true;

	public object As => m_adapterObject ?? (m_adapterObject = new DomBindingAdapterObject(base.DomNode, s_enableOptimisation));

	public static bool EnableNodeTypeExtensionOptimisation
	{
		get
		{
			return s_enableOptimisation;
		}
		set
		{
			s_enableOptimisation = value;
		}
	}
}
