// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace crcPdf.Images {
    /// <summary>
    /// Get an image
    /// </summary>
	public static class ImageFactory {

        private static readonly  Dictionary<string, DocumentImage> dct = new Dictionary<string, DocumentImage>();
        private static object lck = new object();

        private static SHA1 sha1 = SHA1CryptoServiceProvider.Create();

        public static DocumentImage GetImage(PDFObjects pdf, PdfObject pdfObject) {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);

            if (dic.Dictionary.ContainsKey("Filter") && pdf.GetObject<NameObject>(dic.Dictionary["Filter"]).Value == "DCTDecode")
                return GetImageInternal(dic.Stream);

            int width = pdf.GetObject<IntegerObject>(dic.Dictionary["Width"]).IntValue;
            int height = pdf.GetObject<IntegerObject>(dic.Dictionary["Height"]).IntValue;
            int bitsPerComponents = pdf.GetObject<IntegerObject>(dic.Dictionary["BitsPerComponent"]).IntValue;
            string colorSpace = pdf.GetObject<NameObject>(dic.Dictionary["ColorSpace"]).Value;

            return GetRawImage(dic.Stream, width, height, bitsPerComponents, Enum.Parse<DeviceColorSpace>(colorSpace));
        }

        public static DocumentImage GetImage(string fullFilePath) {
            if (!File.Exists(fullFilePath)) { 
                throw new PdfException(PdfExceptionCodes.IMAGE_NOT_FOUND, $"File {fullFilePath} doesnt exists");
            }

            byte[] image = File.ReadAllBytes(fullFilePath);

            return GetImageInternal(image);
        }

        public static DocumentImage GetImage(byte[] image) 
            => GetImageInternal(image);

        public static DocumentImage GetRawImage(byte[] image, int widht, int height, int bitsPerComponent, DeviceColorSpace colorSpace) 
        {
            byte[] hashBytes = sha1.ComputeHash(image);
            string hash = string.Concat(hashBytes.Select(b => b.ToString("x2")));

            lock (lck) {
                if (dct.ContainsKey(hash)) {
                    return dct[hash];
                }

                var img = new DocumentImageRaw(image, widht, height, bitsPerComponent, colorSpace);
                dct.Add(hash, img);
                return img;
            }

        }

        private static DocumentImage GetImageInternal(byte[] image) {
            byte[] hashBytes = sha1.ComputeHash(image);
            string hash = string.Concat(hashBytes.Select(b => b.ToString("x2")));

            lock (lck) {
                if (dct.ContainsKey(hash)) {
                    return dct[hash];
                }

                DocumentImage img;
                // https://en.wikipedia.org/wiki/JPEG_File_Interchange_Format
                // SOI FF D8 Start of Image
                if (image[0] == 0xFF && image[1] == 0xD8) {     
                    img = new DocumentImageJpeg(image);
                } else {
                    throw new PdfException(PdfExceptionCodes.IMAGE_FORMAT_NOT_SUPPORTED, "Image format not supported");
                }
                
                dct.Add(hash, img);
                return img;
            }
        }
    }
}