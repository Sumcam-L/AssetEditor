using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.TextureExport;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public class EnvironmentLightContext : BaseEntityPropertyContext, IEnvironmentLightEditingContext, IPropertyEditingContext
{
	private bool m_applyChangesAutomatically;

	private bool m_sampleIntensityFromMap;

	private uint m_nLastWidth = 128u;

	private uint m_nLastSampleCount = 128u;

	private bool m_hasSource;

	private IEnvironmentMapImporter m_envImporter;

	private bool m_bImageDirty = true;

	private Image m_cachedImage;

	private IEnvironmentSource m_source;

	private ICubeMap m_cube;

	public bool ApplyChangedAutomatically
	{
		get
		{
			return m_applyChangesAutomatically;
		}
		set
		{
			if (m_applyChangesAutomatically != value)
			{
				m_applyChangesAutomatically = value;
				base.GUI.As<EnvironmentLightEditorControl>()?.RefreshUIState();
			}
		}
	}

	public bool SampleIntensityFromMap
	{
		get
		{
			return m_sampleIntensityFromMap;
		}
		set
		{
			if (m_sampleIntensityFromMap != value)
			{
				m_sampleIntensityFromMap = value;
				base.GUI.As<EnvironmentLightEditorControl>()?.RefreshUIState();
			}
		}
	}

	public bool HasSource => m_hasSource;

	public uint LastSampleCount => m_nLastSampleCount;

	public IEnvironmentMapImporter Importer => m_envImporter;

	public Image CubeImage
	{
		get
		{
			if (Cube != null && m_bImageDirty)
			{
				m_cachedImage = Cube.CreateThumbnail();
				m_bImageDirty = false;
			}
			return m_cachedImage;
		}
	}

	public IEnvironmentSource Source => m_source;

	public ICubeMap Cube
	{
		get
		{
			return m_cube;
		}
		set
		{
			if (m_cube != value)
			{
				m_cube = value;
				m_bImageDirty = true;
				base.GUI.As<EnvironmentLightEditorControl>()?.Bind(this);
			}
		}
	}

	public IInstanceSet Instances => base.DomNode.As<IInstanceEntityDocument>().InstanceSet;

	public IEnvironmentLightInstance EnvironmentLight => base.DomNode.As<IInstanceEntityDocument>().InstanceEntity as IEnvironmentLightInstance;

	public IEnvironmentLightClass LightClass => base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(EnvironmentLight) as IEnvironmentLightClass;

	private IAssetPreviewer PreviewerService => base.DomNode.As<InstanceEntityAdapter>().PreviewerService;

	public IList<LightDirectionTagAdapter> LightDirectionTags => base.DomNode.As<EnvironmentLightAdapter>().LightDirectionTagSet.LightDirectionTags;

	public EnvironmentLightContext()
	{
		m_envImporter = Context.EnsureCreated<CivTechContext>().CreateInstance<IEnvironmentMapImporter>();
	}

	public void SetCubePath(string path)
	{
		m_cube = Importer.OpenCubeMap(path);
	}

	public LightDirectionTagAdapter AddDirectionTag(string name, float u, float v)
	{
		LightDirectionTagAdapter lightDirectionTagAdapter = null;
		if (Cube != null)
		{
			float[] array = Importer.ThumbnailUVToDirection(u, v);
			IFloatVector3 lightIntensity = Cube.GetLightIntensity(array[0], array[1], array[2]);
			lightDirectionTagAdapter = EnvironmentLightAdapter.CreateLightDirectionTag(array[0], array[1], array[2]);
			lightDirectionTagAdapter.U = u;
			lightDirectionTagAdapter.V = v;
			lightDirectionTagAdapter.Red = lightIntensity.X;
			lightDirectionTagAdapter.Green = lightIntensity.Y;
			lightDirectionTagAdapter.Blue = lightIntensity.Z;
			lightDirectionTagAdapter.CastsShadows = true;
			lightDirectionTagAdapter.Diameter = 0f;
			lightDirectionTagAdapter.AngularFalloff = 0f;
			lightDirectionTagAdapter.Name = name;
			lightDirectionTagAdapter.SetColor(lightIntensity.X, lightIntensity.Y, lightIntensity.Z);
			LightDirectionTags.Add(lightDirectionTagAdapter);
		}
		else
		{
			BugSubmitter.SilentReport("Invalid environment light cube @assign bwhitman");
		}
		return lightDirectionTagAdapter;
	}

	public void SetSourceFile(string path)
	{
		base.DomNode.As<EnvironmentLightAdapter>().SourceFilePath = path;
	}

	public void OpenSourceFile(string path)
	{
		IEnvironmentSource source = m_source;
		ICubeMap cube = m_cube;
		try
		{
			m_source = Importer.OpenSourceFile(path);
			EnvironmentMapParameterization eSourceParametrization = (m_source.IsCubeMap ? EnvironmentMapParameterization.ENVMAP_CUBE : EnvironmentMapParameterization.ENVMAP_LATLONG);
			CreateCube(eSourceParametrization, m_nLastSampleCount, m_nLastWidth);
			m_hasSource = true;
		}
		catch (System.Exception ex)
		{
			m_source = source;
			m_cube = cube;
			string text = "An error occurred while trying to load the source file: " + ex.Message;
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void CreateCube(EnvironmentMapParameterization eSourceParametrization, uint sampleCount, uint width)
	{
		PlatformAssert.If(Source == null, "Tried to recreate the cube without having a source loaded.");
		m_nLastSampleCount = sampleCount;
		m_nLastWidth = width;
		ICubeMap origCube = m_cube;
		this.DoTransaction(delegate
		{
			Action doAction = delegate
			{
				try
				{
					if (LightClass != null)
					{
						m_cube = Importer.CreateCube(Source, eSourceParametrization, sampleCount, LightClass.ImportOptions, useIdentityBasis: false, width);
						m_bImageDirty = true;
					}
					else
					{
						string text = "Please select a Class before the environment can be displayed properly.";
						MessageBox.Show(text);
						Outputs.WriteLine(OutputMessageType.Error, text);
						m_cube = origCube;
					}
				}
				catch (System.Exception ex)
				{
					string text2 = "An exception happened when trying to create the cube map. Cube map is reverting to its previous state.\nError: " + ex.Message;
					MessageBox.Show(text2);
					Outputs.WriteLine(OutputMessageType.Error, text2);
					m_cube = origCube;
				}
			};
			AddOperation(new EnvironmentLightChangingOperation(this, doAction));
		}, "Updating cube map.");
	}

	public void ApplyChanges()
	{
		IDocument document = base.DomNode.As<IDocument>();
		if (!document.Dirty)
		{
			document.Dirty = true;
		}
		EnvironmentLight.PopulateDataFiles(LightClass);
		foreach (IClassDataFile dataFile in LightClass.DataFiles)
		{
			IInstanceDataFile instanceDataFile = EnvironmentLight.FindDataFileByID(dataFile.ID);
			string path = StaticMethods.PantryRootForInstanceType(base.CivTechService.PrimaryProject.Paths.GamePantry, EnvironmentLight.Type);
			path = Path.Combine(path, instanceDataFile.RelativePath);
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists && fileInfo.IsReadOnly)
			{
				string text = "The data file located at path (" + path + ") is read-only and cannot be saved over.  Please check the file out of Perforce.";
				Outputs.WriteLine(OutputMessageType.Error, text);
				MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				continue;
			}
			try
			{
				Cube.SaveDDS(path, LightClass.ImportOptions.PixelFormat);
			}
			catch (System.Exception ex)
			{
				string text2 = "Exception thrown when trying to save the Cube Map of Environment Light (" + EnvironmentLight.Name + ") to (" + path + ").  Exception message:\n(" + ex.Message + ")";
				Outputs.WriteLine(OutputMessageType.Error, text2);
				MessageBox.Show(text2, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		base.BatchChangelist?.CreateEntityChangedEvent(EnvironmentLight.Type, EnvironmentLight.Name);
	}

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			base.GUI?.Bind(null);
			base.GUI?.Dispose();
			base.GUI = null;
		}
		base.Dispose(bDisposing);
	}
}
