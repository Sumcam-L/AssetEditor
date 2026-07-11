using System.Collections.Generic;
using System.Text;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.CivTech;

public static class IQueryServiceExtensions
{
	public static void Add(this IQueryService request, IInstanceEntity entity)
	{
		if (entity == null)
		{
			ExceptionLogger.Log("IQueryService.Add", "Entity is null");
		}
		else
		{
			request.InstanceItems[entity.Type].Add(entity.Name);
		}
	}

	public static void AddRange(this IQueryService request, IEnumerable<IInstanceEntity> entities)
	{
		foreach (IInstanceEntity entity in entities)
		{
			request.Add(entity);
		}
	}

	public static void AddRange(this IQueryService request, IQueryService other)
	{
		for (int i = 0; i < 13; i++)
		{
			request.InstanceItems[(InstanceType)i].AddRange(other.InstanceItems[(InstanceType)i]);
		}
	}

	public static bool IsEmpty(this IQueryService request)
	{
		for (int i = 0; i < 13; i++)
		{
			if (request.InstanceItems[(InstanceType)i].Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	private static string ToParameterString(string parameterName, List<string> list)
	{
		StringBuilder builder = new StringBuilder();
		if (list.Count > 0)
		{
			builder.Append(parameterName);
			builder.Append('=');
			list.ForEach(delegate(string item)
			{
				builder.Append(item);
				builder.Append(',');
			});
			builder.Remove(builder.Length - 1, 1);
			return builder.ToString();
		}
		return string.Empty;
	}

	public static string[] ToParameterArray(this IQueryService request)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 13; i++)
		{
			InstanceType key = (InstanceType)i;
			list.Add(ToParameterString(key.ToString(), request.InstanceItems[key]));
		}
		return list.ToArray();
	}
}
