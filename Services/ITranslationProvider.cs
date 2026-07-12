using System.Threading;
using System.Threading.Tasks;

namespace AllaganTranslator.Services
{
    public interface ITranslationProvider
    {
        bool IsReady { get; }
        Task InitializeAsync(CancellationToken token = default);
        Task<string> TranslateAsync(string text, string targetLanguage, CancellationToken token);
        void Dispose();
    }
}

