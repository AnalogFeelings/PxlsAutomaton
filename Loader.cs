using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;

namespace PxlsAutomaton
{
    public class Loader
    {
        public string LoadedImage;

        public Bitmap DownloadImageAndResize(string Url, int Width, int Height)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    Client.Headers.Add("User-Agent", "AestheticalZ/PxlsAutomaton");

                    string TargetFilename = "download";

                    byte[] ImageBytes = Client.DownloadData(Url);
                    string ImageType = Client.ResponseHeaders[HttpResponseHeader.ContentType];

                    if (ImageType == null) throw new Exception("Error getting image type from URL.");

                    switch (ImageType)
                    {
                        case "image/jpeg": TargetFilename += ".jpeg"; break;
                        case "image/png": TargetFilename += ".png"; break;
                        default: throw new Exception("Invalid image type. Must be JPEG or PNG.");
                    }

                    File.WriteAllBytes(TargetFilename, ImageBytes);
                    LoadedImage = TargetFilename;

                    if (Width <= 0 || Height <= 0) return (Bitmap)Image.FromFile(TargetFilename);
                    else return LoadAndResizeBmp(TargetFilename, Width, Height);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("ERROR: " + ex.Message, LogSeverity.Error);

                return null;
            }
        }

        public Bitmap LoadAndResizeBmp(string Filename, int Width, int Height)
        {
            Bitmap SourceImage = new Bitmap(Image.FromFile(Filename));
            Bitmap ResultImage = new Bitmap(Width, Height);

            using (Graphics GdiGraphics = Graphics.FromImage(ResultImage))
            {
                GdiGraphics.Clear(Color.White);
                GdiGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                GdiGraphics.DrawImage(SourceImage, 0, 0, Width, Height);
            }

            return ResultImage;
        }
    }
}
