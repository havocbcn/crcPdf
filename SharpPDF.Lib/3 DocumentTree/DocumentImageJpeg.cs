// This file is part of SharpReport.
// 
// SharpReport is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpReport is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpReport.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class DocumentImageJpeg : DocumentImage {
        private readonly byte[] m_image;
        private readonly int m_width = -1;
        private readonly int m_height = -1;
        private readonly int m_bitsPerComponent = -1;    // 8 bits
        private readonly int m_component = -1;           // 1 => black&white, 3 => rgb

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="image">Image byte array</param>
        public DocumentImageJpeg(PDFObjects pdf,  byte[] image): base(pdf) {
            m_image = image;
      
            // https://en.wikipedia.org/wiki/JPEG#Syntax_and_structure
            // jpeg is a catalog made of blocks marked as FF XX and depending
            // which XX a size block
            //
            // SOI   0xFF, 0xD8  none           Start Of Image  
            // SOF0  0xFF, 0xC0  variable size  Start Of Frame (baseline DCT) Indicates that this is
            //                                  a baseline DCT-based JPEG, and specifies the width, 
            //                                  height, number of components, and component 
            //                                  subsampling (e.g., 4:2:0).
            // SOF2  0xFF, 0xC2  variable size  Start Of Frame (progressive DCT) Indicates that this
            //                                  is a progressive DCT-based JPEG, and specifies the 
            //                                  width, height, number of components, and component 
            //                                  subsampling (e.g., 4:2:0).
            // DHT   0xFF, 0xC4  variable size  Define Huffman Table(s) Specifies one or more 
            //                                  Huffman tables.
            // DQT   0xFF, 0xDB  variable size  Define Quantization Table(s) Specifies one or more 
            //                                  quantization tables.
            // DRI   0xFF, 0xDD  4 bytes        Define Restart Interval Specifies the interval 
            //                                  between RSTn markers, in Minimum Coded Units (MCUs).
            //                                  This marker is followed by two bytes indicating the
            //                                  fixed size so it can be treated like any other 
            //                                  variable size segment.
            // SOS   0xFF, 0xDA  variable size  Start Of Scan   Begins a top-to-bottom scan of the
            //                                  image. In baseline DCT JPEG images, there is 
            //                                  generally a single scan. Progressive DCT JPEG images
            //                                  usually contain multiple scans. This marker 
            //                                  specifies which slice of data it will contain, and 
            //                                  is immediately followed by entropy-coded data.
            // RSTn  0xFF, 0xDn (n=0..7) none   Restart Inserted every r macroblocks, where r is the
            //                                  restart interval set by a DRI marker. Not used if
            //                                  there was no DRI marker. The low three bits of the
            //                                  marker code cycle in value from 0 to 7.
            // APPn  0xFF, 0xEn  variable size  Application-specific. For example, an Exif JPEG file
            //                                  uses an APP1 marker to store metadata, laid out in a
            //                                  structure based closely on TIFF.
            // COM   0xFF, 0xFE  variable size  Comment Contains a text comment.
            // EOI   0xFF, 0xD9  none           End Of Image    
            //
            // Example:
            // -------
            // 
            // FF D8 <- first block
            // FF C2 dd ee <- another block
            // ...
            // FF C0 ff gg H1 H2 W1 W2 <- H1, H2 are height, W1, W2 width
            // ...
            //
            // where: dd, ee, ff, gg are 16bits size in bytes of the block excluding the block mark
            int filePosition = 0;

            if (filePosition > m_image.Length - 10) {
                throw new PdfException(PdfExceptionCodes.IMAGE_BAD_IMAGE, $"Cannot find jpeg size");
            }

            if (m_image[filePosition] != 0xFF) {
                throw new PdfException(PdfExceptionCodes.IMAGE_BAD_IMAGE, $"Bad JPEG image");
            }
            
            while (m_image[filePosition + 1] != 0xC0 &&
                    m_image[filePosition + 1] != 0xC2 ) {
                if (m_image[filePosition + 1] >= 0xD0 && m_image[filePosition + 1] <= 0xD9) {
                    filePosition += 2;  // the 2 bytes block definition
                } else if (m_image[filePosition+1] == 0xDD) {
                    filePosition += 2;  // the 2 bytes block definition
                    filePosition += 4;  
                } else {
                    filePosition += m_image[filePosition+2] << 8 | m_image[filePosition+3];
                    filePosition += 2;  // the 2 bytes block definition
                }

                if (filePosition > m_image.Length - 10) {
                    throw new PdfException(PdfExceptionCodes.IMAGE_BAD_IMAGE, $"Cannot find jpeg size");
                }

                if (m_image[filePosition] != 0xFF) {
                    throw new PdfException(PdfExceptionCodes.IMAGE_BAD_IMAGE, $"Bad JPEG image");
                }
            }

            m_bitsPerComponent = m_image[filePosition + 4];
            m_height = (ushort) (m_image[filePosition + 5] << 8 | m_image[filePosition + 6]);
            m_width = (ushort) (m_image[filePosition + 7] << 8 | m_image[filePosition + 8]);
            m_component = m_image[filePosition + 9];
        }

        /// <summary>
        /// Get the image byte array
        /// </summary>
        /// <returns>The image.</returns>
        public byte[] Image() {
            return m_image;
        }

        /// <summary>
        /// Image height
        /// </summary>
        /// <returns>The height.</returns>
        public override int Height => m_height;

        /// <summary>
        /// Image width
        /// </summary>
        /// <returns>The width.</returns>
        public override int Width => m_width;

        /// <summary>
        /// Bits the per component.
        /// </summary>
        /// <returns>1 = black&amp;white, 8, 16, 25...</returns>
        public int BitsPerComponent => m_bitsPerComponent;

        /// <summary>
        /// How much components
        /// </summary>
        /// <returns>1 = black&amp;white, 3=RGB</returns>
        public int Components => m_component;

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("XObject") },
                { "Subtype", new NameObject("Image") },
                { "Filter", new NameObject("DCTDecode") },
                { "Length", new IntegerObject(m_image.Length) },
                { "Width", new IntegerObject(this.Width) },
                { "Height", new IntegerObject(this.Height) },
                { "BitsPerComponent", new IntegerObject(this.BitsPerComponent) },
                { "ColorSpace", (this.Components == 3 ? new NameObject("DeviceRGB") : new NameObject("DeviceGray")) }
            };   
   
            indirectObject.SetChild(new DictionaryObject(entries, m_image));
        }
    }
}
