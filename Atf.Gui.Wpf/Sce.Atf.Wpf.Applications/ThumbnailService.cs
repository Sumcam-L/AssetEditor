using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(ThumbnailService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ThumbnailService : IInitializable
{
	private class ResolvedThumbnail
	{
		public readonly Uri ResourceUri;

		public readonly object Image;

		public ResolvedThumbnail(Uri resourceUri, object image)
		{
			ResourceUri = resourceUri;
			Image = image;
		}
	}

	[ImportMany]
	private IEnumerable<IThumbnailResolver> m_resolvers = null;

	private Thread m_workThread;

	private DispatcherTimer m_timer;

	private readonly Queue<ResolvedThumbnail> m_resolvedResources = new Queue<ResolvedThumbnail>();

	private readonly Queue<ThumbnailParameters> m_resourcesToResolve = new Queue<ThumbnailParameters>();

	private static XmlResolver s_defaultXmlResolver = new FindFileResolver();

	public static XmlResolver DefaultXmlResolver
	{
		get
		{
			return s_defaultXmlResolver;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			s_defaultXmlResolver = value;
		}
	}

	public event EventHandler<ThumbnailReadyEventArgs> ThumbnailReady;

	public void Initialize()
	{
		m_timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0), DispatcherPriority.ApplicationIdle, DispatcherThreadIdle, Application.Current.Dispatcher);
	}

	public static string GetResourcePath(Uri resourceUri)
	{
		string result = null;
		string localPath = resourceUri.LocalPath;
		Uri uri = DefaultXmlResolver.ResolveUri(null, localPath);
		if (uri != null)
		{
			result = uri.LocalPath;
		}
		return result;
	}

	public void ResolveThumbnail(ThumbnailParameters resourceUri)
	{
		lock (m_resourcesToResolve)
		{
			m_resourcesToResolve.Enqueue(resourceUri);
		}
		if (!IsThreadAlive())
		{
			StartThread();
		}
	}

	public object ResolveThumbnailBlocking(ThumbnailParameters resourceUri)
	{
		object obj = null;
		foreach (IThumbnailResolver resolver in m_resolvers)
		{
			obj = resolver.Resolve(resourceUri);
			if (obj != null)
			{
				break;
			}
		}
		return obj;
	}

	protected virtual void OnThumbnailReady(ThumbnailReadyEventArgs e)
	{
		this.ThumbnailReady?.Invoke(this, e);
	}

	private void StartThread()
	{
		m_workThread = new Thread(ResolverThread);
		m_workThread.Name = "thumbnail service";
		m_workThread.IsBackground = true;
		m_workThread.SetApartmentState(ApartmentState.STA);
		m_workThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
		m_workThread.Start();
	}

	private bool IsThreadAlive()
	{
		return m_workThread != null && m_workThread.IsAlive;
	}

	private void DispatcherThreadIdle(object sender, EventArgs e)
	{
		if (!IsThreadAlive())
		{
			while (m_resolvedResources.Count > 0)
			{
				ResolvedThumbnail resolvedThumbnail = m_resolvedResources.Dequeue();
				OnThumbnailReady(new ThumbnailReadyEventArgs(resolvedThumbnail.ResourceUri, resolvedThumbnail.Image));
			}
			if (m_resourcesToResolve.Count > 0)
			{
				StartThread();
			}
			return;
		}
		lock (m_resolvedResources)
		{
			while (m_resolvedResources.Count > 0)
			{
				ResolvedThumbnail resolvedThumbnail2 = m_resolvedResources.Dequeue();
				OnThumbnailReady(new ThumbnailReadyEventArgs(resolvedThumbnail2.ResourceUri, resolvedThumbnail2.Image));
			}
		}
	}

	private void ResolverThread()
	{
		ThumbnailParameters thumbnailParameters = null;
		do
		{
			lock (m_resourcesToResolve)
			{
				thumbnailParameters = ((m_resourcesToResolve.Count <= 0) ? null : m_resourcesToResolve.Dequeue());
			}
			if (thumbnailParameters == null)
			{
				continue;
			}
			lock (m_resolvers)
			{
				object obj = null;
				foreach (IThumbnailResolver resolver in m_resolvers)
				{
					try
					{
						obj = resolver.Resolve(thumbnailParameters);
						if (obj != null)
						{
							lock (m_resolvedResources)
							{
								m_resolvedResources.Enqueue(new ResolvedThumbnail(thumbnailParameters.Source, obj));
							}
							break;
						}
					}
					catch (Exception ex)
					{
						Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
					}
				}
				if (obj == null)
				{
					lock (m_resolvedResources)
					{
						m_resolvedResources.Enqueue(new ResolvedThumbnail(thumbnailParameters.Source, obj));
					}
				}
			}
		}
		while (thumbnailParameters != null);
	}
}
