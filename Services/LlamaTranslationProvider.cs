using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using LLama;
using LLama.Common;

namespace AllaganTranslator.Services
{
    public class LlamaTranslationProvider : ITranslationProvider
    {
        private readonly IPluginLog log;
        private readonly ModelManager modelManager;
        private readonly Configuration configuration;

        private LLamaWeights? model;
        private LLamaContext? context;
        private InstructExecutor? executor;
        private TranslationEngineType? loadedEngineType;

        public bool IsReady => this.executor != null && this.loadedEngineType == this.configuration.TranslationEngine;
        public bool IsDownloading => this.modelManager.IsDownloading;
        public bool IsModelDownloaded => this.modelManager.IsModelDownloaded(this.configuration.TranslationEngine == TranslationEngineType.LocalLlama8B_CPU);

        public event Action<float>? OnDownloadProgress
        {
            add => this.modelManager.OnDownloadProgress += value;
            remove => this.modelManager.OnDownloadProgress -= value;
        }

        public LlamaTranslationProvider(IPluginLog log, ModelManager modelManager, Configuration configuration)
        {
            this.log = log;
            this.modelManager = modelManager;
            this.configuration = configuration;
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            try
            {
                if (this.executor != null)
                {
                    this.Dispose();
                }

                bool is8BModel = this.configuration.TranslationEngine == TranslationEngineType.LocalLlama8B_CPU;
                this.modelManager.SetupNativePaths();
                var modelPath = await this.modelManager.EnsureModelDownloadedAsync(is8BModel, token);

                this.log.Information("Inizializzazione modello Llama in memoria...");
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 1024,
                    GpuLayerCount = 0
                };
                
                this.model = LLamaWeights.LoadFromFile(parameters);
                this.context = this.model.CreateContext(parameters);
                this.executor = new InstructExecutor(this.context);
                this.loadedEngineType = this.configuration.TranslationEngine;
                
                this.log.Information("[LlamaTranslationProvider] Modello caricato e pronto all'uso!");
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[LlamaTranslationProvider] Errore nel caricamento del modello.");
            }
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, CancellationToken token)
        {
            if (this.executor == null)
            {
                return "Modello non inizializzato.";
            }
            
            try
            {
                string languageName = targetLanguage.ToLower() switch
                {
                    "it" => "Italian",
                    "es" => "Spanish",
                    "fr" => "French",
                    "de" => "German",
                    "ja" => "Japanese",
                    "pt" => "Portuguese",
                    "ru" => "Russian",
                    _ => targetLanguage
                };

                // Prompt dinamico con il targetLanguage corretto
                var prompt = $"<|begin_of_text|><|start_header_id|>system<|end_header_id|>\n\nYou are an expert professional localization translator for Final Fantasy XIV. Translate the following text into {languageName}. Maintain a natural, fluid, and colloquial tone suitable for a fantasy RPG. Do NOT translate idioms literally, adapt them to natural {languageName} equivalents. Output ONLY the final translation without any explanations, quotes, or additional text.<|eot_id|><|start_header_id|>user<|end_header_id|>\n\n{text}<|eot_id|><|start_header_id|>assistant<|end_header_id|>\n\n";
                var inferenceParams = new InferenceParams() { 
                    MaxTokens = 256, 
                    AntiPrompts = new List<string> { "<|eot_id|>" }
                };

                // Uso StringBuilder per evitare colli di bottiglia di memoria
                StringBuilder sb = new StringBuilder();
                await foreach (var tokenText in this.executor.InferAsync(prompt, inferenceParams, token))
                {
                    sb.Append(tokenText);
                }
                return sb.ToString();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "Errore durante l'inferenza di Llama.");
                return "Errore locale Llama.";
            }
        }

        public void Dispose()
        {
            this.executor = null;
            this.context?.Dispose();
            this.model?.Dispose();
            this.loadedEngineType = null;
        }
    }
}

