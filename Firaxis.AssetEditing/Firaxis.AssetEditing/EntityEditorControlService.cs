using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

[Export(typeof(IEntityEditorControlService))]
[Export(typeof(EntityEditorControlService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntityEditorControlService : IEntityEditorControlService
{
	private IDictionary<InstanceType, Func<IThemeService, IEntityDocument, EntityEditorControlBase>> m_typeControlFactory = new Dictionary<InstanceType, Func<IThemeService, IEntityDocument, EntityEditorControlBase>>();

	[Import(AllowDefault = true)]
	public IThemeService ThemeService { get; set; }

	[Import(AllowDefault = true)]
	public AssetEditor AssetEditor { get; set; }

	[Import(AllowDefault = true)]
	public FireFXEditor FireFXEditor { get; set; }

	[ImportingConstructor]
	public EntityEditorControlService()
	{
		m_typeControlFactory[InstanceType.IT_ASSET] = (IThemeService themeSvc, IEntityDocument doc) => CreateAssetEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_BEHAVIOR] = (IThemeService themeSvc, IEntityDocument doc) => new BehaviorEditorControl(themeSvc)
		{
			Tag = doc
		};
		m_typeControlFactory[InstanceType.IT_MATERIAL] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_GEOMETRY] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_TEXTURE] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_ANIMATION] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_DSG] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_ANALYTIC_LIGHT] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_ENVIRONMENT_LIGHT] = (IThemeService themeSvc, IEntityDocument doc) => new EnvironmentLightEditorControl
		{
			Tag = doc
		};
		m_typeControlFactory[InstanceType.IT_LIGHT_RIG] = (IThemeService themeSvc, IEntityDocument doc) => new LightRigEditorControl
		{
			Tag = doc
		};
		m_typeControlFactory[InstanceType.IT_PARTICLE_EFFECT] = (IThemeService themeSvc, IEntityDocument doc) => CreateEntityEditorControl(themeSvc, doc);
		m_typeControlFactory[InstanceType.IT_FIREFX] = (IThemeService themeSvc, IEntityDocument doc) => CreateFireFXEditorControl(themeSvc, doc);
	}

	private AssetEditorControl CreateAssetEditorControl(IThemeService themeSvc, IEntityDocument doc)
	{
		return new AssetEditorControl(AssetEditor?.EditorLayoutState ?? string.Empty, themeSvc, (doc as AssetDocument)?.EntityAdapter?.ClassName)
		{
			Tag = doc
		};
	}

	private FireFXEditorControl CreateFireFXEditorControl(IThemeService themeSvc, IEntityDocument doc)
	{
		return new FireFXEditorControl(FireFXEditor?.FireFXEditorlayoutState ?? string.Empty, themeSvc)
		{
			Tag = doc
		};
	}

	private EntityEditorControl CreateEntityEditorControl(IThemeService themeSvc, IEntityDocument doc)
	{
		return new EntityEditorControl(themeSvc)
		{
			Tag = doc
		};
	}

	public EntityEditorControlBase CreateControl(IEntityDocument entDoc)
	{
		return m_typeControlFactory[entDoc.InstanceEntity.Type](ThemeService, entDoc);
	}
}
