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
namespace crcPdf
{
    public enum PdfExceptionCodes
    {
        FILE_ABRUPTLY_TERMINATED,
        EXPECTED_DELIMITER,
        INVALID_NAMEOBJECT_TOKEN,
        UNKNOWN_TOKEN, 
        INVALID_NUMBER_TOKEN,
        INVALID_INDIRECTOBJECT_TOKEN,
        INVALID_GENERATION,
        HEADER_NOT_FOUND,
        BOF,
        INVALID_EOF,
        INVALID_XREF,
        DICTIONARY_KEY_NAMEOBJECT,
        INVALID_ARRAY,
        INVALID_DICTIONARY,
        DICTIONARY_VALUE_LENGTH_INTEGER,
        INVALID_DICTIONARY_STREAM,
        INVALID_STREAM,
        INVALID_NUMBER,
        INVALID_TRAILER,
        INVALID_CONTENT,
        INVALID_FONT,
        INVALID_RESOURCE,
        UNKNOWN_ENTRY,
        FONT_NOT_FOUND,
        INVALID_COLOR,
        COMPRESSION_NOT_IMPLEMENTED,
        INVALID_FILTER,
        INVALID_OPERATOR,
        FONT_NOT_SUPPORTED,
        FONT_ERROR,
        IMAGE_NOT_FOUND,
        IMAGE_FORMAT_NOT_SUPPORTED,
        IMAGE_BAD_IMAGE,
        CMAP_TODO
    }  
}