using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class ArtDefinitionElementNameTypeConverter : StringConverter
{
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return context.GetService<IArtDefinitionInformationProvider>() != null;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return context.GetService<IArtDefinitionInformationProvider>() != null;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		IArtDefinitionInformationProvider service = context.GetService<IArtDefinitionInformationProvider>();
		if (service != null)
		{
			TriggerArtDefVFX triggerArtDefVFX = (TriggerArtDefVFX)context.Instance;
			if (service.ArtDefinitionInformation.ContainsKey(triggerArtDefVFX.CollectionName))
			{
				List<string> list = new List<string>();
				list.Add(string.Empty);
				list.AddRange(service.ArtDefinitionInformation[triggerArtDefVFX.CollectionName]);
				return new StandardValuesCollection(list);
			}
		}
		return null;
	}
}
