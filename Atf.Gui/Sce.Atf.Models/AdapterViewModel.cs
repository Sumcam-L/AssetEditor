using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Models;

public class AdapterViewModel : Adapter, INotifyPropertyChanged
{
	private object m_adapterObject;

	[Browsable(false)]
	public new object As
	{
		get
		{
			if (m_adapterObject == null)
			{
				DomNode domNode = base.Adaptee.As<DomNode>();
				if (domNode != null)
				{
					m_adapterObject = new DomBindingAdapterObject(domNode, enableNodeTypeExtensionOptimisation: true);
				}
				else
				{
					m_adapterObject = new BindingAdapterObject(this);
				}
			}
			return m_adapterObject;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public AdapterViewModel()
	{
	}

	public AdapterViewModel(object adaptee)
		: base(adaptee)
	{
	}

	protected override void OnAdapteeChanged(object oldAdaptee)
	{
		base.OnAdapteeChanged(oldAdaptee);
		m_adapterObject = null;
		OnPropertyChanged(new PropertyChangedEventArgs("As"));
	}

	protected virtual void RaisePropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}
}
