using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using AllaganTranslator.Services;
using AllaganTranslator.Windows;

namespace AllaganTranslator
{
    public sealed class Plugin : IDalamudPlugin
    {
        public const string PluginName = "Allagan Translator (Local AI & Online)";
        public string Name => PluginName;

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;

        public Configuration Configuration { get; init; }
        
        private readonly ModelManager modelManager;
        private readonly GoogleTranslationProvider googleProvider;
        private readonly LlamaTranslationProvider llamaProvider;
        private readonly TranslationManager translationManager;
        private readonly ChatManager chatManager;
        private readonly WindowManager windowManager;

        private const string CommandName = "/translator";

        public Plugin()
        {
            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(PluginInterface);

            this.modelManager = new ModelManager(Log, PluginInterface.ConfigDirectory.FullName, PluginInterface.AssemblyLocation.DirectoryName!);
            this.googleProvider = new GoogleTranslationProvider(Log);
            this.llamaProvider = new LlamaTranslationProvider(Log, this.modelManager, this.Configuration);

            this.translationManager = new TranslationManager(Log, DataManager, this.Configuration, this.googleProvider, this.llamaProvider);
            
            this.windowManager = new WindowManager(PluginInterface, this.Configuration, this.translationManager);
            this.chatManager = new ChatManager(Chat, Log, this.translationManager, this.Configuration);

            _ = this.translationManager.InitializeModelAsync();

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Apre la configurazione del Dalamud Translator."
            });

            Chat.Print("Dalamud Translator Loaded (Refactored)!");
        }

        private void OnCommand(string command, string args)
        {
            this.windowManager.DrawConfigUI();
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(CommandName);
            
            this.chatManager.Dispose();
            this.windowManager.Dispose();
            this.translationManager.Dispose();
            this.llamaProvider.Dispose();
            this.googleProvider.Dispose();
        }
    }
}

