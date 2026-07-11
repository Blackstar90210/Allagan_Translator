using System;
using System.Linq;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using DalamudTranslator.Windows;
using DalamudTranslator.Services;

namespace DalamudTranslator
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Allagan Translator (Local AI & Online)";

        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static IChatGui Chat { get; private set; } = null!;
        public static IPluginLog Log { get; private set; } = null!;
        public static ICommandManager CommandManager { get; private set; } = null!;

        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Allagan Translator (Local AI & Online)");
        public ConfigWindow ConfigWindow { get; init; }
        public TranslatorOverlay TranslatorOverlay { get; init; }
        public TranslationService TranslationService { get; init; }
        public static IDataManager DataManager { get; private set; } = null!;

        private const string CommandName = "/translator";

        public Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPluginLog log, ICommandManager commandManager, IDataManager dataManager)
        {
            PluginInterface = pluginInterface;
            Chat = chat;
            Log = log;
            CommandManager = commandManager;
            DataManager = dataManager;

            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(PluginInterface);

            this.TranslationService = new TranslationService(Log, DataManager, this.Configuration, PluginInterface.ConfigDirectory.FullName);
            this.ConfigWindow = new ConfigWindow(this.Configuration, this.TranslationService);
            this.TranslatorOverlay = new TranslatorOverlay(this.Configuration, DrawConfigUI);

            // Inizializza il modello asincronamente (scarica se necessario)
            _ = this.TranslationService.InitializeModelAsync();

            this.TranslationService.OnTranslationFinished += (translatedText) => 
            {
                this.TranslatorOverlay.AddTranslation(translatedText);
            };
            
            this.WindowSystem.AddWindow(this.ConfigWindow);
            this.WindowSystem.AddWindow(this.TranslatorOverlay);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Apre la configurazione del Dalamud Translator."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;

            Chat.ChatMessage += OnChatMessage;

            Chat.Print("Dalamud Translator Loaded!");
        }

        private void OnChatMessage(IHandleableChatMessage message)
        {
            // Verifica che il tipo di chat sia uno di quelli supportati
            bool isSupportedChatType = message.LogKind switch
            {
                XivChatType.Say => true,
                XivChatType.Yell => true,
                XivChatType.Shout => true,
                XivChatType.Party => true,
                XivChatType.CrossParty => true,
                XivChatType.Alliance => true,
                XivChatType.FreeCompany => true,
                XivChatType.TellIncoming => true,
                XivChatType.NPCDialogue => true,
                XivChatType.NPCDialogueAnnouncements => true,
                _ => false
            };

            if (!isSupportedChatType)
                return;

            var text = message.Message.TextValue;
            var senderName = message.Sender?.TextValue ?? "";
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                Log.Information($"[Translator] Intercettato ({message.LogKind}): {text}");
                
                var translationMsg = new TranslationMessage
                {
                    Sender = senderName,
                    Text = text,
                    ChatType = message.LogKind
                };
                
                this.TranslationService.EnqueueTranslation(translationMsg);
            }
        }

        private void OnCommand(string command, string args)
        {
            this.ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            this.ConfigWindow.IsOpen = true;
        }

        private void DrawMainUI()
        {
            this.TranslatorOverlay.IsOpen = true;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.ConfigWindow.Dispose();
            
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            PluginInterface.UiBuilder.OpenMainUi -= DrawMainUI;
            
            CommandManager.RemoveHandler(CommandName);
            Chat.ChatMessage -= OnChatMessage;
            
            this.TranslationService.Dispose();
        }
    }
}
