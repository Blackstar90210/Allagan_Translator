using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Newtonsoft.Json.Linq;

namespace AllaganTranslator.Services
{
    public class GoogleTranslationProvider : ITranslationProvider
    {
        private readonly IPluginLog log;
        private readonly HttpClient httpClient;

        public bool IsReady { get; private set; } = false;

        public GoogleTranslationProvider(IPluginLog log)
        {
            this.log = log;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public Task InitializeAsync(CancellationToken token = default)
        {
            this.IsReady = true;
            return Task.CompletedTask;
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, CancellationToken token)
        {
            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={targetLanguage}&dt=t&q={Uri.EscapeDataString(text)}";
                var response = await this.httpClient.GetStringAsync(url, token);
                var json = JArray.Parse(response);
                
                string translatedText = "";
                var firstItem = json[0] as JArray;
                if (firstItem != null)
                {
                    foreach (var item in firstItem)
                    {
                        translatedText += item[0]?.ToString() ?? "";
                    }
                }

                await Task.Delay(300, token); // Previene HTTP 429
                return translatedText;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "Errore durante la chiamata a Google Translate.");
                return "Errore di connessione API Google.";
            }
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}

