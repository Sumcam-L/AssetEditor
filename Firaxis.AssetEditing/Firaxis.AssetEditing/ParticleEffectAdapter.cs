using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.FireFX;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ParticleEffectAdapter : ImportedEntityAdapter
{
	public const string FireFXReferenceParameter = "FireFX Reference";

	public const string EmitterMaterialsParameter = "EmitterMaterials";

	public const string EmitterGeometriesParameter = "EmitterGeometries";

	public const string FireFXParticleClassName = "FireFXParticle";

	public IEnumerable<string> RootEmitters
	{
		get
		{
			IList<string> list = new List<string>();
			if (ClassName != "FireFXParticle")
			{
				return list;
			}
			if (!(base.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "FireFX Reference" && fld.Is<ObjectFieldValueAdapter>()) is ObjectFieldValueAdapter objectFieldValueAdapter))
			{
				return list;
			}
			if (objectFieldValueAdapter.ObjectParameter.ObjectType != InstanceType.IT_FIREFX)
			{
				return list;
			}
			using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.CivTechService.ProjectMapService.GetActivePantryPaths() });
			IEnumerable<IFireFXEmitter> enumerable = (instanceSet.LoadEntity<IFireFXInstance>(base.CivTechService.ProjectMapService.LayeredPantry, objectFieldValueAdapter.ObjectName)?.InstanceData.As<IFireFXScriptData>())?.CompiledScript?.Emitters;
			foreach (IFireFXEmitter item in enumerable ?? Enumerable.Empty<IFireFXEmitter>())
			{
				if ((item.Flags & EmitterFlags.BaseEmitter) == EmitterFlags.BaseEmitter)
				{
					list.Add(item.FullName);
				}
			}
			return list;
		}
	}

	public override IImportedEntity ImportedEntity => ParticleEffect;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.ParticleEffectEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.ParticleEffectEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.ParticleEffectEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.ParticleEffectEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.ParticleEffectEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.ParticleEffectEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.ParticleEffectEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.ParticleEffectEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.ParticleEffectEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.ParticleEffectEntityType.TagsChild;

	public IParticleEffectInstance ParticleEffect => InstanceEntity as IParticleEffectInstance;

	private void UpdateCookParameters(IFireFXEffect effect)
	{
		UpdateMaterialCookParameters(effect);
		UpdateGeometryCookParameters(effect);
	}

	private void UpdateMaterialCookParameters(IFireFXEffect effect)
	{
		CollectionFieldValueAdapter matAdapter = base.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "EmitterMaterials" && fld.Is<CollectionFieldValueAdapter>()) as CollectionFieldValueAdapter;
		if (matAdapter == null || effect == null)
		{
			return;
		}
		foreach (IFireFXEmitter emitter in effect.Emitters)
		{
			if (!matAdapter.Values.Any((IFieldValueAdapter item) => item.Name == emitter.Name))
			{
				matAdapter.AddNamedValue(emitter.Name, -1).AssignDefaultValue();
			}
		}
		matAdapter.Values.Where((IFieldValueAdapter ma) => !effect.Emitters.Select((IFireFXEmitter es) => es.Name).Contains(ma.Name)).ToArray().ForEach(delegate(IFieldValueAdapter tr)
		{
			matAdapter.Values.Remove(tr);
		});
	}

	private void UpdateGeometryCookParameters(IFireFXEffect effect)
	{
		CollectionFieldValueAdapter geoAdapter = base.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "EmitterGeometries" && fld.Is<CollectionFieldValueAdapter>()) as CollectionFieldValueAdapter;
		if (geoAdapter == null || effect == null)
		{
			return;
		}
		foreach (IFireFXEmitter emitter in effect.Emitters)
		{
			if (!geoAdapter.Values.Any((IFieldValueAdapter item) => item.Name == emitter.Name))
			{
				geoAdapter.AddNamedValue(emitter.Name, -1).AssignDefaultValue();
			}
		}
		geoAdapter.Values.Where((IFieldValueAdapter ma) => !effect.Emitters.Select((IFireFXEmitter es) => es.Name).Contains(ma.Name)).ToArray().ForEach(delegate(IFieldValueAdapter tr)
		{
			geoAdapter.Values.Remove(tr);
		});
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (base.CookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == "FireFX Reference" && fld.Is<ObjectFieldValueAdapter>()) is ObjectFieldValueAdapter objectFieldValueAdapter)
		{
			using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.CivTechService.GetActivePantryPaths() });
			IFireFXEffect effect = instanceSet.LoadByName<IFireFXInstance>(objectFieldValueAdapter.ObjectName)?.InstanceData.As<IFireFXScriptData>()?.CompiledScript;
			UpdateCookParameters(effect);
			return;
		}
		UpdateCookParameters(null);
	}
}
