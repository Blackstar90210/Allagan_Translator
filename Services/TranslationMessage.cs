using Dalamud.Game.Text;

namespace AllaganTranslator.Services
{
    public class TranslationMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public XivChatType ChatType { get; set; }
        public string TranslatedText { get; set; } = string.Empty;
    }
}

