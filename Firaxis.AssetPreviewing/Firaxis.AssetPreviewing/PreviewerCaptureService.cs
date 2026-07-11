using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Controls;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IPreviewerCaptureService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerCaptureService : IPreviewerCaptureService, IInitializable, IDisposable
{
	private bool _captureData = false;

	private int _targetFPS = 30;

	private Size _captureSize = new Size(1024, 768);

	private const string IMAGE_SEQUENCE_ID = "Image Sequence";

	private string _preferredCodec;

	private BoundPropertyDescriptor _preferredCodecDescriptor;

	private int _compressionLevel = -1;

	private BoundPropertyDescriptor _compressionLevelDescriptor;

	private readonly CivTechContext _civTechContext = Context.EnsureCreated<CivTechContext>();

	private readonly ConcurrentQueue<Bitmap> _bitmapsToProcess = new ConcurrentQueue<Bitmap>();

	private readonly ISettingsService _settingsService;

	private readonly Thread _imageProcessingThread;

	private readonly AutoResetEvent _imageProcessingThreadSignal = new AutoResetEvent(initialState: false);

	private long _running = 0L;

	private object _processingImagesLock = new object();

	private IVideoFile _currentVideoFile = null;

	private ICodecLister _codecLister;

	private int _frameCounter;

	public bool CaptureData
	{
		get
		{
			return _captureData;
		}
		private set
		{
			_captureData = value;
		}
	}

	public int InterCaptureDelay { get; set; } = 100;

	public int TargetFPS
	{
		get
		{
			return _targetFPS;
		}
		set
		{
			_targetFPS = value;
		}
	}

	public Size CaptureSize
	{
		get
		{
			return _captureSize;
		}
		set
		{
			if (!(_captureSize == value))
			{
				_captureSize = value;
			}
		}
	}

	public IEnumerable<string> AvailableCodecs => _codecLister.AvailableVideoCodecs.Concat(new string[1] { "Image Sequence" });

	public string PreferredCodec
	{
		get
		{
			return _preferredCodec;
		}
		set
		{
			if (!(_preferredCodec == value) && (!AvailableCodecs.Any() || AvailableCodecs.Contains(value)))
			{
				_preferredCodec = value;
				if (_preferredCodecDescriptor != null)
				{
					_preferredCodecDescriptor.SetValue(null, _preferredCodec);
				}
			}
		}
	}

	public int CompressionLevel
	{
		get
		{
			return _compressionLevel;
		}
		set
		{
			if (_compressionLevel != value)
			{
				_compressionLevel = Math.Min(value, 10000);
				_compressionLevel = Math.Max(_compressionLevel, -1);
				if (_compressionLevelDescriptor != null)
				{
					_compressionLevelDescriptor.SetValue(null, _compressionLevel);
				}
			}
		}
	}

	private string CurrentFilePath { get; set; }

	public event EventHandler DataCaptureStarted;

	public event EventHandler DataCaptureEnded;

	protected virtual void OnDataCaptureStarted()
	{
		this.DataCaptureStarted?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnDataCaptureEnded()
	{
		this.DataCaptureEnded?.Invoke(this, EventArgs.Empty);
	}

	[ImportingConstructor]
	public PreviewerCaptureService(ISettingsService settingsService)
	{
		_codecLister = _civTechContext.CreateInstance<ICodecLister>();
		_settingsService = settingsService;
		_preferredCodec = AvailableCodecs.FirstOrDefault();
		_imageProcessingThread = new Thread(ProcessImageQueue);
		_imageProcessingThread.Name = "Previewer Video Thread";
		_imageProcessingThread.IsBackground = false;
	}

	public void Initialize()
	{
		Interlocked.Increment(ref _running);
		_imageProcessingThread.Start();
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => InterCaptureDelay, "Inter-animation capture delay".Localize(), "Previewer Animation Capturing".Localize(), "This is the time delay in milliseconds between animation renderings. This should give the animation system enough to reset its internal state.".Localize());
		BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => TargetFPS, "Target Capture FPS".Localize(), "Previewer Animation Capturing".Localize(), "This is the targeted FPS that the previewer will be recorded at.".Localize());
		_preferredCodecDescriptor = new BoundPropertyDescriptor(this, () => PreferredCodec, "Preferred Codec".Localize(), "Previewer Animation Capturing".Localize(), "The codec that will be used to encode videos.".Localize());
		_compressionLevelDescriptor = new BoundPropertyDescriptor(this, () => CompressionLevel, "Compression Level".Localize(), "Previewer Animation Capturing".Localize(), "Compression level for recorded videos.  -1 Signals to use whatever the default is for the selected Codec.".Localize());
		_settingsService.RegisterSettings("Previewer".Localize(), boundPropertyDescriptor);
		_settingsService.RegisterUserSettings("Previewer".Localize(), boundPropertyDescriptor);
		_settingsService.RegisterSettings("Previewer".Localize(), boundPropertyDescriptor2);
		_settingsService.RegisterUserSettings("Previewer".Localize(), boundPropertyDescriptor2);
		_settingsService.RegisterSettings("Previewer".Localize(), _preferredCodecDescriptor);
		_settingsService.RegisterUserSettings("Previewer".Localize(), _preferredCodecDescriptor);
		_settingsService.RegisterSettings("Previewer".Localize(), _compressionLevelDescriptor);
		_settingsService.RegisterUserSettings("Previewer".Localize(), _compressionLevelDescriptor);
	}

	public bool Start(string filePath)
	{
		if (_currentVideoFile != null)
		{
			BugSubmitter.SilentReport("Tried to start recording while another recording was in process.  @assign bwhitman");
			return false;
		}
		CurrentFilePath = filePath;
		if (PreferredCodec != "Image Sequence")
		{
			long codecFCCHandler = _codecLister.GetCodecFCCHandler(PreferredCodec);
			_currentVideoFile = _civTechContext.CreateInstance<IVideoFile>(new object[5] { filePath, TargetFPS, CaptureSize, codecFCCHandler, CompressionLevel });
		}
		CaptureData = true;
		_frameCounter = 0;
		OnDataCaptureStarted();
		return true;
	}

	public bool Stop(string filePath)
	{
		if (_currentVideoFile == null && PreferredCodec != "Image Sequence")
		{
			BugSubmitter.SilentReport("Tried to stop a recording even though no recordings are happening.  @assign bwhitman");
			return false;
		}
		if (CurrentFilePath != filePath)
		{
			BugSubmitter.SilentReport("Tried to stop a recording a different recording than the current recording.  @assign bwhitman");
			return false;
		}
		CaptureData = false;
		OnDataCaptureEnded();
		lock (_processingImagesLock)
		{
			if (!PreferredCodec.Equals("Image Sequence"))
			{
				_currentVideoFile.SaveFile();
			}
			if (_currentVideoFile != null)
			{
				_currentVideoFile.Dispose();
				_currentVideoFile = null;
			}
		}
		return true;
	}

	public bool CaptureScreen(Control control)
	{
		Bitmap bitmap = null;
		try
		{
			bitmap = control.ScreenshotContents();
		}
		catch (Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
			return false;
		}
		_bitmapsToProcess.Enqueue(bitmap);
		_imageProcessingThreadSignal.Set();
		return true;
	}

	private void ProcessImageQueue(object context)
	{
		while (Interlocked.Read(ref _running) > 0 && _imageProcessingThreadSignal.WaitOne())
		{
			lock (_processingImagesLock)
			{
				if (_currentVideoFile == null && !PreferredCodec.Equals("Image Sequence"))
				{
					continue;
				}
				Bitmap result;
				while (_bitmapsToProcess.TryDequeue(out result))
				{
					if (PreferredCodec.Equals("Image Sequence"))
					{
						string directoryName = Path.GetDirectoryName(CurrentFilePath);
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(CurrentFilePath);
						result.Save(Path.Combine(directoryName, string.Format(fileNameWithoutExtension + "_{0:D4}.tga", _frameCounter++)));
					}
					else
					{
						_currentVideoFile.AddFrame(result);
					}
					result.Dispose();
				}
			}
		}
	}

	public void Dispose()
	{
		lock (_processingImagesLock)
		{
			if (_currentVideoFile != null)
			{
				string filePath = _currentVideoFile.FilePath;
				Stop(filePath);
			}
			Bitmap result;
			while (_bitmapsToProcess.TryDequeue(out result))
			{
				result.Dispose();
			}
		}
		Interlocked.Decrement(ref _running);
		_imageProcessingThreadSignal.Set();
		if (_imageProcessingThread.IsAlive)
		{
			_imageProcessingThread.Join();
		}
		_imageProcessingThreadSignal.Dispose();
	}
}
