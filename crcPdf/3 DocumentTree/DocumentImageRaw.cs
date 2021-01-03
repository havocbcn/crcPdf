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
using System.Collections.Generic;
using crcPdf.Images;

namespace crcPdf
{
    internal class DocumentImageRaw : DocumentImage {    
        private byte[] image;
        private int width;
        private int height;
        private int bitsPerComponent;
        private DeviceColorSpace colorSpace;

        public DocumentImageRaw(byte[] image, int width, int height, int bitsPerComponent, DeviceColorSpace colorSpace) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.bitsPerComponent = bitsPerComponent;
            this.colorSpace = colorSpace;
        }


        /// <summary>
        /// Get the image byte array
        /// </summary>
        /// <returns>The image.</returns>
        public byte[] Image() => image;

        /// <summary>
        /// Image height
        /// </summary>
        /// <returns>The height.</returns>
        public override int Height => height;

        /// <summary>
        /// Image width
        /// </summary>
        /// <returns>The width.</returns>
        public override int Width => width;

        /// <summary>
        /// Bits the per component.
        /// </summary>
        /// <returns>1 = black&amp;white, 8, 16, 24...</returns>
        public override int BitsPerComponent => bitsPerComponent;

        /// <summary>
        /// How much components
        /// </summary>
        /// <returns>1 = black&amp;white, 3=RGB</returns>
        public override int Components { 
            get { 
                switch(colorSpace) {
                    case DeviceColorSpace.DeviceRGB:
                        return 3;            
                    default:
                        throw new PdfException(PdfExceptionCodes.IMAGE_FORMAT_NOT_SUPPORTED, "Colorspace not implemented");
                }
            }
         }

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects)
        {
            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("XObject") },
                { "Subtype", new NameObject("Image") },
                { "Length", new IntegerObject(image.Length) },
                { "Width", new IntegerObject(this.Width) },
                { "Height", new IntegerObject(this.Height) },
                { "BitsPerComponent", new IntegerObject(this.BitsPerComponent) },
                { "ColorSpace", (this.Components == 3 ? new NameObject("DeviceRGB") : new NameObject("DeviceGray")) }
            };   
   
            indirectObject.SetChild(new DictionaryObject(entries, image));
        }
    }
}