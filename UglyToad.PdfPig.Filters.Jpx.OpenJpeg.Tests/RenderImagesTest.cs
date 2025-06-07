using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jpx.OpenJpeg.Tests
{
    public class RenderImagesTest
    {
        private const string _path = "Documents";
        private const string _outputPath = "Output";

        private static readonly ParsingOptions _parsingOption = new ParsingOptions()
        {
            UseLenientParsing = true,
            SkipMissingFonts = true,
            FilterProvider = JpxFilterProvider.Instance
        };

        public static IEnumerable<object[]> GetAllDocuments => Directory.EnumerateFiles(_path, "*.pdf")
            .Select(p => new object[] { p });

        public RenderImagesTest()
        {
            Directory.CreateDirectory(_outputPath);
        }

        [Theory]
        [MemberData(nameof(GetAllDocuments))]
        public void CanRenderAllJpxImages(string docPath)
        {
            string fileRootName = Path.ChangeExtension(Path.GetFileName(docPath), "");

            bool isDocumentJpx = false;

            using (var doc = PdfDocument.Open(docPath, _parsingOption))
            {
                int i = 0;
                foreach (var page in doc.GetPages())
                {
                    foreach (var pdfImage in page.GetImages())
                    {
                        bool isJpx = false;

                        if (!pdfImage.ImageDictionary.ContainsKey(NameToken.Filter))
                        {
                            continue;
                        }

                        if (pdfImage.ImageDictionary.TryGet(NameToken.Filter, out NameToken filter))
                        {
                            isJpx = filter.Data.ToUpper().Contains("JPX");
                            if (isJpx)
                            {
                                isDocumentJpx = true;
                            }
                        }
                        else if (pdfImage.ImageDictionary.TryGet(NameToken.Filter, out ArrayToken filterArray))
                        {
                            isJpx = filterArray.Data.OfType<NameToken>().Any(f => f.Data.ToUpper().Contains("JPX"));
                            if (isJpx)
                            {
                                isDocumentJpx = true;
                            }
                        }
                        else
                        {
                            throw new Exception("Could not get a valid Filter.");
                        }

                        if (!isJpx)
                        {
                            continue;
                        }

                        Assert.True(pdfImage.TryGetPng(out var bytes));

                        File.WriteAllBytes(Path.Combine(_outputPath, $"{fileRootName}_{i++}.png"), bytes);
                    }
                }
            }

            if (!isDocumentJpx)
            {
                throw new Exception("No JPX image in document.");
            }
        }
    }
}