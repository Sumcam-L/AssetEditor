using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IPaletteService))]
[Export(typeof(IInitializable))]
[Export(typeof(PaletteService))]
[PartCreationPolicy(CreationPolicy.Any)]
public class PaletteService : IPaletteService, Sce.Atf.Applications.IPaletteService, IControlHostClient, IInitializable
{
	[Import]
	private IControlHostService m_controlHostService = null;

	private IControlInfo m_controlInfo;

	private Dictionary<object, IPaletteClient> m_objectClients = new Dictionary<object, IPaletteClient>();

	private static Guid s_paletteControl = new Guid("266a2ae4-a801-482d-9dd7-a8ca33b6beea");

	public void Initialize()
	{
		m_controlInfo = m_controlHostService.RegisterControl(new PaletteContent(this), "Palette".Localize(), "Creates new instances".Localize(), StandardControlGroup.Left, s_paletteControl.ToString(), this);
	}

	public void Activate(object control)
	{
	}

	public void Deactivate(object control)
	{
	}

	public bool Close(object control, bool mainWindowClosing)
	{
		return true;
	}

	public void AddItem(object item, string categoryName, IPaletteClient client)
	{
		if (m_objectClients.ContainsKey(item))
		{
			throw new InvalidOperationException("duplicate item");
		}
		m_objectClients.Add(item, client);
		if (m_controlInfo.Content is PaletteContent paletteContent)
		{
			paletteContent.AddItem(item, categoryName ?? string.Empty);
		}
	}

	public void RemoveItem(object item)
	{
		if (m_controlInfo.Content is PaletteContent paletteContent)
		{
			paletteContent.RemoveItem(item);
		}
		m_objectClients.Remove(item);
	}

	public IEnumerable<object> Convert(IEnumerable<object> items)
	{
		List<object> list = new List<object>();
		foreach (object item in items)
		{
			if (m_objectClients.TryGetValue(item, out var value))
			{
				object obj = value.Convert(item);
				if (obj != null)
				{
					list.Add(obj);
				}
			}
		}
		return list;
	}
}
