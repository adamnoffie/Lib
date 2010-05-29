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
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// Some extra imaging methods
    /// </summary>
    public static class Imaging
    {
        private static ImageCodecInfo thumbEncoderInfo = null;
        /// <summary>
        /// Gets the EncoderInfo used to encode the Thumbnails
        /// </summary>
        public static ImageCodecInfo JpegCodecInfo
        {
            get
            {
                lock (typeof(Imaging))
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
        /// Gets the ImageCodecInfo class pertaining to a given ImageFormat
        /// </summary>    
        public static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            return encoders.First(e => e.FormatID == format.Guid);
        }


        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {            
            //the codec to return, default to null
            ImageCodecInfo foundCodec = null;
            if (Encoders.ContainsKey(mimeType))
                foundCodec = Encoders[mimeType];
            return foundCodec;
        } 

        private static Dictionary<string, ImageCodecInfo> encoders = null;
        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            get
            {
                lock (typeof(Imaging))
                {
                    if (encoders == null)
                    {
                        encoders = new Dictionary<string, ImageCodecInfo>();
                        foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                            encoders.Add(codec.MimeType.ToLower(), codec);
                    } 
                }
                return encoders;
            }
        }

        /// <summary>
        /// Convert Image to byte[]
        /// </summary>
        public static byte[] ImageToByteArray(Image img)
        {
            byte[] retVal;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                retVal = ms.ToArray();
            }
            return retVal;
        }

        /// <summary>
        /// Scales an image, maintaining aspect ratio, so that both the height and width
        /// of the newly scaled image fit within given dimensions
        /// </summary>
        /// <remarks>Returns reference to original image if no scaling is required</remarks>
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

                // compute the new width / height maintaing aspect ratio to fit into bounding box
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

                return ResizeImage(img, nWidth, nHeight);
            }
            else
            {
                return img;
            }
        }

        /// <summary>
        /// Scales an image, maintaining aspect ratio, so that both the height and width
        /// of the newly scaled image fit within given bounding box
        /// </summary>
        /// <remarks>Returns reference to original image if no scaling is required</remarks>
        /// <param name="img">Image to scale</param>
        /// <param name="size">lenght of the sides of the bounding square to fit
        ///     the scaled image to</param>
        public static Image ScaleImage(Image img, int size)
        {
            return ScaleImage(img, size, size);
        }        

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path">Path to which the image would be saved.</param> 
        /// <param name="quality">An integer from 0 to 100, with 100 being the 
        /// highest quality</param> 
        /// <exception cref="ArgumentOutOfRangeException">
        /// An invalid value was entered for image quality.
        /// </exception>
        public static void SaveJpeg(string path, Image image, int quality)
        {            
            ImageCodecInfo jpegCodec = GetEncoderInfo(ImageFormat.Jpeg);
            EncoderParameters encoderParams = GetJpegEncoderParams(quality);

            image.Save(path, jpegCodec, encoderParams);
        }

        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality and returns the byte[] of that jpeg image
        /// </summary> 
        /// <param name="quality">An integer from 0 to 100, with 100 being the highest quality</param> 
        /// <exception cref="ArgumentOutOfRangeException">
        /// An invalid value was entered for image quality.
        /// </exception>
        public static byte[] SaveJpeg(Image image, int quality)
        {            
            ImageCodecInfo jpegCodec = GetEncoderInfo(ImageFormat.Jpeg);
            EncoderParameters encoderParams = GetJpegEncoderParams(quality);

            byte[] retVal;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                image.Save(ms, jpegCodec, encoderParams);
                retVal = ms.ToArray();
            }
            return retVal;
        }

        /// <summary>
        /// Gets the EncoderParameters structure for encoding a Jpeg image, with a set <paramref name="quality"/>
        /// </summary>
        /// <param name="quality">integer between 0 and 100, 100 being the highest quality</param>
        /// <returns></returns>
        public static EncoderParameters GetJpegEncoderParams(int quality)
        {
            //ensure the quality is within the correct range
            if ((quality < 0) || (quality > 100))
            {
                //create the error message
                string error = string.Format("Jpeg image quality must be between 0 and 100, with 100 being the highest quality.  A value of {0} was specified.", quality);
                throw new ArgumentOutOfRangeException(error);
            }

            // create the EncoderParameters array, and fill it with the quality setting
            EncoderParameters encoderParams = new EncoderParameters(1);
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = qualityParam;

            return encoderParams;
        }
    }    
}