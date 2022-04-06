using Newtonsoft.Json;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;

namespace PxlsAutomaton
{
    public class Bot
    {
        public Config BotConfig = null;

        public Ditherer Ditherer = new Ditherer();
        public Loader Loader = new Loader();

        public ClientWebSocket SocketClient = new ClientWebSocket();
        public Bitmap TargetImage = null;

        public void LoadConfig()
        {
            if (!File.Exists("config.json"))
            {
                Logger.Log("Configuration file is missing. Writing default one.", LogSeverity.Error);
                BotConfig = new Config();

                JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                };
                string DefaultJson = JsonConvert.SerializeObject(BotConfig, SerializerSettings);

                File.WriteAllText("config.json", DefaultJson);
                BotConfig = null;

                return;
            }

            BotConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            if (BotConfig == null) Logger.Log("Error loading configuration file.", LogSeverity.Error);
        }

        public async Task InitializeBot()
        {
            if (BotConfig == null) return;

            if (BotConfig.PixelDelay < 50)
                Logger.Log($"A pixel delay of {BotConfig.PixelDelay}ms may cause timing issues. Please set it to 50ms or more.", LogSeverity.Warning);

            if (BotConfig.ImageWidth <= 0 || BotConfig.ImageHeight <= 0) Logger.Log($"Downloading and resizing image from {BotConfig.ImageUrl}...", LogSeverity.Message);
            else Logger.Log($"Downloading image from {BotConfig.ImageUrl}...", LogSeverity.Message);
            Bitmap DownloadedBitmap = Loader.DownloadImageAndResize(BotConfig.ImageUrl, BotConfig.ImageWidth, BotConfig.ImageHeight);

            if(DownloadedBitmap == null) return;

            Logger.Log("Downloaded image successfully!", LogSeverity.Success);

            Logger.Log("Dithering image...", LogSeverity.Message);
            TargetImage = Ditherer.ExecuteFloydSteinberg(ref DownloadedBitmap);

            Logger.Log("Image dithered successfully!", LogSeverity.Success);

            Logger.Log($"Attempting to open websocket connection to \"{BotConfig.PxlsUrl}\"...", LogSeverity.Message);
            try
            {
                CancellationTokenSource TimeoutToken = new CancellationTokenSource();
                TimeoutToken.CancelAfter(5000);
                TimeoutToken.Token.ThrowIfCancellationRequested();

                await SocketClient.ConnectAsync(new Uri(BotConfig.PxlsUrl), TimeoutToken.Token);
                Console.Title = $"PxlsAutomaton - Connected to {BotConfig.PxlsUrl}";
                Logger.Log("Connection established successfully!", LogSeverity.Success);

                Task.WaitAll(SendThread(), ReceiveThread());
            }
            catch (Exception ex)
            {
                Logger.Log("ERROR: " + ex.Message, LogSeverity.Error);
            }
            finally
            {
                if (SocketClient != null) SocketClient.Dispose();
                if (DownloadedBitmap != null) DownloadedBitmap.Dispose();
                if (TargetImage != null) TargetImage.Dispose();
                if (File.Exists(Loader.LoadedImage)) File.Delete(Loader.LoadedImage);
            }
        }

        private async Task SendThread()
        {
            Logger.Log($"Drawing {TargetImage.Width}x{TargetImage.Height} dithered image " +
                        $"at ({BotConfig.PositionX}, {BotConfig.PositionY}) with {BotConfig.PixelDelay}ms delay...", LogSeverity.Message);

            for (int y = 0; y < TargetImage.Height; y++)
            {
                for (int x = 0; x < TargetImage.Width; x++)
                {
                    if (SocketClient.State != WebSocketState.Open) throw new Exception("Lost connection to websocket.");

                    CancellationTokenSource SendTimeoutToken = new CancellationTokenSource();
                    SendTimeoutToken.CancelAfter(5000);
                    SendTimeoutToken.Token.ThrowIfCancellationRequested();

                    Color PixelColor = TargetImage.GetPixel(x, y);
                    int ColorIndex = Ditherer.ColorPalette[PixelColor];
                    Pixel PixelRequest = new Pixel()
                    {
                        Type = "pixel",
                        X = x + BotConfig.PositionX,
                        Y = y + BotConfig.PositionY,
                        Color = ColorIndex
                    };
                    string JsonPixel = JsonConvert.SerializeObject(PixelRequest);

                    await SocketClient.SendAsync(Encoding.UTF8.GetBytes(JsonPixel), WebSocketMessageType.Text, true, SendTimeoutToken.Token);

                    await Task.Delay(BotConfig.PixelDelay);
                }
            }

            Logger.Log("Image has been drawn successfully!", LogSeverity.Success);

            await SocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        private async Task ReceiveThread()
        {
            byte[] buffer = new byte[64];
            while (SocketClient.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await SocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) await SocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                catch (Exception)
                {
                    //Do nothing. Receiving is not as important as sending.
                }
            }
        }
    }

    public class Config
    {
        public string ImageUrl { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PixelDelay { get; set; }
        public string PxlsUrl { get; set; }
    }

    public class Pixel
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("color")]
        public int Color { get; set; }
    }
}
