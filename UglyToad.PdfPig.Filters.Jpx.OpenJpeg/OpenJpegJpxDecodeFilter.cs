using OpenJpeg;
using System.Buffers.Binary;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Tokens;

// Based on https://github.com/notBald/OpenJPEG.net README

namespace UglyToad.PdfPig.Filters.Jpx.OpenJpeg
{
    /// <summary>
    /// JPX Filter for image data.
    /// <para>
    /// Based on <see href="https://github.com/notBald/OpenJPEG.net"/>.
    /// </para>
    /// </summary>
    public sealed class OpenJpegJpxDecodeFilter : IFilter
    {
        /// <inheritdoc/>
        public bool IsSupported => true;

        /// <inheritdoc/>
        public Memory<byte> Decode(Memory<byte> input, DictionaryToken streamDictionary, IFilterProvider filterProvider, int filterIndex)
        {
            var codecFormat = GetCodecFormat(input.Span);

            //OpenJpeg.Net uses the OpenJpeg 1.4 API
            var cInfo = new CompressionInfo(true, codecFormat);

            //Sets up decoding parameters. Can for instance be used to
            //speed up decoding of thumbnails by decoding less resolutions
            var parameters = new DecompressionParameters();

            //Destination for the decoded image
            JPXImage? img = null;

            using (var ms = MemoryHelper.AsReadOnlyMemoryStream(input))
            {
                //cio is a wrapper that is used by the libary when
                //reading. A bit like "BinaryReader"
                var cio = cInfo.OpenCIO(ms, true);
                cInfo.SetupDecoder(cio, parameters);

                //Decodes the image
                if (!cInfo.ReadHeader(out img) || !cInfo.Decode(img) || !cInfo.EndDecompress())
                {
                    throw new Exception("Failed to get JPX compression information.");
                }

                //If there's an error, you won't get an image. To get the error message,
                //set up cinfo with message callback functions
                if (img is null)
                {
                    throw new NullReferenceException("Something wrong happened while getting JPX image.");
                }
            }

            //Makes the bits per channel uniform so that it's easier
            //to work with.
            img.MakeUniformBPC();

            //Jpeg 2000 images can have a color palette, this removes
            //that.
            img.ApplyIndex();

            //Handle some color spaces. Note, img.CMYKtoRGB() implements the
            //reference conversion algorithm. You should propbably use something
            //better. See CMYKFilter in PdfLib for ways to do this.
            switch (img.ColorSpace)
            {
                case COLOR_SPACE.YCCK:
                    if (!img.ESyccToRGB())
                    {
                        throw new Exception("Failed to RGB convert image");
                    }
                    break;
                case COLOR_SPACE.CMYK:
                    if (!img.CMYKtoRGB())
                    {
                        throw new Exception("Failed to RGB convert image");
                    }
                    break;
            }
            //Note, we don't here handle grayscale or CMY format. 

            //Assembles the image into a stream of bytes
            using (var ms = img.ToMemoryStream())
            {
                return ms.AsMemory();
            }
        }

        /// <summary>
        /// Get bits per component values for Jp2 (Jpx) encoded images (first component).
        /// </summary>
        private static CodecFormat GetCodecFormat(ReadOnlySpan<byte> jp2Bytes)
        {
            // Ensure the input has at least 12 bytes for the signature box
            if (jp2Bytes.Length < 12)
            {
                throw new InvalidOperationException("Input is too short to be a valid JPEG2000 file.");
            }

            // Verify the JP2 signature box
            uint length = BinaryPrimitives.ReadUInt32BigEndian(jp2Bytes.Slice(0, 4));
            if (length == 0xFF4FFF51)
            {
                // J2K format detected (SOC marker) (See GHOSTSCRIPT-688999-2.pdf)
                return CodecFormat.Jpeg2K;
            }

            uint type = BinaryPrimitives.ReadUInt32BigEndian(jp2Bytes.Slice(4, 4));
            uint magic = BinaryPrimitives.ReadUInt32BigEndian(jp2Bytes.Slice(8, 4));
            if (length == 0x0000000C && type == 0x6A502020 && magic == 0x0D0A870A)
            {
                // JP2 format detected
                return CodecFormat.Jpeg2P;
            }

            /*
            if (length == 0x0000000C && type == 0x6A502058 && magic == 0x0D0A870A)
            {
                // JPX format detected
                return CodecFormat.Jpx;
            }
            */

            throw new InvalidOperationException("Invalid JP2 or J2K signature.");
        }
    }
}
