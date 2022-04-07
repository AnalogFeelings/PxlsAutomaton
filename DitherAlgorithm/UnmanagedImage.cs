// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2005-2012
// Copyright AestheticalZ, 2022
// contacts@aforgenet.com
//

//CHANGES FROM ORIGINAL:
//Made it work under .NET 6.0.
//Trimmed off some large unnecessary functions.

namespace AForge.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Collections.Generic;

    /// <summary>
    /// Image in unmanaged memory.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>The class represents wrapper of an image in unmanaged memory. Using this class
    /// it is possible as to allocate new image in unmanaged memory, as to just wrap provided
    /// pointer to unmanaged memory, where an image is stored.</para>
    /// 
    /// <para>Usage of unmanaged images is mostly beneficial when it is required to apply <b>multiple</b>
    /// image processing routines to a single image. In such scenario usage of .NET managed images 
    /// usually leads to worse performance, because each routine needs to lock managed image
    /// before image processing is done and then unlock it after image processing is done. Without
    /// these lock/unlock there is no way to get direct access to managed image's data, which means
    /// there is no way to do fast image processing. So, usage of managed images lead to overhead, which
    /// is caused by locks/unlock. Unmanaged images are represented internally using unmanaged memory
    /// buffer. This means that it is not required to do any locks/unlocks in order to get access to image
    /// data (no overhead).</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // sample 1 - wrapping .NET image into unmanaged without
    /// // making extra copy of image in memory
    /// BitmapData imageData = image.LockBits(
    ///     new Rectangle( 0, 0, image.Width, image.Height ),
    ///     ImageLockMode.ReadWrite, image.PixelFormat );
    /// 
    /// try
    /// {
    ///     UnmanagedImage unmanagedImage = new UnmanagedImage( imageData ) );
    ///     // apply several routines to the unmanaged image
    /// }
    /// finally
    /// {
    ///     image.UnlockBits( imageData );
    /// }
    /// 
    /// 
    /// // sample 2 - converting .NET image into unmanaged
    /// UnmanagedImage unmanagedImage = UnmanagedImage.FromManagedImage( image );
    /// // apply several routines to the unmanaged image
    /// ...
    /// // conver to managed image if it is required to display it at some point of time
    /// Bitmap managedImage = unmanagedImage.ToManagedImage( );
    /// </code>
    /// </remarks>
    /// 
    public class UnmanagedImage : IDisposable
    {
        // pointer to image data in unmanaged memory
        private IntPtr imageData;
        // image size
        private int width, height;
        // image stride (line size)
        private int stride;
        // image pixel format
        private PixelFormat pixelFormat;
        // flag which indicates if the image should be disposed or not
        private bool mustBeDisposed = false;

        /// <summary>
        /// Pointer to image data in unmanaged memory.
        /// </summary>
        public IntPtr ImageData
        {
            get { return imageData; }
        }

        /// <summary>
        /// Image width in pixels.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Image stride (line size in bytes).
        /// </summary>
        public int Stride
        {
            get { return stride; }
        }

        /// <summary>
        /// Image pixel format.
        /// </summary>
        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedImage"/> class.
        /// </summary>
        /// 
        /// <param name="imageData">Pointer to image data in unmanaged memory.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="stride">Image stride (line size in bytes).</param>
        /// <param name="pixelFormat">Image pixel format.</param>
        /// 
        /// <remarks><para><note>Using this constructor, make sure all specified image attributes are correct
        /// and correspond to unmanaged memory buffer. If some attributes are specified incorrectly,
        /// this may lead to exceptions working with the unmanaged memory.</note></para></remarks>
        /// 
        public UnmanagedImage(IntPtr imageData, int width, int height, int stride, PixelFormat pixelFormat)
        {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.pixelFormat = pixelFormat;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedImage"/> class.
        /// </summary>
        /// 
        /// <param name="bitmapData">Locked bitmap data.</param>
        /// 
        /// <remarks><note>Unlike <see cref="FromManagedImage(BitmapData)"/> method, this constructor does not make
        /// copy of managed image. This means that managed image must stay locked for the time of using the instance
        /// of unamanged image.</note></remarks>
        /// 
        public UnmanagedImage(BitmapData bitmapData)
        {
            this.imageData = bitmapData.Scan0;
            this.width = bitmapData.Width;
            this.height = bitmapData.Height;
            this.stride = bitmapData.Stride;
            this.pixelFormat = bitmapData.PixelFormat;
        }

        /// <summary>
        /// Destroys the instance of the <see cref="UnmanagedImage"/> class.
        /// </summary>
        /// 
        ~UnmanagedImage()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <remarks><para>Frees unmanaged resources used by the object. The object becomes unusable
        /// after that.</para>
        /// 
        /// <par><note>The method needs to be called only in the case if unmanaged image was allocated
        /// using <see cref="Create"/> method. In the case if the class instance was created using constructor,
        /// this method does not free unmanaged memory.</note></par>
        /// </remarks>
        /// 
        public void Dispose()
        {
            Dispose(true);
            // remove me from the Finalization queue 
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <param name="disposing">Indicates if disposing was initiated manually.</param>
        /// 
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free image memory if the image was allocated using this class
            if ((mustBeDisposed) && (imageData != IntPtr.Zero))
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(imageData);
                System.GC.RemoveMemoryPressure(stride * height);
                imageData = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Clone the unmanaged images.
        /// </summary>
        /// 
        /// <returns>Returns clone of the unmanaged image.</returns>
        /// 
        /// <remarks><para>The method does complete cloning of the object.</para></remarks>
        /// 
        public UnmanagedImage Clone()
        {
            // allocate memory for the image
            IntPtr newImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);
            System.GC.AddMemoryPressure(stride * height);

            UnmanagedImage newImage = new UnmanagedImage(newImageData, width, height, stride, pixelFormat);
            newImage.mustBeDisposed = true;

            AForge.SystemTools.CopyUnmanagedMemory(newImageData, imageData, stride * height);

            return newImage;
        }

        /// <summary>
        /// Create unmanaged image from the specified managed image.
        /// </summary>
        /// 
        /// <param name="image">Source managed image.</param>
        /// 
        /// <returns>Returns new unmanaged image, which is a copy of source managed image.</returns>
        /// 
        /// <remarks><para>The method creates an exact copy of specified managed image, but allocated
        /// in unmanaged memory.</para></remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of source image.</exception>
        /// 
        public static UnmanagedImage FromManagedImage(Bitmap image)
        {
            UnmanagedImage dstImage = null;

            BitmapData sourceData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                dstImage = FromManagedImage(sourceData);
            }
            finally
            {
                image.UnlockBits(sourceData);
            }

            return dstImage;
        }

        /// <summary>
        /// Create unmanaged image from the specified managed image.
        /// </summary>
        /// 
        /// <param name="imageData">Source locked image data.</param>
        /// 
        /// <returns>Returns new unmanaged image, which is a copy of source managed image.</returns>
        /// 
        /// <remarks><para>The method creates an exact copy of specified managed image, but allocated
        /// in unmanaged memory. This means that managed image may be unlocked right after call to this
        /// method.</para></remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of source image.</exception>
        /// 
        public static UnmanagedImage FromManagedImage(BitmapData imageData)
        {
            PixelFormat pixelFormat = imageData.PixelFormat;

            // check source pixel format
            if (
                (pixelFormat != PixelFormat.Format8bppIndexed) &&
                (pixelFormat != PixelFormat.Format16bppGrayScale) &&
                (pixelFormat != PixelFormat.Format24bppRgb) &&
                (pixelFormat != PixelFormat.Format32bppRgb) &&
                (pixelFormat != PixelFormat.Format32bppArgb) &&
                (pixelFormat != PixelFormat.Format32bppPArgb) &&
                (pixelFormat != PixelFormat.Format48bppRgb) &&
                (pixelFormat != PixelFormat.Format64bppArgb) &&
                (pixelFormat != PixelFormat.Format64bppPArgb))
            {
                throw new Exception("Unsupported pixel format of the source image.");
            }

            // allocate memory for the image
            IntPtr dstImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(imageData.Stride * imageData.Height);
            System.GC.AddMemoryPressure(imageData.Stride * imageData.Height);

            UnmanagedImage image = new UnmanagedImage(dstImageData, imageData.Width, imageData.Height, imageData.Stride, pixelFormat);
            AForge.SystemTools.CopyUnmanagedMemory(dstImageData, imageData.Scan0, imageData.Stride * imageData.Height);
            image.mustBeDisposed = true;

            return image;
        }

        /// <summary>
        /// Collect coordinates of none black pixels in the image.
        /// </summary>
        /// 
        /// <returns>Returns list of points, which have other than black color.</returns>
        /// 
        public List<IntPoint> CollectActivePixels()
        {
            return CollectActivePixels(new Rectangle(0, 0, width, height));
        }

        /// <summary>
        /// Collect coordinates of none black pixels within specified rectangle of the image.
        /// </summary>
        /// 
        /// <param name="rect">Image's rectangle to process.</param>
        /// 
        /// <returns>Returns list of points, which have other than black color.</returns>
        ///
        public List<IntPoint> CollectActivePixels(Rectangle rect)
        {
            List<IntPoint> pixels = new List<IntPoint>();

            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;

            // correct rectangle
            rect.Intersect(new Rectangle(0, 0, width, height));

            int startX = rect.X;
            int startY = rect.Y;
            int stopX = rect.Right;
            int stopY = rect.Bottom;

            unsafe
            {
                byte* basePtr = (byte*)imageData.ToPointer();

                if ((pixelFormat == PixelFormat.Format16bppGrayScale) || (pixelSize > 4))
                {
                    int pixelWords = pixelSize >> 1;

                    for (int y = startY; y < stopY; y++)
                    {
                        ushort* ptr = (ushort*)(basePtr + y * stride + startX * pixelSize);

                        if (pixelWords == 1)
                        {
                            // grayscale images
                            for (int x = startX; x < stopX; x++, ptr++)
                            {
                                if (*ptr != 0)
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                        else
                        {
                            // color images
                            for (int x = startX; x < stopX; x++, ptr += pixelWords)
                            {
                                if ((ptr[RGB.R] != 0) || (ptr[RGB.G] != 0) || (ptr[RGB.B] != 0))
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int y = startY; y < stopY; y++)
                    {
                        byte* ptr = basePtr + y * stride + startX * pixelSize;

                        if (pixelSize == 1)
                        {
                            // grayscale images
                            for (int x = startX; x < stopX; x++, ptr++)
                            {
                                if (*ptr != 0)
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                        else
                        {
                            // color images
                            for (int x = startX; x < stopX; x++, ptr += pixelSize)
                            {
                                if ((ptr[RGB.R] != 0) || (ptr[RGB.G] != 0) || (ptr[RGB.B] != 0))
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                }
            }

            return pixels;
        }

        /// <summary>
        /// Set pixel with the specified coordinates to the specified color.
        /// </summary>
        /// 
        /// <param name="x">X coordinate of the pixel to set.</param>
        /// <param name="y">Y coordinate of the pixel to set.</param>
        /// <param name="color">Color to set for the pixel.</param>
        /// 
        /// <remarks><para><note>For images having 16 bpp per color plane, the method extends the specified color
        /// value to 16 bit by multiplying it by 256.</note></para>
        /// 
        /// <para>For grayscale images this method will calculate intensity value based on the below formula:
        /// <code lang="none">
        /// 0.2125 * Red + 0.7154 * Green + 0.0721 * Blue
        /// </code>
        /// </para>
        /// </remarks>
        /// 
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Set pixel with the specified coordinates to the specified value.
        /// </summary>
        ///
        /// <param name="x">X coordinate of the pixel to set.</param>
        /// <param name="y">Y coordinate of the pixel to set.</param>
        /// <param name="value">Pixel value to set.</param>
        /// 
        /// <remarks><para>The method sets all color components of the pixel to the specified value.
        /// If it is a grayscale image, then pixel's intensity is set to the specified value.
        /// If it is a color image, then pixel's R/G/B components are set to the same specified value
        /// (if an image has alpha channel, then it is set to maximum value - 255 or 65535).</para>
        /// 
        /// <para><note>For images having 16 bpp per color plane, the method extends the specified color
        /// value to 16 bit by multiplying it by 256.</note></para>
        /// </remarks>
        /// 
        public void SetPixel(int x, int y, byte value)
        {
            SetPixel(x, y, value, value, value, 255);
        }

        private void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            if ((x >= 0) && (y >= 0) && (x < width) && (y < height))
            {
                unsafe
                {
                    int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                    byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;
                    ushort* ptr2 = (ushort*)ptr;

                    switch (pixelFormat)
                    {
                        case PixelFormat.Format8bppIndexed:
                            *ptr = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
                            break;

                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                            ptr[RGB.R] = r;
                            ptr[RGB.G] = g;
                            ptr[RGB.B] = b;
                            break;

                        case PixelFormat.Format32bppArgb:
                            ptr[RGB.R] = r;
                            ptr[RGB.G] = g;
                            ptr[RGB.B] = b;
                            ptr[RGB.A] = a;
                            break;

                        case PixelFormat.Format16bppGrayScale:
                            *ptr2 = (ushort)((ushort)(0.2125 * r + 0.7154 * g + 0.0721 * b) << 8);
                            break;

                        case PixelFormat.Format48bppRgb:
                            ptr2[RGB.R] = (ushort)(r << 8);
                            ptr2[RGB.G] = (ushort)(g << 8);
                            ptr2[RGB.B] = (ushort)(b << 8);
                            break;

                        case PixelFormat.Format64bppArgb:
                            ptr2[RGB.R] = (ushort)(r << 8);
                            ptr2[RGB.G] = (ushort)(g << 8);
                            ptr2[RGB.B] = (ushort)(b << 8);
                            ptr2[RGB.A] = (ushort)(a << 8);
                            break;

                        default:
                            throw new Exception("The pixel format is not supported: " + pixelFormat);
                    }
                }
            }
        }

        /// <summary>
        /// Get color of the pixel with the specified coordinates.
        /// </summary>
        /// 
        /// <param name="point">Point's coordiates to get color of.</param>
        /// 
        /// <returns>Return pixel's color at the specified coordinates.</returns>
        /// 
        /// <remarks><para>See <see cref="GetPixel(int, int)"/> for more information.</para></remarks>
        ///
        public Color GetPixel(IntPoint point)
        {
            return GetPixel(point.X, point.Y);
        }

        /// <summary>
        /// Get color of the pixel with the specified coordinates.
        /// </summary>
        /// 
        /// <param name="x">X coordinate of the pixel to get.</param>
        /// <param name="y">Y coordinate of the pixel to get.</param>
        /// 
        /// <returns>Return pixel's color at the specified coordinates.</returns>
        /// 
        /// <remarks>
        /// <para><note>In the case if the image has 8 bpp grayscale format, the method will return a color with
        /// all R/G/B components set to same value, which is grayscale intensity.</note></para>
        /// 
        /// <para><note>The method supports only 8 bpp grayscale images and 24/32 bpp color images so far.</note></para>
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">The specified pixel coordinate is out of image's bounds.</exception>
        /// <exception cref="UnsupportedImageFormatException">Pixel format of this image is not supported by the method.</exception>
        /// 
        public Color GetPixel(int x, int y)
        {
            if ((x < 0) || (y < 0))
            {
                throw new ArgumentOutOfRangeException("x", "The specified pixel coordinate is out of image's bounds.");
            }

            if ((x >= width) || (y >= height))
            {
                throw new ArgumentOutOfRangeException("y", "The specified pixel coordinate is out of image's bounds.");
            }

            Color color = new Color();

            unsafe
            {
                int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;

                switch (pixelFormat)
                {
                    case PixelFormat.Format8bppIndexed:
                        color = Color.FromArgb(*ptr, *ptr, *ptr);
                        break;

                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        color = Color.FromArgb(ptr[RGB.R], ptr[RGB.G], ptr[RGB.B]);
                        break;

                    case PixelFormat.Format32bppArgb:
                        color = Color.FromArgb(ptr[RGB.A], ptr[RGB.R], ptr[RGB.G], ptr[RGB.B]);
                        break;

                    default:
                        throw new Exception("The pixel format is not supported: " + pixelFormat);
                }
            }

            return color;
        }
    }
}