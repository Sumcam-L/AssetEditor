using System.ComponentModel.Composition;

namespace Firaxis.AssetEditing;

[Export(typeof(EntitySchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaLoader
{
	[ImportingConstructor]
	public EntitySchemaLoader(EntitySchemaInitializer initializer, EntitySchemaExtension extensions)
	{
	}
}
