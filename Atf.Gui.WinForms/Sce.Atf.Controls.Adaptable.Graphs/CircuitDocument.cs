using System;
using System.Collections.Generic;
using System.IO;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class CircuitDocument : DomDocument
{
	private IList<SubCircuit> m_subCircuits;

	private string m_editorFileType;

	private ControlInfo m_controlInfo;

	public ControlInfo ControlInfo
	{
		get
		{
			return m_controlInfo;
		}
		set
		{
			m_controlInfo = value;
		}
	}

	public override string Type => m_editorFileType;

	protected virtual ChildInfo SubCircuitChildInfo => null;

	[Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
	public IList<SubCircuit> SubCircuits => m_subCircuits;

	protected override void OnNodeSet()
	{
		DocumentClientInfo tag = base.DomNode.Type.GetTag<DocumentClientInfo>();
		if (tag != null)
		{
			SetEditorFileType(tag.FileType);
		}
		if (SubCircuitChildInfo != null)
		{
			m_subCircuits = new DomNodeListAdapter<SubCircuit>(base.DomNode, SubCircuitChildInfo);
		}
		base.OnNodeSet();
	}

	public void SetEditorFileType(string fileType)
	{
		m_editorFileType = fileType;
	}

	protected override void OnDirtyChanged(EventArgs e)
	{
		UpdateControlInfo();
		base.OnDirtyChanged(e);
	}

	protected override void OnUriChanged(UriChangedEventArgs e)
	{
		UpdateControlInfo();
		base.OnUriChanged(e);
	}

	private void UpdateControlInfo()
	{
		string localPath = Uri.LocalPath;
		string text = Path.GetFileName(localPath);
		if (Dirty)
		{
			text += "*";
		}
		if (m_controlInfo != null)
		{
			m_controlInfo.Name = text;
			m_controlInfo.Description = localPath;
		}
	}
}
