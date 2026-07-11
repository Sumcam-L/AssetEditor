using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Applications;

[Export(typeof(ThumbnailService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ThumbnailService : IInitializable
{
	private class ResolvedThumbnail
	{
		public readonly Uri ResourceUri;

		public readonly Image Image;

		public ResolvedThumbnail(Uri resourceUri, Image image)
		{
			ResourceUri = resourceUri;
			Image = image;
		}
	}

	[ImportMany]
	private IEnumerable<IThumbnailResolver> m_resolvers;

	private Thread m_workThread;

	private readonly Queue<Uri> m_resourcesToResolve = new Queue<Uri>();

	private readonly Queue<ResolvedThumbnail> m_resolvedResources = new Queue<ResolvedThumbnail>();

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

	public ThumbnailService()
	{
		Application.Idle += Application_Idle;
	}

	public virtual void Initialize()
	{
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

	public void ResolveThumbnail(Uri resourceUri)
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

	protected virtual void OnThumbnailReady(ThumbnailReadyEventArgs e)
	{
		this.ThumbnailReady?.Invoke(this, e);
	}

	private void StartThread()
	{
		m_workThread = new Thread(ResolverThread)
		{
			Name = "thumbnail service",
			IsBackground = true,
			CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
			Priority = ThreadPriority.Lowest
		};
		m_workThread.Start();
	}

	private bool IsThreadAlive()
	{
		return m_workThread != null && m_workThread.IsAlive;
	}

	private void Application_Idle(object sender, EventArgs e)
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
		Uri uri = null;
		do
		{
			lock (m_resourcesToResolve)
			{
				uri = ((m_resourcesToResolve.Count <= 0) ? null : m_resourcesToResolve.Dequeue());
			}
			if (!(uri != null))
			{
				continue;
			}
			lock (m_resolvers)
			{
				foreach (IThumbnailResolver resolver in m_resolvers)
				{
					try
					{
						Image image = resolver.Resolve(uri);
						if (image != null)
						{
							lock (m_resolvedResources)
							{
								m_resolvedResources.Enqueue(new ResolvedThumbnail(uri, image));
							}
							break;
						}
					}
					catch (Exception ex)
					{
						Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
					}
				}
			}
		}
		while (uri != null);
	}
}
