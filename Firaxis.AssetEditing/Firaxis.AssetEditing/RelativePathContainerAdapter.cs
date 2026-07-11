using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class RelativePathContainerAdapter : DomNodeAdapter
{
	private IList<RelativePathAdapter> m_relativePaths;

	private IRelativePathHost _pathHost;

	public IList<RelativePathAdapter> RelativePaths => m_relativePaths;

	public IRelativePathHost PathHost
	{
		get
		{
			return _pathHost;
		}
		set
		{
			if (_pathHost == value)
			{
				return;
			}
			UnregisterFromDomChanges();
			_pathHost = value;
			m_relativePaths.Clear();
			foreach (string relativePath in value.RelativePaths)
			{
				RelativePathAdapter item = RelativePathAdapter.Create(relativePath);
				RelativePaths.Add(item);
			}
			RegisterForDomChanges();
		}
	}

	protected override void OnNodeSet()
	{
		m_relativePaths = new DomNodeListAdapter<RelativePathAdapter>(base.DomNode, GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	public void AddRelativePath(IEnumerable<string> paths)
	{
		foreach (string path in paths)
		{
			RelativePathAdapter item = RelativePathAdapter.Create(path);
			RelativePaths.Add(item);
		}
	}

	public void RemoveRelativePaths(IEnumerable<string> paths)
	{
		foreach (string path in paths)
		{
			RelativePathAdapter relativePathAdapter = RelativePaths.FirstOrDefault((RelativePathAdapter adp) => adp.RelativePath == path);
			if (relativePathAdapter != null)
			{
				RelativePaths.Remove(relativePathAdapter);
			}
		}
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleChildAttributeChanged;
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleChildAttributeChanged;
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild)
		{
			RelativePathAdapter relativePathAdapter = e.Child.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				PathHost.AddRelativePath(relativePathAdapter.RelativePath);
			}
		}
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild)
		{
			RelativePathAdapter relativePathAdapter = e.Child.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				PathHost.RemoveRelativePath(relativePathAdapter.RelativePath);
			}
		}
	}

	protected virtual void HandleChildAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.RelativePathType.RelativePathAttribute)
		{
			string path = e.OldValue.ToString();
			string path2 = e.NewValue.ToString();
			PathHost.RemoveRelativePath(path);
			PathHost.AddRelativePath(path2);
		}
	}
}
