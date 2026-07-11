using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefDocument : CompositeDomDocument, IVersionProvider, ICookable, IHotloadable, IArtDefProvider, IProjectSpecificDocument
{
	private IInstanceSet m_instanceSet;

	private ISet<HistoryContext> m_dirtyContexts = new HashSet<HistoryContext>();

	private Image m_dirtyImage;

	private IArtDef ArtDef => base.DomNode.As<ArtDefSetAdapter>().ArtDef;

	public ICivTechService CivTechService { get; set; }

	public ICookService CookService { get; set; }

	public IEnumerable<IDocumentClient> DocumentClients { get; set; }

	public IDocumentService FileCommands { get; set; }

	public IFileDialogService FileDialogService { get; set; }

	public IInstanceSet InstanceSet
	{
		get
		{
			if (m_instanceSet == null)
			{
				m_instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
			}
			return m_instanceSet;
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			if (ArtDef == null)
			{
				return false;
			}
			if (!CivTechService.IsFromActiveProjectOrDependencies(Uri))
			{
				return true;
			}
			if (!CivTechService.IsFromModDependencies(Uri))
			{
				if (!VersionService.IsLocalBuild())
				{
					return Version > VersionService.ApplicationVersion;
				}
				return false;
			}
			return true;
		}
	}

	public override string Type => ArtDefEditor.DocumentClientInfo.FileType;

	public IVersionService VersionService { get; set; }

	public Version Version => ArtDef.Version;

	public Uri CookableUri => Uri;

	public string SubSystem => TunerHelper.GetArtDefSubSystem();

	public IEnumerable<string> ConsumerNames
	{
		get
		{
			string text = Uri.LocalPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string oldValue = CivTechService.PrimaryProject.Paths.ArtDefRoot.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar;
			return TunerHelper.GetArtDefConsumerNames(artDefOutputRoot: CivTechService.PrimaryProject.Paths.ArtDefOutputRoot, artDefRelativePath: text.Replace(oldValue, string.Empty));
		}
	}

	IArtDef IArtDefProvider.ArtDef => ArtDef;

	public bool MigrateTemplate()
	{
		ArtDefContext artDefContext = base.DomNode.As<ArtDefContext>();
		ArtDefSetAdapter artDefSetAdapter = base.DomNode.As<ArtDefSetAdapter>();
		if (TemplateRootHasChanged(artDefSetAdapter.ArtDef, artDefSetAdapter.ArtDefTemplate))
		{
			switch (MessageBox.Show("One or more elements of the art def template have changed.\nWould you like to update the art def file \"" + Uri.AbsolutePath + "\" to match these changes?\n\n(Note: if you are out of date this could cause loss of data!)", "Template Changed", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
			{
			case DialogResult.Cancel:
				return false;
			case DialogResult.Yes:
				artDefContext.Begin("Update From Template".Localize());
				artDefContext.AddOperation(new NullOperation(delegate
				{
					artDefSetAdapter.DomApplyTemplateChanges(artDefSetAdapter.ArtDefTemplate);
				}));
				artDefContext.End();
				break;
			}
		}
		return true;
	}

	public void RegisterForDirtyNotifications(HistoryContext context)
	{
		context.DirtyChanged += Context_DirtyChanged;
	}

	public bool TemplateCollectionHasChanged(IArtDefTemplate artDefTmpl, IArtDefElementTemplate artDefElemTmpl, IArtDefCollection artDefCol)
	{
		if (artDefElemTmpl.ReplaceMergedCollectionElements != artDefCol.ReplaceMergedCollectionElements)
		{
			return true;
		}
		foreach (IArtDefElement element in artDefCol.Elements)
		{
			if (TemplateElementHasChanged(artDefTmpl, artDefElemTmpl, element))
			{
				return true;
			}
		}
		return false;
	}

	public bool TemplateElementHasChanged(IArtDefTemplate artDefTmpl, IArtDefElementTemplate artDefElemTmpl, IArtDefElement artDefElem)
	{
		if (artDefElemTmpl.AppendMergedParameterCollections != artDefElem.AppendMergedParameterCollections)
		{
			return true;
		}
		foreach (IValue item in artDefElem.Fields.Items)
		{
			IParameter parameter = artDefElemTmpl.Parameters.FindByName(item.ParameterName);
			if (parameter == null)
			{
				return true;
			}
			if (parameter.ParameterValueType != item.ParameterType)
			{
				return true;
			}
		}
		if (artDefElemTmpl.Parameters.Items.Count() != artDefElem.Fields.Items.Count())
		{
			return true;
		}
		foreach (IArtDefCollection col in artDefElem.Children)
		{
			bool foundCollection = false;
			bool hasChanged = false;
			artDefElemTmpl.Children.ForEach(delegate(IArtDefElementTemplate adet)
			{
				if (adet.Name == col.CollectionName)
				{
					foundCollection = true;
					if (TemplateCollectionHasChanged(artDefTmpl, adet, col))
					{
						hasChanged = true;
					}
				}
			});
			if (!foundCollection || hasChanged)
			{
				return true;
			}
		}
		if (artDefElemTmpl.Children.Count() != artDefElem.Children.Count())
		{
			return true;
		}
		return false;
	}

	public bool TemplateRootHasChanged(IArtDef artDef, IArtDefTemplate artDefTmpl)
	{
		foreach (IArtDefCollection artDefCol in artDef.RootCollections)
		{
			IArtDefElementTemplate artDefElementTemplate = artDefTmpl.Collections.FirstOrDefault((IArtDefElementTemplate adet) => adet.Name == artDefCol.CollectionName);
			if (artDefElementTemplate == null)
			{
				return true;
			}
			if (TemplateCollectionHasChanged(artDefTmpl, artDefElementTemplate, artDefCol))
			{
				return true;
			}
		}
		return artDefTmpl.Collections.Count() != artDef.RootCollections.Count();
	}

	public override void UpdateControlInfo()
	{
		string localPath = Uri.LocalPath;
		string text = Path.GetFileName(localPath);
		if (Dirty)
		{
			text += "*";
		}
		if (IsReadOnly)
		{
			text += "(Read Only)";
		}
		ArtDefContext artDefContext = this.As<ArtDefContext>();
		artDefContext.ControlInfo.Name = text;
		artDefContext.ControlInfo.Description = localPath;
	}

	public bool ValidateTemplate(string tmplName)
	{
		if (ArtDefContext.FindArtDefTemplate(CivTechService.PrimaryProject.Config, tmplName) == null)
		{
			MessageBox.Show("Art Def Template \"" + tmplName + "\" is no longer available in project config \"" + CivTechService.PrimaryProject.ActiveConfigPath + "\".\n\nFile \"" + Uri.AbsolutePath + "\" can not be edited!", "Invalid Template", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		foreach (HistoryContext dirtyContext in m_dirtyContexts)
		{
			dirtyContext.DirtyChanged -= Context_DirtyChanged;
		}
		m_dirtyContexts.Clear();
		ArtDefContext artDefContext = base.DomNode.As<ArtDefContext>();
		artDefContext.DirtyChanged -= Context_DirtyChanged;
		artDefContext.Dispose();
		base.DomNode.As<ArtDefSetAdapter>().Dispose();
		if (m_instanceSet != null)
		{
			m_instanceSet.Dispose();
			m_instanceSet = null;
		}
		if (m_dirtyImage != null)
		{
			m_dirtyImage.Dispose();
			m_dirtyImage = null;
		}
	}

	protected override void OnDirtyChanged(EventArgs e)
	{
		UpdateControlInfo();
		if (!Dirty)
		{
			HistoryContext[] array = m_dirtyContexts.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dirty = false;
			}
			PlatformAssert.If(m_dirtyContexts.Count != 0);
		}
		base.OnDirtyChanged(e);
	}

	protected override void OnUriChanged(UriChangedEventArgs e)
	{
		UpdateControlInfo();
		base.OnUriChanged(e);
	}

	private void Context_DirtyChanged(object sender, EventArgs e)
	{
		HistoryContext historyContext = sender as HistoryContext;
		if (historyContext.Dirty)
		{
			Dirty = true;
			m_dirtyContexts.Add(historyContext);
			return;
		}
		m_dirtyContexts.Remove(historyContext);
		if (m_dirtyContexts.Count == 0)
		{
			Dirty = false;
		}
	}

	private Image GetDirtyImage()
	{
		if (m_dirtyImage == null)
		{
			m_dirtyImage = ImageHelper.GetSolidTriangleImage(Color.Red, 16);
		}
		return m_dirtyImage;
	}
}
