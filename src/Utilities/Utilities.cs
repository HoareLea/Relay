using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using Humanizer;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Relay.Utilities
{
    public static class StringUtilities
    {
        public static string GenerateButtonText(this string fileName)
        {
            return fileName.Replace(".dyn", "").Replace(' ','\n').Truncate(20);
        }

        public static string GetStringBetweenCharacters(this string input, char charFrom, char charTo)
        {
            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
    }
    public static class ImageUtils
    {
        public static BitmapImage LoadImage(Assembly a, string name)
        {
            var img = new BitmapImage();
            try
            {
                var resourceName = a.GetManifestResourceNames().FirstOrDefault(x => x.Contains(name));
                var stream = a.GetManifestResourceStream(resourceName);

                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();
            }
            catch (Exception e)
            {
                // ignored
            }

            return img;
        }

		/// <summary>
		/// Convert a BitmapImage to Bitmap
		/// </summary>
		static Bitmap BitmapImageToBitmap(
		  BitmapImage bitmapImage )
		{
			//BitmapImage bitmapImage = new BitmapImage(
			// new Uri("../Images/test.png", UriKind.Relative));

			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapImage));
				enc.Save(outStream);
				Bitmap bitmap = new Bitmap( outStream );

				return new Bitmap(bitmap);
			}
		}

		//public static extern bool DeleteObject( IntPtr hObject );

		/// <summary>
		/// Convert a Bitmap to a BitmapSource
		/// </summary>
		static BitmapSource BitmapToBitmapSource( Bitmap bitmap )
		{
			IntPtr hBitmap = bitmap.GetHbitmap();

			BitmapSource retval;

			try
			{
				retval = Imaging.CreateBitmapSourceFromHBitmap(
				  hBitmap, IntPtr.Zero, Int32Rect.Empty,
				  BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				// DeleteObject(hBitmap);
			}
			return retval;
		}

		/// <summary>
		/// Resize the image to the specified width and height.
		/// </summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="width">The width to resize to.</param>
		/// <param name="height">The height to resize to.</param>
		/// <returns>The resized image.</returns>
		static Bitmap ResizeImage(
		  Image image,
		  int width,
		  int height )
		{
			var destRect = new System.Drawing.Rectangle(
			0, 0, width, height );

			var destImage = new Bitmap( width, height );

			destImage.SetResolution(image.HorizontalResolution,
			  image.VerticalResolution);

			using (var g = Graphics.FromImage(destImage))
			{
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					g.DrawImage(image, destRect, 0, 0, image.Width,
					  image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}
			return destImage;
		}

		/// <summary>
		/// Scale down large icon to desired size for Revit 
		/// ribbon button, e.g., 32 x 32 or 16 x 16
		/// </summary>
		public static BitmapSource ScaledIcon(
		  BitmapImage large_icon,
		  int w,
		  int h )
		{
			return BitmapToBitmapSource(ResizeImage(
			  BitmapImageToBitmap(large_icon), w, h));
		}
	}
}
﻿using System.Reflection;
using System.Windows.Media.Imaging;
using Humanizer;

namespace Relay.Utilities
{
    public static class StringUtilities
    {
        public static string GenerateButtonText(this string fileName)
        {
            return fileName.Replace(".dyn", "").Replace(' ','\n').Truncate(20);
        }
        public static string GetStringBetweenCharacters(this string input, char charFrom, char charTo)
        {
            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
    }
    public static class ImageUtils
    {
        public static BitmapImage LoadImage(Assembly a, string name)
        {
            var img = new BitmapImage();
            try
            {
                var resourceName = a.GetManifestResourceNames().FirstOrDefault(x => x.Contains(name));
                var stream = a.GetManifestResourceStream(resourceName);

                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();
            }
            catch (Exception e)
            {
                // ignored
            }

            return img;
        }
    }
}
