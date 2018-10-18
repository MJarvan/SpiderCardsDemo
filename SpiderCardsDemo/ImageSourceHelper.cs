using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text;
using System.Drawing;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Brushes = System.Windows.Media.Brushes;

namespace SpiderCardsDemo
{
	/// <summary>
	/// 图像工具类
	/// </summary>
	public static class ImageSourceHelper
	{
		/// <summary>
		/// 将一个Visual对象渲染成一幅图片返回
		/// </summary>
		/// <param name="visual">目标Visual</param>
		/// <returns>该Visual截图</returns>
		public static ImageSource VisualToImageSource(Visual visual)
		{
			if(visual == null)
			{
				throw new ArgumentNullException("visual");
			}

			double doubleDpi = 1.0;

			Rect rect = VisualTreeHelper.GetDescendantBounds(visual);
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
				(int)(rect.Width * doubleDpi),
				(int)(rect.Height * doubleDpi),
				(int)doubleDpi * 96,
				(int)doubleDpi * 96,
				PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			using(DrawingContext drawingContext = drawingVisual.RenderOpen())
			{
				VisualBrush visualBrush = new VisualBrush(visual);
				drawingContext.DrawRectangle(visualBrush,null,rect);
			}
			renderTargetBitmap.Render(drawingVisual);

			return renderTargetBitmap;
		}

		/// <summary>
		/// 将图片集合纵向合并图片
		/// </summary>
		/// <param name="listImageSource">图片集合</param>
		/// <returns>合成后的图片</returns>
		public static BitmapSource JoinImageSourceToBitmapSource(IEnumerable<ImageSource> listImageSource)
		{
			if(listImageSource == null || !listImageSource.Any())
			{
				return null;
			}

			double doubleDpi = 96.0d;

			// 计算上下拼接后的图片大小
			Rect rectAll = new Rect(new Size(
				listImageSource.Max(m => m.Width),
				listImageSource.Sum(s => s.Height)));

			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
				(int)(rectAll.Width),
				(int)(rectAll.Height),
				doubleDpi,
				doubleDpi,
				PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			using(DrawingContext drawingContext = drawingVisual.RenderOpen())
			{
				// 绘制背景
				drawingContext.DrawRectangle(Brushes.White,null,rectAll);

				// 拼接
				double doubleCurrentY = 0d;
				foreach(ImageSource imageSource in listImageSource)
				{
					Rect rect = new Rect(
						new Point(0,doubleCurrentY),
						new Point(imageSource.Width,imageSource.Height));

					ImageBrush imageBrush = new ImageBrush(imageSource);
					drawingContext.DrawRectangle(imageBrush,null,rect);
					doubleCurrentY += imageSource.Height;
				}
			}
			renderTargetBitmap.Render(drawingVisual);
			return renderTargetBitmap;
		}

		/// <summary>
		/// 将一个图片切片为多个图片
		/// </summary>
		/// <param name="bitmapSource"></param>
		/// <param name="size"></param>
		/// <param name="isCutHeight"></param>
		/// <returns></returns>
		public static List<BitmapSource> CutBitmapSourceToBitmapSourceList(BitmapSource bitmapSource,System.Drawing.Size size,bool isCutHeight = true)
		{
			List<BitmapSource> listBitmapSource = new List<BitmapSource>();

			int intWidth = (int)bitmapSource.Width;
			int intHeight = (int)bitmapSource.Height;

			if(isCutHeight)
			{
				int intCount = intHeight / size.Height;
				intCount = intCount + 1;
				for(int i = 0;i < intCount;i++)
				{
					try
					{
						//定义切割矩形
						Int32Rect int32Rect = new Int32Rect(0,i * size.Height,intWidth,size.Height);
						//计算Stride
						var stride = bitmapSource.Format.BitsPerPixel * int32Rect.Width;
						//声明字节数组
						byte[] data = new byte[int32Rect.Height * stride];
						//调用CopyPixels
						bitmapSource.CopyPixels(int32Rect,data,stride,0);

						listBitmapSource.Add(BitmapSource.Create(intWidth,size.Height,0,0,PixelFormats.Pbgra32,null,data,stride));
					}
					catch
					{
					}
				}
			}
			else
			{
				int intCount = intHeight / size.Width;
				intCount = intCount + 1;
				for(int i = 0;i < intCount;i++)
				{
					try
					{
						//定义切割矩形
						Int32Rect int32Rect = new Int32Rect(i * size.Width,0,size.Width,intHeight);
						//计算Stride
						var stride = bitmapSource.Format.BitsPerPixel * int32Rect.Width;
						//声明字节数组
						byte[] data = new byte[int32Rect.Height * stride];
						//调用CopyPixels
						bitmapSource.CopyPixels(int32Rect,data,stride,0);

						listBitmapSource.Add(BitmapSource.Create(size.Width,intHeight,0,0,PixelFormats.Pbgra32,null,data,stride));
					}
					catch
					{
					}
				}
			}

			return listBitmapSource;
		}

		/// <summary>
		/// 将一个图片切片为多个图片
		/// </summary>
		/// <param name="image"></param>
		/// <param name="size"></param>
		/// <param name="isCutHeight"></param>
		/// <returns></returns>
		public static List<Image> CutImageToImageList(Image image,System.Drawing.Size size,bool isCutHeight = true)
		{
			List<Image> listImage = new List<Image>();

			int intWidth = (int)image.Width;
			int intHeight = (int)image.Height;

			if(isCutHeight)
			{
				int intCount = intHeight / size.Height;
				intCount = intCount + 1;
				for(int i = 0;i < intCount;i++)
				{
					try
					{
						listImage.Add(ImageCutRectangle(image,new Rectangle(0,i * size.Height,intWidth,size.Height)));
					}
					catch
					{
					}
				}
			}
			else
			{
				int intCount = intWidth / size.Width;
				intCount = intCount + 1;
				for(int i = 0;i < intCount;i++)
				{
					try
					{
						listImage.Add(ImageCutRectangle(image,new Rectangle(i * size.Width,0,intHeight,size.Width)));
					}
					catch
					{
					}
				}
			}

			return listImage;
		}

		/// <summary>
		/// 截取图像的矩形区域
		/// </summary>
		/// <param name="image">源图像对应image</param>
		/// <param name="rectangle">矩形区域，如上初始化的rectangle</param>
		/// <returns>矩形区域的图像</returns>
		public static Image ImageCutRectangle(Image image,Rectangle rectangle)
		{
			if(image == null || rectangle.IsEmpty)
			{
				return null;
			}
			Bitmap bitmap = new Bitmap(rectangle.Width,rectangle.Height,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			//Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height, image.PixelFormat);
			using(Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(image,
								  new System.Drawing.Rectangle(0,0,bitmap.Width,bitmap.Height),
								  rectangle,
								  GraphicsUnit.Pixel);
				graphics.Dispose();
			}
			return bitmap;
		}

		/// <summary>
		/// 将一个UIElement渲染成图片byte[]返回
		/// </summary>
		/// <param name="uiElement"></param>
		/// <param name="doubleScale"></param>
		/// <param name="intQuality"></param>
		/// <returns></returns>
		public static byte[] UIElementToBytes(UIElement uiElement,double doubleScaleWidth,double doubleScaleHeight/*,int intQuality*/)
		{
			double doubleActualWidth = uiElement.RenderSize.Width;
			double doubleActualHeight = uiElement.RenderSize.Height;

			double doubleRenderWidth = doubleActualWidth * doubleScaleWidth;
			double doubleRenderHeight = doubleActualHeight * doubleScaleHeight;

			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)doubleRenderWidth,(int)doubleRenderHeight,96,96,PixelFormats.Pbgra32);
			VisualBrush visualBrush = new VisualBrush(uiElement);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			using(drawingContext)
			{
				drawingContext.PushTransform(new ScaleTransform(doubleScaleWidth,doubleScaleHeight));
				drawingContext.DrawRectangle(visualBrush,null,new Rect(new Point(0,0),new Point(doubleActualWidth,doubleActualHeight)));
			}
			renderTargetBitmap.Render(drawingVisual);
			PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Byte[] bytes;

			using(MemoryStream memoryStream = new MemoryStream())
			{
				pngBitmapEncoder.Save(memoryStream);
				bytes = memoryStream.ToArray();
			}

			return bytes;
		}

		/// <summary>
		/// 将一个FrameworkElement渲染成图片byte[]返回
		/// </summary>
		/// <param name="frameworkElement"></param>
		/// <returns></returns>
		public static byte[] FrameworkElementToBytes(FrameworkElement frameworkElement)
		{
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)frameworkElement.ActualWidth,(int)frameworkElement.ActualHeight,96d,96d,PixelFormats.Pbgra32);
			renderTargetBitmap.Render(frameworkElement);

			PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Byte[] bytes;
			using(MemoryStream memoryStream = new MemoryStream())
			{
				pngBitmapEncoder.Save(memoryStream);
				bytes = memoryStream.ToArray();
			}

			return bytes;
		}

		/// <summary>
		/// BytesToImage
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static Image BytesToImage(byte[] bytes)
		{
			MemoryStream memoryStream = new MemoryStream(bytes);
			Image image = System.Drawing.Image.FromStream(memoryStream);
			return image;
		}

		/// <summary>
		/// BytesToBitmapImage
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static BitmapImage BytesToBitmapImage(Byte[] bytes)
		{
			MemoryStream memoryStream = new MemoryStream(bytes);
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memoryStream;
			bitmapImage.EndInit();
			return bitmapImage;
		}

		/// <summary>
		/// BitmapImageToBytes
		/// </summary>
		/// <param name="bitmapImage"></param>
		/// <returns></returns>
		public static Byte[] BitmapImageToBytes(BitmapImage bitmapImage)
		{
			Stream stream = bitmapImage.StreamSource;
			Byte[] bytes = null;
			if(stream != null && stream.Length > 0)
			{
				using(BinaryReader binaryReader = new BinaryReader(stream))
				{
					bytes = binaryReader.ReadBytes((Int32)stream.Length);
				}
			}
			return bytes;
		}

		/// <summary>
		/// BitmapSourceCopyPixelsToBitmap
		/// </summary>
		/// <param name="bitmapSource"></param>
		/// <returns></returns>
		public static Bitmap BitmapSourceCopyPixelsToBitmap(BitmapSource bitmapSource)
		{
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapSource.PixelWidth,bitmapSource.PixelHeight,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty,bitmap.Size),System.Drawing.Imaging.ImageLockMode.WriteOnly,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			bitmapSource.CopyPixels(Int32Rect.Empty,bitmapData.Scan0,bitmapData.Height * bitmapData.Stride,bitmapData.Stride);
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		/// <summary>
		/// BitmapSourceMemoryStreamToBitmap
		/// </summary>
		/// <param name="bitmapSource"></param>
		/// <returns></returns>
		public static Bitmap BitmapSourceMemoryStreamToBitmap(BitmapSource bitmapSource)
		{
			using(MemoryStream memoryStream = new MemoryStream())
			{
				BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
				bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				bmpBitmapEncoder.Save(memoryStream);
				Bitmap bitmap = new Bitmap(memoryStream);
				return bitmap;
			}
		}
	}
}
