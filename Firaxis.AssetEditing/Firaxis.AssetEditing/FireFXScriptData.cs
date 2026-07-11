using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.FireFX;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class FireFXScriptData : IFireFXScriptResource, IResource, IFireFXScriptData, IFireFXInstanceData, IFireFXAdapterCompileHandler
{
	private const string GeometryCookParameter = "EmitterGeometries";

	private const string MaterialCookParameter = "EmitterMaterials";

	private const string LightCookParameter = "EmitterLights";

	private IFireFXEffect m_effect;

	private FireFXInstanceAdapter FireFXAdapter { get; set; }

	private IFireFXInstance FireFX { get; set; }

	private IFireFXScriptData ScriptData { get; set; }

	public string Text
	{
		get
		{
			return ScriptData?.Script ?? string.Empty;
		}
		set
		{
			if (ScriptData != null && !(ScriptData.Script == value))
			{
				ScriptData.Script = value;
				this.TextChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public IList<CompileIssue> Issues { get; set; } = new List<CompileIssue>();

	public IFireFXEffect Effect
	{
		get
		{
			return m_effect;
		}
		set
		{
			if (m_effect != value)
			{
				m_effect = value;
				this.EffectChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public string Type => "FireFX Script";

	public Uri Uri
	{
		get
		{
			return FireFXAdapter.FireFX.GetDataFileURIs().FirstOrDefault();
		}
		set
		{
		}
	}

	public string Script
	{
		get
		{
			return ScriptData.Script;
		}
		set
		{
			if (ScriptData.Script != value)
			{
				ScriptData.Script = value;
			}
		}
	}

	public IFireFXEffect CompiledScript => Effect;

	public event EventHandler TextChanged;

	public event EventHandler EffectChanged;

	public event EventHandler<UriChangedEventArgs> UriChanged;

	public FireFXScriptData(FireFXInstanceAdapter fireFx)
	{
		FireFXAdapter = fireFx;
		ScriptData = FireFXAdapter.FireFX.InstanceData as IFireFXScriptData;
		_ = this.UriChanged;
	}

	public void UpdateCookParameters(IClassEntity entityClass)
	{
		UpdateMaterialCookParameters();
		UpdateGeometryCookParameters();
		UpdateLightCookParameters();
	}

	private void UpdateMaterialCookParameters()
	{
		CollectionFieldValueAdapter matAdapter = FireFXAdapter.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "EmitterMaterials" && fld.Is<CollectionFieldValueAdapter>()) as CollectionFieldValueAdapter;
		if (matAdapter == null || CompiledScript == null)
		{
			return;
		}
		foreach (IFireFXEmitter emitter in CompiledScript.Emitters)
		{
			if (!matAdapter.Values.Any((IFieldValueAdapter item) => item.Name == emitter.Name))
			{
				matAdapter.AddNamedValue(emitter.Name, -1).AssignDefaultValue();
			}
		}
		matAdapter.Values.Where((IFieldValueAdapter ma) => !CompiledScript.Emitters.Select((IFireFXEmitter es) => es.Name).Contains(ma.Name)).ToArray().ForEach(delegate(IFieldValueAdapter tr)
		{
			matAdapter.Values.Remove(tr);
		});
	}

	private void UpdateGeometryCookParameters()
	{
		CollectionFieldValueAdapter geoAdapter = FireFXAdapter.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "EmitterGeometries" && fld.Is<CollectionFieldValueAdapter>()) as CollectionFieldValueAdapter;
		if (geoAdapter == null || CompiledScript == null)
		{
			return;
		}
		foreach (IFireFXEmitter emitter in CompiledScript.Emitters)
		{
			if (!geoAdapter.Values.Any((IFieldValueAdapter item) => item.Name == emitter.Name))
			{
				geoAdapter.AddNamedValue(emitter.Name, -1).AssignDefaultValue();
			}
		}
		geoAdapter.Values.Where((IFieldValueAdapter ma) => !CompiledScript.Emitters.Select((IFireFXEmitter es) => es.Name).Contains(ma.Name)).ToArray().ForEach(delegate(IFieldValueAdapter tr)
		{
			geoAdapter.Values.Remove(tr);
		});
	}

	private void UpdateLightCookParameters()
	{
		CollectionFieldValueAdapter lightAdapter = FireFXAdapter.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "EmitterLights" && fld.Is<CollectionFieldValueAdapter>()) as CollectionFieldValueAdapter;
		if (lightAdapter == null || CompiledScript == null)
		{
			return;
		}
		foreach (IFireFXEmitter emitter in CompiledScript.Emitters)
		{
			if (!lightAdapter.Values.Any((IFieldValueAdapter item) => item.Name == emitter.Name))
			{
				lightAdapter.AddNamedValue(emitter.Name, -1).AssignDefaultValue();
			}
		}
		lightAdapter.Values.Where((IFieldValueAdapter ma) => !CompiledScript.Emitters.Select((IFireFXEmitter es) => es.Name).Contains(ma.Name)).ToArray().ForEach(delegate(IFieldValueAdapter tr)
		{
			lightAdapter.Values.Remove(tr);
		});
	}
}
