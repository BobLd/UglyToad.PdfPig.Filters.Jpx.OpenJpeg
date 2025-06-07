using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jpx.OpenJpeg.Tests
{
    public class SpecificTest
    {
        private static readonly string _path = "Documents";

        private static readonly ParsingOptions _parsingOption = new ParsingOptions()
        {
            UseLenientParsing = true,
            SkipMissingFonts = true,
            FilterProvider = JpxFilterProvider.Instance
        };

        [Fact]
        public void FoxboxSrHdmiTest1()
        {
            using (var doc = PdfDocument.Open(Path.Combine(_path, "68-1990-01_A.pdf"), _parsingOption))
            {
                int i = 0;
                foreach (var page in doc.GetPages())
                {
                    foreach (var pdfImage in page.GetImages())
                    {
                        if (pdfImage.ImageDictionary.TryGet(NameToken.Filter, out NameToken filter))
                        {
                            if (!filter.Data.ToUpper().Contains("JPX"))
                            {
                                continue;
                            }
                        }

                        Assert.True(pdfImage.TryGetPng(out var bytes));

                        File.WriteAllBytes($"image_{i++}.png", bytes);
                    }
                }
            }
        }
    }
}
