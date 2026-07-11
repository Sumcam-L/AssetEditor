using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Media;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Interop;

[Export(typeof(Sce.Atf.Applications.IStatusService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StatusServiceAdapter : Sce.Atf.Applications.IStatusService
{
	private class StatusTextAdapter : IStatusText
	{
		private StatusText m_adaptee;

		public string Text
		{
			get
			{
				return m_adaptee.Text;
			}
			set
			{
				m_adaptee.Text = value;
			}
		}

		public System.Drawing.Color ForeColor
		{
			get
			{
				if (!(m_adaptee.ForeColor is SolidColorBrush { Color: var color } solidColorBrush))
				{
					throw new InvalidOperationException("Not a solid color brush");
				}
				return System.Drawing.Color.FromArgb(color.A, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
			}
			set
			{
				m_adaptee.ForeColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(value.A, value.R, value.G, value.B));
			}
		}

		public StatusTextAdapter(StatusText adaptee)
		{
			m_adaptee = adaptee;
		}
	}

	private class StatusImageAdapter : IStatusImage
	{
		private StatusImage m_adaptee;

		public Image Image
		{
			get
			{
				return m_adaptee.ImageSourceKey as Image;
			}
			set
			{
				if (value != null)
				{
					Util.GetOrCreateResourceForEmbeddedImage(value);
				}
				m_adaptee.ImageSourceKey = value;
			}
		}

		public StatusImageAdapter(StatusImage adaptee)
		{
			m_adaptee = adaptee;
		}
	}

	[Import]
	private IComposer m_composer = null;

	[Import]
	private Sce.Atf.Wpf.Applications.IStatusService m_adaptee = null;

	public event EventHandler ProgressCancelled;

	public void ShowStatus(string status)
	{
		m_adaptee.ShowStatus(status);
	}

	public IStatusText AddText(int width)
	{
		StatusText statusText = new StatusText(width);
		m_composer.Container.ComposeExportedValue((IStatusItem)statusText);
		return new StatusTextAdapter(statusText);
	}

	public IStatusImage AddImage()
	{
		StatusImage statusImage = new StatusImage();
		m_composer.Container.ComposeExportedValue((IStatusItem)statusImage);
		return new StatusImageAdapter(statusImage);
	}

	public void BeginProgress(string message)
	{
		throw new NotImplementedException();
	}

	public void BeginProgress(string message, int expectedDuration)
	{
		throw new NotImplementedException();
	}

	public void BeginProgress(string message, bool canCancel)
	{
		throw new NotImplementedException();
	}

	public void BeginProgress(string message, int expectedDuration, bool canCancel)
	{
		throw new NotImplementedException();
	}

	public void ShowProgress(double progress)
	{
		throw new NotImplementedException();
	}

	public void EndProgress()
	{
		throw new NotImplementedException();
	}
}
