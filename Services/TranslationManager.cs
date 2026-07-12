using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace AllaganTranslator.Services
{
    public class TranslationManager : IDisposable
    {
        private readonly IPluginLog log;
        private readonly IDataManager dataManager;
        private readonly Configuration configuration;
        
        private readonly BlockingCollection<TranslationMessage> translationQueue;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task workerTask;
        private readonly Dictionary<string, string> translationCache = new();
        private readonly HashSet<string> luminaContextTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private static readonly Regex urlRegex = new Regex(@"(http[s]?://\S+|www\.\S+|\w+\.\w+/\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<string, Regex> luminaRegexCache = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Regex> glossaryRegexCache = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);

        private ITranslationProvider? currentProvider;
        private readonly GoogleTranslationProvider googleProvider;
        private readonly LlamaTranslationProvider llamaProvider;

        public bool IsReady => this.currentProvider?.IsReady ?? false;
        public bool IsDownloading => this.llamaProvider.IsDownloading;

        public bool IsCurrentModelDownloaded()
        {
            if (this.configuration.TranslationEngine == TranslationEngineType.GoogleCloudFree) return true;
            return this.llamaProvider.IsModelDownloaded;
        }

        public event Action<TranslationMessage>? OnTranslationFinished;
        
        public event Action<float>? OnDownloadProgress
        {
            add => this.llamaProvider.OnDownloadProgress += value;
            remove => this.llamaProvider.OnDownloadProgress -= value;
        }

        public TranslationManager(IPluginLog log, IDataManager dataManager, Configuration configuration, 
            GoogleTranslationProvider googleProvider, LlamaTranslationProvider llamaProvider)
        {
            this.log = log;
            this.dataManager = dataManager;
            this.configuration = configuration;
            
            this.googleProvider = googleProvider;
            this.llamaProvider = llamaProvider;
            
            this.translationQueue = new BlockingCollection<TranslationMessage>(new ConcurrentQueue<TranslationMessage>());
            this.cancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(() => ExtractLuminaTerms());

            this.workerTask = Task.Run(ProcessQueue, this.cancellationTokenSource.Token);
            this.log.Information("[TranslationManager] Inizializzato.");
        }

        public async Task InitializeModelAsync(bool forceDownload = false)
        {
            if (this.configuration.TranslationEngine == TranslationEngineType.GoogleCloudFree)
            {
                this.currentProvider = this.googleProvider;
            }
            else
            {
                this.currentProvider = this.llamaProvider;
            }

            if (this.currentProvider != null && !this.currentProvider.IsReady)
            {
                if (this.currentProvider == this.llamaProvider && !this.llamaProvider.IsModelDownloaded && !forceDownload)
                {
                    // Non inizializziamo (e non scarichiamo) finché l'utente non lo richiede esplicitamente
                    return;
                }

                await this.currentProvider.InitializeAsync(this.cancellationTokenSource.Token);
            }
        }

        private void ExtractLuminaTerms()
        {
            try
            {
                this.log.Information("[TranslationManager] Inizio estrazione termini da Lumina...");
                
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
                this.log.Information($"[TranslationManager] Estrazione completata: {this.luminaContextTerms.Count} termini caricati.");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationManager] Errore estrazione Lumina.");
            }
        }

        public void EnqueueTranslation(TranslationMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text)) return;
            if (this.currentProvider == null || !this.currentProvider.IsReady)
            {
                this.log.Warning($"[TranslationManager] Modello non pronto. Messaggio ignorato: {message.Text}");
                return;
            }

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
                    this.log.Information($"[TranslationManager] Traduzione in corso per: {msg.Text}");
                    
                    var placeholders = new Dictionary<string, string>();
                    int pIndex = 0;
                    string textToTranslate = msg.Text;

                    textToTranslate = urlRegex.Replace(textToTranslate, m => {
                        string ph = $"___PH{pIndex++}___";
                        placeholders[ph] = m.Value;
                        return ph;
                    });

                    var customGlossaryRules = this.configuration.CustomGlossary.ToList();
                    foreach (var rule in customGlossaryRules)
                    {
                        if (!this.glossaryRegexCache.TryGetValue(rule.Key, out var regex))
                        {
                            regex = new Regex($@"\b{Regex.Escape(rule.Key)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            this.glossaryRegexCache[rule.Key] = regex;
                        }

                        textToTranslate = regex.Replace(textToTranslate, m => {
                            string ph = $"___PH{pIndex++}___";
                            placeholders[ph] = rule.Value;
                            return ph;
                        });
                    }

                    var foundLumina = this.luminaContextTerms.Where(t => textToTranslate.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var term in foundLumina)
                    {
                        if (!this.luminaRegexCache.TryGetValue(term, out var regex))
                        {
                            regex = new Regex($@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            this.luminaRegexCache[term] = regex;
                        }

                        textToTranslate = regex.Replace(textToTranslate, m => {
                            string ph = $"___PH{pIndex++}___";
                            placeholders[ph] = m.Value;
                            return ph;
                        });
                    }

                    if (this.translationCache.TryGetValue(msg.Text, out var cachedText))
                    {
                        msg.TranslatedText = cachedText;
                        OnTranslationFinished?.Invoke(msg);
                        continue;
                    }

                    string translatedText = "";
                    if (this.currentProvider != null)
                    {
                        translatedText = await this.currentProvider.TranslateAsync(textToTranslate, this.configuration.TargetLanguage, this.cancellationTokenSource.Token);
                    }

                    translatedText = Regex.Replace(translatedText, @"_*\s*PH\s*(\d+)\s*_*", "___PH$1___", RegexOptions.IgnoreCase);
                    foreach (var kvp in placeholders)
                    {
                        translatedText = translatedText.Replace(kvp.Key, kvp.Value);
                    }

                    translatedText = translatedText.Replace(">", "").Replace("♪", "~").Trim();

                    msg.TranslatedText = translatedText;
                    this.translationCache[msg.Text] = translatedText;

                    OnTranslationFinished?.Invoke(msg);
                }
            }
            catch (OperationCanceledException)
            {
                this.log.Information("[TranslationManager] Servizio fermato.");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[TranslationManager] Errore imprevisto nel worker thread.");
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
        }
    }
}

