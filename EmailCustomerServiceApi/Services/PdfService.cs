using DinkToPdf;
using DinkToPdf.Contracts;

namespace EmailCustomerServiceApi.Services
{
    public class PdfService
    {
        private readonly IConverter _converter;
        public PdfService(IConverter converter)
        {
            _converter = converter;
        }
        public byte[] GeneratePdf(string htmlContent, string fileName)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Margins = new MarginSettings { Top = 10, Left = 10, Right = 10 },
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                DocumentTitle = fileName
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontSize = 13, Line = true, Right = "Page [page] of [toPage]", Spacing = 2.812 },
                FooterSettings = { FontSize = 13, Line = true, Right = "0 " + DateTime.Now.Year }
            };
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }
    }
}
