using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jpx.OpenJpeg.Tests
{
    internal sealed class JpxFilterProvider : BaseFilterProvider
    {
        /// <summary>
        /// The single instance of this provider.
        /// </summary>
        public static readonly IFilterProvider Instance = new JpxFilterProvider();

        /// <inheritdoc/>
        private JpxFilterProvider() : base(GetDictionary())
        {
        }

        private static Dictionary<string, IFilter> GetDictionary()
        {
            var ascii85 = new Ascii85Filter();
            var asciiHex = new AsciiHexDecodeFilter();
            var ccitt = new CcittFaxDecodeFilter();
            var dct = new DctDecodeFilter();
            var flate = new FlateFilter();
            var jbig2 = new Jbig2DecodeFilter();
            var jpx = new OpenJpegJpxDecodeFilter(); // new
            var runLength = new RunLengthFilter();
            var lzw = new LzwFilter();

            return new Dictionary<string, IFilter>
                {
                    { NameToken.Ascii85Decode.Data, ascii85 },
                    { NameToken.Ascii85DecodeAbbreviation.Data, ascii85 },
                    { NameToken.AsciiHexDecode.Data, asciiHex },
                    { NameToken.AsciiHexDecodeAbbreviation.Data, asciiHex },
                    { NameToken.CcittfaxDecode.Data, ccitt },
                    { NameToken.CcittfaxDecodeAbbreviation.Data, ccitt },
                    { NameToken.DctDecode.Data, dct },
                    { NameToken.DctDecodeAbbreviation.Data, dct },
                    { NameToken.FlateDecode.Data, flate },
                    { NameToken.FlateDecodeAbbreviation.Data, flate },
                    { NameToken.Jbig2Decode.Data, jbig2 },
                    { NameToken.JpxDecode.Data, jpx },
                    { NameToken.RunLengthDecode.Data, runLength },
                    { NameToken.RunLengthDecodeAbbreviation.Data, runLength },
                    { NameToken.LzwDecode.Data, lzw },
                    { NameToken.LzwDecodeAbbreviation.Data, lzw }
                };
        }
    }
}
