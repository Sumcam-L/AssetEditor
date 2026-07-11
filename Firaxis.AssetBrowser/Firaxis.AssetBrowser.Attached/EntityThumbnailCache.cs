using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Firaxis.AssetBrowser.Properties;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Texture;
using Firaxis.MVVMBase.Helpers;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.Attached;

public class EntityThumbnailCache
{
	private class ThumbnailCacheRequest : ITaskGenerator
	{
		private System.Windows.Controls.Image _requestingImage;

		private readonly string _name;

		private readonly InstanceType _requestedType;

		private readonly ViewMode _requestedViewMode;

		private readonly Dictionary<string, ImageSource> _associatedCache;

		private ImageSource _result;

		public ThumbnailCacheRequest(string name, InstanceType instanceType, ViewMode viewMode, System.Windows.Controls.Image image, Dictionary<string, ImageSource> associatedCache)
		{
			_name = name;
			_requestedType = instanceType;
			_requestedViewMode = viewMode;
			_requestingImage = image;
			_associatedCache = associatedCache;
			image.Unloaded += Image_Unloaded;
		}

		public async Task Generate()
		{
			if (_requestingImage == null)
			{
				return;
			}
			if (_associatedCache.TryGetValue(_name, out var imageSource))
			{
				_result = imageSource;
				_requestingImage.Source = _result;
				return;
			}
			await Task.Run(delegate
			{
				_result = FetchImage(_name, _requestedType, _requestedViewMode);
			});
			_associatedCache[_name] = _result;
			if (_requestingImage != null)
			{
				_requestingImage.Source = _result;
				_requestingImage.Unloaded -= Image_Unloaded;
				_requestingImage = null;
			}
		}

		private void Image_Unloaded(object sender, RoutedEventArgs e)
		{
			_requestingImage.Unloaded -= Image_Unloaded;
			_requestingImage = null;
		}
	}

	private static readonly Dictionary<string, ImageSource>[,] _cache;

	private static readonly ImageSource[] _defaultCache;

	private static readonly TaskStack _taskStack;

	public static readonly DependencyProperty SizeProperty;

	public static readonly DependencyProperty EntityProperty;

	public static ViewMode GetSize(System.Windows.Controls.Image target)
	{
		return (ViewMode)target.GetValue(SizeProperty);
	}

	public static void SetSize(System.Windows.Controls.Image target, ViewMode value)
	{
		target.SetValue(SizeProperty, value);
	}

	public static InstanceEntityViewModel GetEntity(System.Windows.Controls.Image target)
	{
		return (InstanceEntityViewModel)target.GetValue(EntityProperty);
	}

	public static void SetEntity(System.Windows.Controls.Image target, InstanceEntityViewModel value)
	{
		target.SetValue(EntityProperty, value);
	}

	private static async void EntityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is System.Windows.Controls.Image image)
		{
			await UpdateImage(image, e.NewValue as InstanceEntityViewModel);
		}
	}

	static EntityThumbnailCache()
	{
		_taskStack = new TaskStack();
		SizeProperty = DependencyProperty.RegisterAttached("Size", typeof(ViewMode), typeof(EntityThumbnailCache), new PropertyMetadata(ViewMode.ClassicDetails));
		EntityProperty = DependencyProperty.RegisterAttached("Entity", typeof(InstanceEntityViewModel), typeof(EntityThumbnailCache), new PropertyMetadata(null, EntityChanged));
		_cache = new Dictionary<string, ImageSource>[Enum.GetValues(typeof(InstanceType)).Length, Enum.GetValues(typeof(ViewMode)).Length];
		_defaultCache = new ImageSource[12]
		{
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.asset),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.material),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.geometry),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.texture),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.animation),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.state_graph),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.analytic_light),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.sun),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.lightrig),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.particle),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.asset),
			ImageHelper.CreateBitmapSourceFromBitmap(Resources.firefx)
		};
		ImageSource[] defaultCache = _defaultCache;
		foreach (ImageSource imageSource in defaultCache)
		{
			imageSource.Freeze();
		}
	}

	private static ImageSource FetchImage(string name, InstanceType requestedType, ViewMode viewMode)
	{
		CivTechContext civTechContext = CivTechRegistry.CivTechService.CivTechContext;
		using IInstanceSet instanceSet = civTechContext.CreateInstance<IInstanceSet>(new object[1] { CivTechRegistry.CivTechService.GetActivePantryPaths() });
		IInstanceEntity instanceEntity = instanceSet?.LoadEntityIfUnique(name, requestedType);
		if (instanceEntity == null)
		{
			return null;
		}
		int size = 32;
		switch (viewMode)
		{
		case ViewMode.ExtraLargeIcons:
			size = 256;
			break;
		case ViewMode.LargeIcons:
			size = 96;
			break;
		case ViewMode.MediumIcons:
			size = 48;
			break;
		case ViewMode.List:
			size = 16;
			break;
		case ViewMode.ClassicDetails:
			size = 32;
			break;
		case ViewMode.Tiles:
			size = 48;
			break;
		}
		return CreateThumbnail(civTechContext, instanceEntity, instanceSet, size);
	}

	public static ImageSource GetDefaultThumbnail(InstanceType instanceType)
	{
		return _defaultCache[(int)instanceType];
	}

	public static async Task UpdateImage(System.Windows.Controls.Image image, InstanceEntityViewModel entityViewModel)
	{
		if (entityViewModel != null)
		{
			string name = entityViewModel.Name;
			InstanceType instanceType = entityViewModel.InstanceType;
			ViewMode viewMode = GetSize(image);
			Dictionary<string, ImageSource> cache = _cache[(int)instanceType, (int)viewMode];
			ImageSource imageSource;
			if (cache == null)
			{
				cache = new Dictionary<string, ImageSource>();
				_cache[(int)instanceType, (int)viewMode] = cache;
			}
			else if (cache.TryGetValue(name, out imageSource))
			{
				image.Source = imageSource;
				return;
			}
			imageSource = GetDefaultThumbnail(instanceType);
			image.Source = imageSource;
			ThumbnailCacheRequest request = new ThumbnailCacheRequest(name, instanceType, viewMode, image, cache);
			await _taskStack.Run(request);
		}
	}

	private static ImageSource CreateThumbnail(CivTechContext context, IInstanceEntity entity, IInstanceSet instances, int size)
	{
		string text = ThumbnailPath(entity.Name, entity.Type);
		if (File.Exists(text))
		{
			Bitmap bitmap = null;
			try
			{
				bitmap = new Bitmap(text);
			}
			catch (Exception)
			{
			}
			if (bitmap == null)
			{
				return null;
			}
			Bitmap bitmap2 = new Bitmap(bitmap, new System.Drawing.Size(size, size));
			BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(bitmap2);
			bitmapSource?.Freeze();
			return bitmapSource;
		}
		if (entity.Type == InstanceType.IT_TEXTURE)
		{
			return CreateThumbnailForTexture(context, entity, size);
		}
		if (entity.Type == InstanceType.IT_MATERIAL)
		{
			return CreateThumbnailForMaterial(context, entity, instances, size);
		}
		if (entity.Type == InstanceType.IT_ANALYTIC_LIGHT)
		{
			return CreateThumbnailForAnalyticLight(entity);
		}
		return GetDefaultThumbnail(entity.Type);
	}

	private static ImageSource CreateThumbnailForMaterial(CivTechContext context, IInstanceEntity entity, IInstanceSet instances, int size)
	{
		if (instances == null)
		{
			return null;
		}
		if (!(entity.CookParameters.FindValue("BaseColor") is IObjectValue objectValue) || objectValue.GetBoundObjectType() != InstanceType.IT_TEXTURE)
		{
			return null;
		}
		string boundObjectName = objectValue.GetBoundObjectName();
		if (string.IsNullOrEmpty(boundObjectName))
		{
			return null;
		}
		return (!(instances.LoadEntityIfUnique(boundObjectName, InstanceType.IT_TEXTURE) is ITextureInstance entity2)) ? null : CreateThumbnail(context, entity2, instances, size);
	}

	private static ImageSource CreateThumbnailForAnalyticLight(IInstanceEntity entity)
	{
		if (!(entity.CookParameters.FindValue("Color") is IRGBValue iRGBValue))
		{
			return GetDefaultThumbnail(InstanceType.IT_ANALYTIC_LIGHT);
		}
		Rectangle rect = new Rectangle(0, 0, 2 * Resources.analytic_light.Width, 2 * Resources.analytic_light.Height);
		Rectangle rect2 = new Rectangle(Resources.analytic_light.Width / 2, Resources.analytic_light.Height / 2, Resources.analytic_light.Width, Resources.analytic_light.Height);
		Bitmap bitmap = new Bitmap(rect.Width, rect.Height, Resources.analytic_light.PixelFormat);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			using System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(iRGBValue.ParameterValue.A, iRGBValue.ParameterValue.R, iRGBValue.ParameterValue.G, iRGBValue.ParameterValue.B));
			graphics.FillRectangle(brush, rect);
			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.DrawImage(Resources.analytic_light, rect2);
		}
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(bitmap);
		bitmapSource?.Freeze();
		return bitmapSource;
	}

	private static ImageSource CreateThumbnailForTexture(CivTechContext context, IInstanceEntity entity, int size)
	{
		IInstanceDataFile instanceDataFile = entity.DataFiles.FirstOrDefault();
		if (instanceDataFile == null || string.IsNullOrEmpty(instanceDataFile.RelativePath))
		{
			return null;
		}
		string dataFilePath = entity.GetDataFilePath(instanceDataFile.RelativePath);
		using IDDSDataExtractor iDDSDataExtractor = context.CreateInstance<IDDSDataExtractor>();
		if (iDDSDataExtractor == null || !iDDSDataExtractor.LoadDDSFile(dataFilePath))
		{
			return null;
		}
		Bitmap bitmap = iDDSDataExtractor.CreateThumbnailImage(size, size);
		if (bitmap == null)
		{
			return null;
		}
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(bitmap);
		bitmapSource?.Freeze();
		return bitmapSource;
	}

	private static string ThumbnailPath(string Name, InstanceType type)
	{
		string text = "";
		string[] array = CivTechRegistry.CivTechService.ProjectMapService.LayeredPantry.PantryRoots.ToArray();
		string pantryPath = CivTechRegistry.CivTechService.ProjectMapService.LayeredPantry.GetPantryPath(Name, type);
		string text2 = null;
		string text3 = "/thumbnails";
		string extension = ".png";
		pantryPath = Path.ChangeExtension(pantryPath, extension);
		string[] array2 = array;
		foreach (string text4 in array2)
		{
			if (pantryPath.StartsWith(text4))
			{
				pantryPath = pantryPath.Remove(0, text4.Length);
				text2 = text4;
				break;
			}
		}
		return text2 + text3 + pantryPath;
	}
}
