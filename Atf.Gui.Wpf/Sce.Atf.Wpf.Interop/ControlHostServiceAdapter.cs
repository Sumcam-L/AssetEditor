using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;
using WeifenLuo.WinFormsUI.Docking;

namespace Sce.Atf.Wpf.Interop;

[Export(typeof(Sce.Atf.Applications.IControlHostService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ControlHostServiceAdapter : Sce.Atf.Applications.IControlHostService
{
	private Sce.Atf.Wpf.Applications.IControlHostService m_adaptee;

	private Dictionary<Sce.Atf.Applications.IControlHostClient, ControlHostClientAdapter> m_clientAdapters = new Dictionary<Sce.Atf.Applications.IControlHostClient, ControlHostClientAdapter>();

	private Dictionary<Sce.Atf.Applications.ControlInfo, IControlInfo> m_info = new Dictionary<Sce.Atf.Applications.ControlInfo, IControlInfo>();

	public IEnumerable<Sce.Atf.Applications.ControlInfo> Controls => m_info.Keys;

	public ThemeBase Theme
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public event EventHandler ControlVisibilityChanged;

	[ImportingConstructor]
	public ControlHostServiceAdapter(Sce.Atf.Wpf.Applications.IControlHostService adaptee)
	{
		m_adaptee = adaptee;
	}

	public void RegisterControl(Control control, Sce.Atf.Applications.ControlInfo controlInfo, Sce.Atf.Applications.IControlHostClient client)
	{
		ControlHostClientAdapter orCreateClientAdapter = GetOrCreateClientAdapter(client);
		int num = GenerateId(control, controlInfo, client);
		IControlInfo value = m_adaptee.RegisterControl(control, controlInfo.Name, controlInfo.Description, controlInfo.Group, num.ToString(), orCreateClientAdapter);
		controlInfo.Control = control;
		m_info.Add(controlInfo, value);
		TransferControlInfoValues(controlInfo);
		controlInfo.Changed += delegate(object s, EventArgs e)
		{
			TransferControlInfoValues(s as Sce.Atf.Applications.ControlInfo);
		};
	}

	public void UnregisterControl(Control control)
	{
		m_adaptee.UnregisterContent(control);
		Sce.Atf.Applications.ControlInfo controlInfo = m_info.Keys.FirstOrDefault((Sce.Atf.Applications.ControlInfo x) => x.Control == control);
		if (controlInfo != null)
		{
			controlInfo.Changed -= delegate(object s, EventArgs e)
			{
				TransferControlInfoValues(s as Sce.Atf.Applications.ControlInfo);
			};
			m_info.Remove(controlInfo);
		}
	}

	public void Show(Control control)
	{
		m_adaptee.Show(control);
	}

	private ControlHostClientAdapter GetOrCreateClientAdapter(Sce.Atf.Applications.IControlHostClient client)
	{
		if (!m_clientAdapters.TryGetValue(client, out var value))
		{
			value = new ControlHostClientAdapter(client);
			m_clientAdapters.Add(client, value);
		}
		return value;
	}

	private int GenerateId(Control control, Sce.Atf.Applications.ControlInfo controlInfo, Sce.Atf.Applications.IControlHostClient client)
	{
		int hashCode = client.GetType().Name.GetHashCode();
		int hashCode2 = control.GetType().Name.GetHashCode();
		int hashCode3 = controlInfo.Name.GetHashCode();
		return hashCode ^ hashCode2 ^ hashCode3;
	}

	private void TransferControlInfoValues(Sce.Atf.Applications.ControlInfo controlInfo)
	{
		if (m_info.TryGetValue(controlInfo, out var value))
		{
			value.Name = controlInfo.Name;
			value.Description = controlInfo.Description;
			value.ImageSourceKey = controlInfo.Image;
			if (controlInfo.Image != null)
			{
				Util.GetOrCreateResourceForEmbeddedImage(controlInfo.Image);
			}
		}
	}

	public void Hide(Control control)
	{
		this.ControlVisibilityChanged?.Invoke(this, EventArgs.Empty);
		throw new NotImplementedException();
	}
}
