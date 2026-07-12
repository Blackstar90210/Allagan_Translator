using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using LLama.Native;

namespace AllaganTranslator.Services
{
    public class ModelManager
    {
        private readonly IPluginLog log;
        private readonly string configDirectory;
        private readonly string pluginDirectory;
        private const string ModelUrl3B = "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";
        private const string ModelUrl8B = "https://huggingface.co/bartowski/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf";

        public event Action<float>? OnDownloadProgress;
        public bool IsDownloading { get; private set; } = false;

        public ModelManager(IPluginLog log, string configDirectory, string pluginDirectory)
        {
            this.log = log;
            this.configDirectory = configDirectory;
            this.pluginDirectory = pluginDirectory;
        }

        private bool isNativePathsSetup = false;

        public void SetupNativePaths()
        {
            if (this.isNativePathsSetup) return;

            try 
            {
                if (!string.IsNullOrEmpty(pluginDirectory))
                {
                    var backendFolder = "avx2";
                    var nativeDir = Path.Combine(pluginDirectory, "runtimes", "win-x64", "native", backendFolder);
                    
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

            try
            {
                NativeLibraryConfig.All.WithLogCallback(delegate(LLamaLogLevel level, string message) {
                    // Disable verbose C++ logs
                });
            }
            catch (Exception ex)
            {
                this.log.Warning(ex, "Impossibile impostare il log callback per LLamaSharp (potrebbe essere già impostato).");
            }

            this.isNativePathsSetup = true;
        }

        public bool IsModelDownloaded(bool is8BModel)
        {
            var modelName = is8BModel ? "llama_3.1_8b_model.gguf" : "llama_3.2_3b_model.gguf";
            var modelPath = Path.Combine(this.configDirectory, modelName);
            var expectedMinSize = is8BModel ? 4_000_000_000L : 1_500_000_000L;
            return File.Exists(modelPath) && new FileInfo(modelPath).Length >= expectedMinSize;
        }

        public async Task<string> EnsureModelDownloadedAsync(bool is8BModel, CancellationToken token = default)
        {
            var modelName = is8BModel ? "llama_3.1_8b_model.gguf" : "llama_3.2_3b_model.gguf";
            var modelPath = Path.Combine(this.configDirectory, modelName);
            
            var expectedMinSize = is8BModel ? 4_000_000_000L : 1_500_000_000L;
            if (File.Exists(modelPath) && new FileInfo(modelPath).Length < expectedMinSize)
            {
                this.log.Information("Rilevato file del modello corrotto o parziale. Eliminazione in corso...");
                File.Delete(modelPath);
            }

            if (!File.Exists(modelPath))
            {
                var tmpPath = modelPath + ".tmp";
                await DownloadModelAsync(tmpPath, is8BModel, token);
                
                if (File.Exists(tmpPath))
                {
                    File.Move(tmpPath, modelPath);
                }
                else
                {
                    throw new Exception("Il download del modello è fallito.");
                }
            }
            return modelPath;
        }

        private async Task DownloadModelAsync(string destPath, bool is8BModel, CancellationToken token)
        {
            this.IsDownloading = true;
            this.log.Information("[ModelManager] Download del modello linguistico iniziato...");
            
            var modelUrl = is8BModel ? ModelUrl8B : ModelUrl3B;
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();
                var fallbackSize = is8BModel ? 4920000000L : 2142277888L;
                var totalBytes = response.Content.Headers.ContentLength ?? fallbackSize;
                
                using var contentStream = await response.Content.ReadAsStreamAsync(token);
                using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;
                var lastReport = DateTime.Now;

                do
                {
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer, 0, read, token);
                        totalRead += read;

                        if ((DateTime.Now - lastReport).TotalMilliseconds > 500)
                        {
                            var progress = (float)totalRead / totalBytes;
                            OnDownloadProgress?.Invoke(progress);
                            lastReport = DateTime.Now;
                        }
                    }
                }
                while (isMoreToRead);
                
                this.log.Information("[ModelManager] Download completato.");
            }
            catch (OperationCanceledException)
            {
                this.log.Information("[ModelManager] Download annullato.");
                if (File.Exists(destPath)) File.Delete(destPath);
                throw;
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "[ModelManager] Errore durante il download.");
                if (File.Exists(destPath)) File.Delete(destPath);
                throw;
            }
            finally
            {
                this.IsDownloading = false;
            }
        }
    }
}

