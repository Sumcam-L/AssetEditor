using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;

namespace Sce.Atf;

[Export(typeof(ResourceService))]
[Export(typeof(IInitializable))]
[Export(typeof(IResourceService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ResourceService : IInitializable, IResourceService
{
	private readonly Dictionary<Uri, IResource> m_resourceMap = new Dictionary<Uri, IResource>();

	[ImportMany(typeof(IResourceResolver))]
	private IEnumerable<IResourceResolver> m_resolvers;

	public IEnumerable<IResource> Resources => m_resourceMap.Values;

	public event EventHandler<ItemChangedEventArgs<IResource>> ResourceLoaded;

	public event EventHandler<ItemChangedEventArgs<IResource>> ResourceUnloaded;

	public void Initialize()
	{
	}

	public IResource Load(Uri uri)
	{
		if (!m_resourceMap.TryGetValue(uri, out var value))
		{
			foreach (IResourceResolver resolver in m_resolvers)
			{
				value = resolver.Resolve(uri);
				if (value != null)
				{
					m_resourceMap[uri] = value;
					OnResourceLoaded(value);
					break;
				}
			}
		}
		return value;
	}

	public IResource GetResource(Uri uri)
	{
		IResource value = null;
		m_resourceMap.TryGetValue(uri, out value);
		return value;
	}

	public bool Unload(Uri uri)
	{
		if (m_resourceMap.TryGetValue(uri, out var value))
		{
			Unload(value);
			m_resourceMap.Remove(uri);
			return true;
		}
		return false;
	}

	private void Unload(IResource resource)
	{
		resource.As<IDisposable>()?.Dispose();
	}

	protected virtual void OnResourceLoaded(IResource resource)
	{
		if (this.ResourceLoaded != null)
		{
			this.ResourceLoaded(this, new ItemChangedEventArgs<IResource>(resource));
		}
	}

	protected virtual void OnResourceUnloaded(IResource resource)
	{
		if (this.ResourceUnloaded != null)
		{
			this.ResourceUnloaded(this, new ItemChangedEventArgs<IResource>(resource));
		}
	}
}
