using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class BehaviorDocument : BaseInstanceEntityDocument
{
	protected override void HandleDocumentImported(object sender, DocumentImportedEventArgs e)
	{
		base.HandleDocumentImported(sender, e);
		if (e.Successful && e.Document.InstanceEntity is IGeometryInstance geometryInstance)
		{
			for (IInstanceEntity instanceEntity = base.InstanceSet.FindByNameAndType(geometryInstance.Name, geometryInstance.Type); instanceEntity != null; instanceEntity = base.InstanceSet.FindByNameAndType(geometryInstance.Name, geometryInstance.Type))
			{
				base.InstanceSet.Remove(instanceEntity);
			}
		}
	}
}
