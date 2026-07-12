using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using System;

namespace AllaganTranslator.Services
{
    public class ChatManager : IDisposable
    {
        private readonly IChatGui chat;
        private readonly IPluginLog log;
        private readonly TranslationManager translationManager;
        private readonly Configuration configuration;

        public ChatManager(IChatGui chat, IPluginLog log, TranslationManager translationManager, Configuration configuration)
        {
            this.chat = chat;
            this.log = log;
            this.translationManager = translationManager;
            this.configuration = configuration;

            this.chat.ChatMessage += OnChatMessage;
        }

        private void OnChatMessage(IHandleableChatMessage message)
        {
            bool isSupportedChatType = message.LogKind switch
            {
                XivChatType.Say => this.configuration.TranslateSay,
                XivChatType.Yell => this.configuration.TranslateYell,
                XivChatType.Shout => this.configuration.TranslateShout,
                XivChatType.Party => this.configuration.TranslateParty,
                XivChatType.CrossParty => this.configuration.TranslateCrossParty,
                XivChatType.Alliance => this.configuration.TranslateAlliance,
                XivChatType.FreeCompany => this.configuration.TranslateFreeCompany,
                XivChatType.TellIncoming => this.configuration.TranslateTell,
                XivChatType.NPCDialogue => this.configuration.TranslateNPCDialogue,
                XivChatType.NPCDialogueAnnouncements => this.configuration.TranslateNPCDialogue,
                _ => false
            };

            if (!isSupportedChatType)
                return;

            var text = message.Message.TextValue;
            var senderName = message.Sender?.TextValue ?? "";
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                this.log.Information($"[ChatManager] Intercettato ({message.LogKind}): {text}");
                
                var translationMsg = new TranslationMessage
                {
                    Sender = senderName,
                    Text = text,
                    ChatType = message.LogKind
                };
                
                this.translationManager.EnqueueTranslation(translationMsg);
            }
        }

        public void Dispose()
        {
            this.chat.ChatMessage -= OnChatMessage;
        }
    }
}

