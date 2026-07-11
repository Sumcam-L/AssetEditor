using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.AssetEditing;

public class MaterialDocument : BaseInstanceEntityDocument
{
	public override bool MeetsSavePreconditions(ICollection<string> errorMessages)
	{
		bool flag;
		if ((flag = base.MeetsSavePreconditions(errorMessages)) && base.ProjectConfig.Classes.FindForInstance(base.InstanceEntity) is IMaterialClass materialClass && materialClass.ValidationOptions.RequireUniformTextureSize)
		{
			flag = AreAllTexturesTheSameSize();
			if (!flag)
			{
				errorMessages.Add("Material Class " + materialClass.Name + " requires each texture to be the same size.");
			}
		}
		return flag;
	}

	private bool AreAllTexturesTheSameSize()
	{
		bool result = true;
		IEnumerable<EntityID> entities = from val in base.InstanceEntity.CookParameters.Items.OfType<IObjectValue>().GetBoundObjectValues(InstanceType.IT_TEXTURE)
			select val.GetBoundObjectName() into name
			select new EntityID(name, InstanceType.IT_TEXTURE);
		using (IInstanceSet instances = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.CivTechService.GetActivePantryPaths() }))
		{
			IEnumerable<ITextureInstance> source = (IEnumerable<ITextureInstance>)instances.LoadEntities(base.CivTechService.ProjectMapService.LayeredPantry, entities);
			if (source.Any())
			{
				uint width = source.First().Width;
				uint height = source.First().Height;
				result = source.All((ITextureInstance text) => text.Height == height && text.Width == width);
			}
		}
		return result;
	}
}
