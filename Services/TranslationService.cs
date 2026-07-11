using System;
using System.IO;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Lumina.Excel.Sheets;
using LLama;
using LLama.Common;
using LLama.Native;

namespace DalamudTranslator.Services
{
    public class TranslationMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public XivChatType ChatType { get; set; }
        public string TranslatedText { get; set; } = string.Empty;
    }

    public class TranslationService : IDisposable
    {
        private readonly IPluginLog log;
        private readonly IDataManager dataManager;
        private readonly Configuration configuration;
        private readonly string configDirectory;
        private readonly BlockingCollection<TranslationMessage> translationQueue;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task workerTask;
        private readonly Dictionary<string, string> translationCache = new();
        
        private readonly HashSet<string> luminaContextTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private LLamaWeights? model;
        private LLamaContext? context;
        private InstructExecutor? executor;
        private readonly HttpClient httpClient;

        public event Action<TranslationMessage>? OnTranslationFinished;
        public event Action<float>? OnDownloadProgress;
        
        public bool IsReady { get; private set; } = false;
        public bool IsDownloading { get; private set; } = false;

        private const string ModelUrl = "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";

        public TranslationService(IPluginLog log, IDataManager dataManager, Configuration configuration, string configDirectory)
        {
            this.log = log;
            this.dataManager = dataManager;
            this.configuration = configuration;
            this.configDirectory = configDirectory;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            this.translationQueue = new BlockingCollection<TranslationMessage>(new ConcurrentQueue<TranslationMessage>());
            this.cancellationTokenSource = new CancellationTokenSource();

            try 
            {
                var pluginDir = Plugin.PluginInterface.AssemblyLocation.DirectoryName;
                if (pluginDir != null)
                {
                    var nativeDir = Path.Combine(pluginDir, "runtimes", "win-x64", "native", "avx2");
                    
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                    if (!currentPath.Contains(nativeDir))
                    {
                        Environment.SetEnvironmentVariable("PATH", nativeDir + ";" + currentPath);
                    }

                    NativeLibraryConfig.All.WithSearchDirectory(nativeDir);
                    NativeLibraryConfig.All.WithLibrary(Path.Combine(nativeDir, "llama.dll"), Path.Combine(nativeDir, "mtmd.dll"));
                }
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "Impossibile impostare la directory nativa per LLamaSharp.");
            }

            NativeLibraryConfig.All.WithLogCallback(delegate(LLamaLogLevel level, string message) {
                // Disable verbose C++ logs
            });

            _ = Task.Run(() => ExtractLuminaTerms());

            this.workerTask = Task.Run(ProcessQueue, this.cancellationTokenSource.Token);
            this.log.Information("[TranslationService] Servizio inizializzato e in ascolto.");
        }

        private void ExtractLuminaTerms()
        {
            try
            {
                this.log.Information("[TranslationService] Inizio estrazione termini da Lumina...");
                
                var places = this.dataManager.GetExcelSheet<Lumina.Excel.Sheets.PlaceName>();
                if (places != null)
                {
                    foreach (var place in places)
                    {
                        var name = place.Name.ToString();
                        if (name.Length > 4) this.luminaContextTerms.Add(name);
                    }
                }
                
                var duties = this.dataManager.GetExcelSheet<Lumina.Excel.Sheets.ContentFinderCondition>();
                if (duties != null)
                {
                    foreach (var duty in duties)
                    {
                        var name = duty.Name.ToString();
                        if (name.Length > 4) this.luminaContextTerms.Add(name);
                    }
                }
                this.log.Information($"[TranslationService] Estrazione completata: {this.luminaContextTerms.Count} termini caricati in memoria.");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationService] Errore durante l'estrazione da Lumina.");
            }
        }

        public async Task InitializeModelAsync()
        {
            if (this.configuration.TranslationEngine == TranslationEngineType.GoogleCloudFree)
            {
                this.IsReady = true;
                return;
            }

            try
            {
                var modelPath = Path.Combine(this.configDirectory, "llama_3.2_3b_model.gguf");
                
                // Se il file esiste ma è più piccolo di 1.5GB, probabile download corrotto (spegnimento PC ecc.)
                if (File.Exists(modelPath) && new FileInfo(modelPath).Length < 1_500_000_000)
                {
                    this.log.Information("Rilevato file del modello corrotto o parziale. Eliminazione in corso...");
                    File.Delete(modelPath);
                }

                if (!File.Exists(modelPath))
                {
                    this.IsDownloading = true;
                    this.log.Information("Download del modello Llama in corso...");
                    
                    var url = "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";
                    var tmpPath = modelPath + ".tmp";
                    
                    using (var response = await this.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }
                    
                    File.Move(tmpPath, modelPath);
                    this.IsDownloading = false;
                }

                this.log.Information("Inizializzazione modello in memoria...");
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 1024,
                    GpuLayerCount = 0 // Usiamo esplicitamente 0 per la pura CPU
                };
                
                this.model = LLamaWeights.LoadFromFile(parameters);
                this.context = this.model.CreateContext(parameters);
                this.executor = new InstructExecutor(this.context);
                
                this.IsReady = true;
                this.log.Information("[TranslationService] Modello caricato e pronto all'uso!");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationService] Errore nel caricamento del modello.");
            }
        }

        private async Task DownloadModelAsync(string destPath)
        {
            this.IsDownloading = true;
            this.log.Information("[TranslationService] Download del modello linguistico iniziato...");
            
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;
                var lastReport = DateTime.Now;

                do
                {
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalRead += read;

                        if (canReportProgress && (DateTime.Now - lastReport).TotalMilliseconds > 500)
                        {
                            var progress = (float)totalRead / totalBytes;
                            OnDownloadProgress?.Invoke(progress);
                            lastReport = DateTime.Now;
                        }
                    }
                }
                while (isMoreToRead);
                
                this.log.Information("[TranslationService] Download completato.");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationService] Errore durante il download.");
                if (File.Exists(destPath)) File.Delete(destPath);
            }
            finally
            {
                this.IsDownloading = false;
            }
        }

        public void EnqueueTranslation(TranslationMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text)) return;
            if (!this.IsReady) return; // Se non Ã¨ pronto, ignora.

            if (!this.translationQueue.IsAddingCompleted)
            {
                this.translationQueue.Add(message);
            }
        }

        private async Task ProcessQueue()
        {
            try
            {
                foreach (var msg in this.translationQueue.GetConsumingEnumerable(this.cancellationTokenSource.Token))
                {
                    this.log.Information($"[TranslationService] Traduzione reale in corso per: {msg.Text}");
                    
                    var foundLumina = this.luminaContextTerms.Where(t => msg.Text.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();
                    var customGlossaryRules = this.configuration.CustomGlossary.Where(kvp => msg.Text.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)).ToList();

                    string glossaryInstructions = "";
                    
                    if (foundLumina.Count > 0)
                    {
                        var luminaStr = string.Join(", ", foundLumina.Select(t => $"'{t}'"));
                        glossaryInstructions += $"\nCRITICAL INSTRUCTION: You MUST keep the following FFXIV proper nouns EXACTLY in English, do not translate them: {luminaStr}.";
                    }
                    
                    if (customGlossaryRules.Count > 0)
                    {
                        var rulesStr = string.Join(", ", customGlossaryRules.Select(r => $"'{r.Key}' = '{r.Value}'"));
                        glossaryInstructions += $"\nCRITICAL INSTRUCTION: You MUST use these exact Italian translations for these terms: {rulesStr}.";
                    }

                    if (this.translationCache.TryGetValue(msg.Text, out var cachedText))
                    {
                        msg.TranslatedText = cachedText;
                        OnTranslationFinished?.Invoke(msg);
                        continue;
                    }

                    string translatedText = "";

                    if (this.configuration.TranslationEngine == TranslationEngineType.GoogleCloudFree)
                    {
                        try
                        {
                            var targetLang = this.configuration.TargetLanguage;
                            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={targetLang}&dt=t&q={Uri.EscapeDataString(msg.Text)}";
                            var response = await this.httpClient.GetStringAsync(url);
                            var json = JArray.Parse(response);
                            
                            translatedText = "";
                            foreach (var item in json[0])
                            {
                                translatedText += item[0].ToString();
                            }

                            // Google sometimes translates the Glossary rules if we don't manually apply them post-translation (or pre-translation).
                            // A simple post-translation replace for the custom glossary:
                            foreach (var rule in customGlossaryRules)
                            {
                                translatedText = translatedText.Replace(rule.Key, rule.Value, StringComparison.OrdinalIgnoreCase);
                            }

                            await Task.Delay(300); // Previene HTTP 429 (Too Many Requests)
                        }
                        catch (Exception ex)
                        {
                            this.log.Error(ex, "Errore durante la chiamata a Google Translate.");
                            translatedText = "Errore di connessione API Google.";
                        }
                    }
                    else
                    {
                        if (this.executor == null)
                        {
                            // Modello Llama non ancora inizializzato
                            continue;
                        }
                        
                        // Llama 3 Format
                        var prompt = $"<|begin_of_text|><|start_header_id|>system<|end_header_id|>\n\nYou are a professional localization translator for Final Fantasy XIV. Translate the following text into Italian. Output only the Italian translation, nothing else.{glossaryInstructions}<|eot_id|><|start_header_id|>user<|end_header_id|>\n\n{msg.Text}<|eot_id|><|start_header_id|>assistant<|end_header_id|>\n\n";
                        var inferenceParams = new InferenceParams() { MaxTokens = 256, AntiPrompts = new List<string> { "<|eot_id|>" } };

                        await foreach (var token in this.executor.InferAsync(prompt, inferenceParams, this.cancellationTokenSource.Token))
                        {
                            translatedText += token;
                        }
                    }

                    // Ripulisce eventuali sbavature del modello e font non supportati da ImGui
                    translatedText = translatedText.Replace(">", "").Replace("♪", "~").Trim();

                    msg.TranslatedText = translatedText;
                    this.translationCache[msg.Text] = translatedText;

                    OnTranslationFinished?.Invoke(msg);
                }
            }
            catch (OperationCanceledException)
            {
                this.log.Information("[TranslationService] Servizio fermato.");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationService] Errore imprevisto nel worker thread.");
            }
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
            this.translationQueue.CompleteAdding();
            
            try
            {
                this.workerTask.Wait(1000);
            }
            catch (AggregateException) { }
            
            this.translationQueue.Dispose();
            this.cancellationTokenSource.Dispose();
            
            this.executor = null;
            this.context?.Dispose();
            this.model?.Dispose();
        }
    }
}
