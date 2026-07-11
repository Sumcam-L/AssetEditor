using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LightRigAdapter : InstanceEntityAdapter, IAnimatableEntityAdapter, IInstanceEntityAdapter, INamedAdapter, IAssetBrowserTypeProvider
{
	private Dictionary<string, IList<StateTransitionInfo>> m_TimelineToStateTransitions;

	public virtual IDictionary<string, IList<StateTransitionInfo>> TimelineStateTransitions => m_TimelineToStateTransitions;

	public LightSetAdapter AnalyticLightSet { get; private set; }

	public LightSetAdapter EnvironmentLightSet { get; private set; }

	public IInstanceSet InstanceSet => base.DomNode.As<LightRigDocument>().InstanceSet;

	public ILightRigInstance LightRig => InstanceEntity as ILightRigInstance;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.LightRigEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.LightRigEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.LightRigEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.LightRigEntityType.DescriptionAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.LightRigEntityType.NameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.LightRigEntityType.TagsChild;

	public AnimationBindingSetAdapter AnimationBindingSet { get; private set; }

	public IAnimatable AnimationData => LightRig;

	public string DSG
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LightRigEntityType.DSGAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightRigEntityType.DSGAttribute, value);
		}
	}

	public IDSGInstance DSGInst { get; private set; }

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			if (base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(LightRig) is ILightRigClass lightRigClass)
			{
				return lightRigClass.AllowedDSGClasses;
			}
			return null;
		}
	}

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_DSG };

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);

	private void InitializeTimelineToStateTransitions()
	{
		m_TimelineToStateTransitions = new Dictionary<string, IList<StateTransitionInfo>>();
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (LightRig.DSGName != DSG)
		{
			DSG = LightRig.DSGName;
		}
		DSGInst = InstanceSet.LoadEntityByName(DSG, InstanceType.IT_DSG) as IDSGInstance;
		AnimationBindingSet.Update();
		AnalyticLightSet.Update(LightRig.LightReferences.AnalyticLightReferences);
		EnvironmentLightSet.Update(LightRig.LightReferences.EnvironmentLightReferences);
	}

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.LightRigEntityType.DSGAttribute)
		{
			LightRig.DSGName = DSG;
			DSGInst = InstanceSet.LoadEntityByName(DSG, InstanceType.IT_DSG) as IDSGInstance;
			base.DomNode.As<BaseEntityPropertyContext>().BatchChangelist?.CreateAssetDSGChangedEvent(LightRig, DSG);
		}
		else
		{
			base.HandleDomNodeAttributeChanged(sender, e);
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		AnimationBindingSet = this.CreateComponentAdapter<AnimationBindingSetAdapter>(EntitySchema.AnimationBindingSetType.Type, EntitySchema.LightRigEntityType.AnimationBindingSetChild);
		EnvironmentLightSet = this.CreateComponentAdapter<LightSetAdapter>(EntitySchema.LightSetType.Type, EntitySchema.LightRigEntityType.EnvironmentLightSetChild);
		EnvironmentLightSet.LightType = InstanceType.IT_ENVIRONMENT_LIGHT;
		AnalyticLightSet = this.CreateComponentAdapter<LightSetAdapter>(EntitySchema.LightSetType.Type, EntitySchema.LightRigEntityType.AnalyticLightSetChild);
		AnalyticLightSet.LightType = InstanceType.IT_ANALYTIC_LIGHT;
	}
}
