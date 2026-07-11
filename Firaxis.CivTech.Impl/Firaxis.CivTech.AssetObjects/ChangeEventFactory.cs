using System;
using System.Collections.Generic;
using System.Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class ChangeEventFactory
{
	private static Dictionary<Type, ConstructorInfo> m_factoryFunctions;

	private static object[] m_emptyArgsArray;

	private static Type[] m_emptyTypeArray;

	static ChangeEventFactory()
	{
		m_factoryFunctions = null;
		m_emptyArgsArray = null;
		m_emptyTypeArray = null;
		m_factoryFunctions = new Dictionary<Type, ConstructorInfo>();
		m_emptyArgsArray = new object[0];
		m_emptyTypeArray = new Type[0];
		RegisterDefaultFactoryFunctions();
	}

	public static T CreateChangeEvent<T>() where T : IEntityChangedEvent
	{
		ConstructorInfo constructorInfo = null;
		Type typeFromHandle = typeof(T);
		constructorInfo = null;
		if (!m_factoryFunctions.TryGetValue(typeFromHandle, out constructorInfo))
		{
			BugSubmitter.SilentAssert(m_factoryFunctions.ContainsKey(typeFromHandle), $"Failed to find registered constructor for type '{typeFromHandle.ToString()}.  @assign bwhitman");
			return default(T);
		}
		return (T)constructorInfo.Invoke(m_emptyArgsArray);
	}

	public static void RegisterFactoryFunction<T>(ConstructorInfo factoryFunc) where T : IEntityChangedEvent
	{
		Type typeFromHandle = typeof(T);
		m_factoryFunctions[typeFromHandle] = factoryFunc;
	}

	private static void RegisterDefaultFactoryFunctions()
	{
		RegisterDefaultConstructor<IEntityChangedEvent>();
		RegisterDefaultConstructor<IEntityCookParameterChanged>();
		RegisterDefaultConstructor<IAssetDSGChanged>();
		RegisterDefaultConstructor<IAssetTimelineChanged>();
		RegisterDefaultConstructor<IAssetTimelineRemoved>();
		RegisterDefaultConstructor<IAssetTimelineSetChanged>();
		RegisterDefaultConstructor<IAssetAnimationSetChanged>();
		RegisterDefaultConstructor<IModelInstanceRemoved>();
		RegisterDefaultConstructor<IModelInstanceChanged>();
		RegisterDefaultConstructor<ITriGroupParameterChanged>();
		RegisterDefaultConstructor<IParticleEffectAdded>();
		RegisterDefaultConstructor<IParticleEffectRemoved>();
		RegisterDefaultConstructor<ILightTagDirectionChanged>();
		RegisterDefaultConstructor<ILightTagDirectionRemoved>();
		RegisterDefaultConstructor<IBehaviorAdded>();
		RegisterDefaultConstructor<IBehaviorRemoved>();
		RegisterDefaultConstructor<IAttachmentRemoved>();
		RegisterDefaultConstructor<IAttachmentChanged>();
		RegisterDefaultConstructor<IAttachmentCookParameterChanged>();
		RegisterDefaultConstructor<ISplineVertexChanged>();
	}

	private static ConstructorInfo FindDefaultConstructor<T>() where T : IEntityChangedEvent
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x1659d4a8_002EGetConstructor<T>(Assembly.GetExecutingAssembly(), m_emptyArgsArray, m_emptyTypeArray);
	}

	private static void RegisterDefaultConstructor<T>() where T : IEntityChangedEvent
	{
		RegisterFactoryFunction<T>(FindDefaultConstructor<T>());
	}
}
