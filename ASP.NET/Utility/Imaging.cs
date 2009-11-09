// ----------------------------------------------------------------------------
// Imaging.cs
// Copyright (c) 2009 Adam Nofsinger <adam.nofsinger@gmail.com>
//
// Permission to use, copy, modify, and distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
// ----------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Utility
{
    /// <summary>
    /// Some extra imaging methods
    /// </summary>
    public static class Imaging
    {
        private static Object iLock = new Object();
        private static ImageCodecInfo thumbEncoderInfo = null;

        /// <summary>
        /// Gets the EncoderInfo used to encode the Thumbnails
        /// </summary>
        public static ImageCodecInfo JpegCodecInfo
        {
            get
            {
                lock (iLock)
                {
                    if (thumbEncoderInfo == null)
                    {
                        thumbEncoderInfo = Imaging.GetEncoderInfo(ImageFormat.Jpeg);
                    }
                }
                return thumbEncoderInfo;
            }
        }

        /// <summary>
        /// Scales an image, maintaining aspect ratio, so that both the height and width
        /// of the newly scaled image fit within given dimensions
        /// </summary>
        /// <param name="img">Image to scale</param>
        /// <param name="maxWidth">The maximum width new image will have</param>
        /// <param name="maxHeight">The maximum height new image will have</param>    
        public static Image ScaleImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Width > maxWidth || img.Height > maxHeight)
            {
                double ratio = img.Width / (double)img.Height;  // original aspect ratio
                int nWidth = img.Width;
                int nHeight = img.Height;

                if (nWidth > maxWidth)
                {
                    nWidth = maxWidth;
                    nHeight = (int)(maxWidth / ratio);
                }
                if (nHeight > maxHeight)
                {
                    nHeight = maxHeight;
                    nWidth = (int)(maxHeight * ratio);
                }
                return new Bitmap(img, nWidth, nHeight);
            }
            else
            {
                return new Bitmap(img); // no adjustment needed
            }
        }

        /// <summary>
        /// Scales an image, maintaining aspect ratio, so that both the height and width
        /// of the newly scaled image fit within given bounding box
        /// </summary>
        /// <param name="img">Image to scale</param>
        /// <param name="size">lenght of the sides of the bounding square to fit
        ///     the scaled image to</param>
        public static Image ScaleImage(Image img, int size)
        {
            return ScaleImage(img, size, size);
        }

        /// <summary>
        /// Gets the ImageCodecInfo class pertaining to a given ImageFormat
        /// </summary>    
        public static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            return encoders.First(e => e.FormatID == format.Guid);
        }

    }
    
}