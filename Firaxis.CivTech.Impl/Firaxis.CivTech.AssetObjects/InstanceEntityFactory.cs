using System;
using System.Collections.Generic;
using System.Reflection;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class InstanceEntityFactory
{
	private Dictionary<Type, ConstructorInfo> m_factoryFunctions;

	private object[] m_argsArray;

	private Type[] m_typeArray;

	private unsafe global::AssetObjects.InstanceSet* m_instanceSet;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private unsafe Serializer* m_serializer;

	private unsafe global::AssetObjects.VirtualPantry* m_virtualPantry;

	public unsafe InstanceEntityFactory(global::AssetObjects.InstanceSet* instanceSet, global::AssetObjects.Deserializer* deserializer, Serializer* serializer, global::AssetObjects.VirtualPantry* virtualPantry)
	{
		m_factoryFunctions = new Dictionary<Type, ConstructorInfo>();
		m_instanceSet = instanceSet;
		m_deserializer = deserializer;
		m_serializer = serializer;
		m_virtualPantry = virtualPantry;
		base._002Ector();
		object[] array = new object[4];
		object obj = Pointer.Box(m_instanceSet, typeof(global::AssetObjects.InstanceSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(m_deserializer, typeof(global::AssetObjects.Deserializer*));
		array[1] = obj2;
		object obj3 = Pointer.Box(m_serializer, typeof(Serializer*));
		array[2] = obj3;
		object obj4 = Pointer.Box(m_virtualPantry, typeof(global::AssetObjects.VirtualPantry*));
		array[3] = obj4;
		m_argsArray = array;
		Type[] array2 = new Type[4];
		Type typeFromHandle = typeof(global::AssetObjects.InstanceSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(global::AssetObjects.Deserializer*);
		array2[1] = typeFromHandle2;
		Type typeFromHandle3 = typeof(Serializer*);
		array2[2] = typeFromHandle3;
		Type typeFromHandle4 = typeof(global::AssetObjects.VirtualPantry*);
		array2[3] = typeFromHandle4;
		m_typeArray = array2;
		RegisterDefaultFactoryFunctions();
	}

	public IInstanceEntity CreateEntity<T>() where T : IInstanceEntity
	{
		ConstructorInfo constructorInfo = null;
		Type typeFromHandle = typeof(T);
		constructorInfo = null;
		if (!m_factoryFunctions.TryGetValue(typeFromHandle, out constructorInfo))
		{
			BugSubmitter.SilentAssert(m_factoryFunctions.ContainsKey(typeFromHandle), $"Failed to find registered constructor for type '{typeFromHandle.ToString()}.  Attempting to register a default constructor.  @assign bwhitman");
			RegisterDefaultConstructor<T>();
			if (!m_factoryFunctions.TryGetValue(typeFromHandle, out constructorInfo))
			{
				BugSubmitter.SilentAssert(m_factoryFunctions.ContainsKey(typeFromHandle), $"Failed to register default constructor for type '{typeFromHandle.ToString()}.  @assign bwhitman");
				return null;
			}
		}
		IInstanceEntity instanceEntity = (IInstanceEntity)constructorInfo.Invoke(m_argsArray);
		byte condition = ((instanceEntity != null) ? ((byte)1) : ((byte)0));
		BugSubmitter.SilentAssert(condition != 0, $"Entity is null after invoking its constructor.  Entity type: '{typeFromHandle.ToString()}'.  @assign bwhitman");
		if (instanceEntity == null)
		{
			return null;
		}
		InstanceEntity instanceEntity2 = (InstanceEntity)instanceEntity;
		byte condition2 = ((instanceEntity2 != null) ? ((byte)1) : ((byte)0));
		BugSubmitter.SilentAssert(condition2 != 0, $"safe_cast<InstanceEntity^> failed on entity of type: '{typeFromHandle.ToString()}'.  @assign bwhitman");
		instanceEntity2?.AddReferences();
		return instanceEntity;
	}

	public void RegisterFactoryFunction<T>(ConstructorInfo factoryFunc) where T : IInstanceEntity
	{
		Type typeFromHandle = typeof(T);
		byte condition = ((!m_factoryFunctions.ContainsKey(typeFromHandle)) ? ((byte)1) : ((byte)0));
		BugSubmitter.SilentAssert(condition != 0, $"Duplicate constructor being registered for type \"{typeFromHandle.ToString()}\".  @summary Duplicate constructor registration in InstanceEntityFactory @assign bwhitman");
		m_factoryFunctions[typeFromHandle] = factoryFunc;
	}

	private void RegisterDefaultFactoryFunctions()
	{
		RegisterDefaultConstructor<IAnalyticLightInstance>();
		RegisterDefaultConstructor<IAnimationInstance>();
		RegisterDefaultConstructor<IAssetInstance>();
		RegisterDefaultConstructor<IBehaviorInstance>();
		RegisterDefaultConstructor<IDSGInstance>();
		RegisterDefaultConstructor<IEnvironmentLightInstance>();
		RegisterDefaultConstructor<IFireFXInstance>();
		RegisterDefaultConstructor<IGeometryInstance>();
		RegisterDefaultConstructor<IGeometryInstanceBuildable>();
		RegisterDefaultConstructor<ILightRigInstance>();
		RegisterDefaultConstructor<IMaterialInstance>();
		RegisterDefaultConstructor<IParticleEffectInstance>();
		RegisterDefaultConstructor<ITextureInstance>();
	}

	private ConstructorInfo FindDefaultConstructor<T>() where T : IInstanceEntity
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x09079382_002EGetConstructor<T>(Assembly.GetExecutingAssembly(), m_argsArray, m_typeArray);
	}

	private void RegisterDefaultConstructor<T>() where T : IInstanceEntity
	{
		ConstructorInfo factoryFunc = FindDefaultConstructor<T>();
		RegisterFactoryFunction<T>(factoryFunc);
	}
}
