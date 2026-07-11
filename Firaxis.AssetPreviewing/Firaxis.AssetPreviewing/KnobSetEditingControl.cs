using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetPreviewing;

public class KnobSetEditingControl : DockHostControl
{
	private IKnobSet _activeKnobSet;

	private readonly Dictionary<string, PropertyTreeControl> m_subgroupTabs = new Dictionary<string, PropertyTreeControl>();

	public IKnobSet ActiveKnobSet
	{
		get
		{
			return _activeKnobSet;
		}
		set
		{
			if (value != _activeKnobSet)
			{
				string oldKnobSetName = _activeKnobSet?.KnobSetName ?? "null";
				string newKnobSetName = value?.KnobSetName ?? "null";
				_activeKnobSet = value;
				if (TryUpdateExistingSubgroubTabs(oldKnobSetName, newKnobSetName))
				{
					return;
				}
				BeginContentUpdate();
				try
				{
					PaintTimingLog.Write("KnobSetEditingControl: begin rebuild old={0}, new={1}", oldKnobSetName, newKnobSetName);
					var removeStart = System.Diagnostics.Stopwatch.StartNew();
					RemoveAllSubgroubTabs();
					removeStart.Stop();
					var addStart = System.Diagnostics.Stopwatch.StartNew();
					AddSubgroubTabs();
					addStart.Stop();
					PaintTimingLog.Write("KnobSetEditingControl: end rebuild remove={0}ms, add={1}ms", removeStart.ElapsedMilliseconds, addStart.ElapsedMilliseconds);
				}
				finally
				{
					EndContentUpdate();
				}
			}
		}
	}

	public KnobSetEditingControl(IThemeService themeSvc)
		: base(themeSvc)
	{
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	private bool TryUpdateExistingSubgroubTabs(string oldKnobSetName, string newKnobSetName)
	{
		IEnumerable<IKnobSubgroup> subgroups = ActiveKnobSet?.KnobsBySubgroup?.Values ?? Enumerable.Empty<IKnobSubgroup>();
		Dictionary<string, IKnobSubgroup> subgroupLookup = subgroups.ToDictionary(GetDockName);
		if (subgroupLookup.Count == 0 || subgroupLookup.Count != m_subgroupTabs.Count || subgroupLookup.Keys.Any(key => !m_subgroupTabs.ContainsKey(key)))
		{
			return false;
		}
		var sw = System.Diagnostics.Stopwatch.StartNew();
		foreach (KeyValuePair<string, IKnobSubgroup> item in subgroupLookup)
		{
			m_subgroupTabs[item.Key].SetTypeDescriptor(new KnobSetSubgroupPropertyProxy(item.Value));
		}
		sw.Stop();
		PaintTimingLog.Write("KnobSetEditingControl: update existing old={0}, new={1}, tabs={2}, update={3}ms", oldKnobSetName, newKnobSetName, subgroupLookup.Count, sw.ElapsedMilliseconds);
		return true;
	}

	private void AddSubgroubTabs()
	{
		IEnumerable<IKnobSubgroup> enumerable = ActiveKnobSet?.KnobsBySubgroup?.Values;
		foreach (IKnobSubgroup item in enumerable ?? Enumerable.Empty<IKnobSubgroup>())
		{
			string dockName = GetDockName(item);
			PropertyTreeControl propertyTreeControl = new PropertyTreeControl(ThemeService, new KnobSetSubgroupPropertyProxy(item));
			propertyTreeControl.Dock = DockStyle.Fill;
			SkinService.ApplyActiveSkin(propertyTreeControl);
			m_subgroupTabs[dockName] = propertyTreeControl;
			AddDockContext(propertyTreeControl, dockName, dockName, "", DockState.Document, canClose: false);
		}
	}

	private static string GetDockName(IKnobSubgroup subgroup)
	{
		return string.IsNullOrEmpty(subgroup.SubgroupName) ? "Base" : subgroup.SubgroupName;
	}

	private void RemoveAllSubgroubTabs()
	{
		m_subgroupTabs.Clear();
		RemoveAllContent();
	}
}
