using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Interop;

public static class Util
{
	public static ImageSource GetOrCreateResourceForEmbeddedImage(Image embeddedImage)
	{
		object obj = System.Windows.Application.Current.TryFindResource(embeddedImage);
		if (obj == null)
		{
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			MemoryStream memoryStream = new MemoryStream();
			embeddedImage.Save(memoryStream, ImageFormat.Png);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			bitmapImage.StreamSource = memoryStream;
			bitmapImage.EndInit();
			System.Windows.Application.Current.Resources.Add(embeddedImage, bitmapImage);
			return bitmapImage;
		}
		if (!(obj is ImageSource result))
		{
			throw new InvalidOperationException("Image resource key already exists but value is not of expected type");
		}
		return result;
	}

	public static KeyGesture ConvertKey(Sce.Atf.Input.Keys atfKey)
	{
		return ConvertKey(KeysInterop.ToWf(atfKey));
	}

	public static KeyGesture ConvertKey(System.Windows.Forms.Keys formsKey)
	{
		System.Windows.Forms.Keys virtualKey = formsKey & System.Windows.Forms.Keys.KeyCode;
		Key key = KeyInterop.KeyFromVirtualKey((int)virtualKey);
		ModifierKeys modifierKeys = ModifierKeys.None;
		if ((formsKey & System.Windows.Forms.Keys.Shift) > System.Windows.Forms.Keys.None)
		{
			modifierKeys |= ModifierKeys.Shift;
		}
		if ((formsKey & System.Windows.Forms.Keys.Control) > System.Windows.Forms.Keys.None)
		{
			modifierKeys |= ModifierKeys.Control;
		}
		if ((formsKey & System.Windows.Forms.Keys.Alt) > System.Windows.Forms.Keys.None)
		{
			modifierKeys |= ModifierKeys.Alt;
		}
		return new KeyGesture(key, modifierKeys);
	}
}
