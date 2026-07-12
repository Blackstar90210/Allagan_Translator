using System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using AllaganTranslator.Services;

namespace AllaganTranslator.Windows
{
    public class WindowManager : IDisposable
    {
        private readonly IDalamudPluginInterface pluginInterface;
        public WindowSystem WindowSystem { get; init; }
        public ConfigWindow ConfigWindow { get; init; }
        public TranslatorOverlay TranslatorOverlay { get; init; }

        public WindowManager(IDalamudPluginInterface pluginInterface, Configuration configuration, TranslationManager translationManager)
        {
            this.pluginInterface = pluginInterface;
            
            this.WindowSystem = new WindowSystem("Allagan Translator");
            
            this.ConfigWindow = new ConfigWindow(configuration, translationManager);
            this.TranslatorOverlay = new TranslatorOverlay(configuration, DrawConfigUI);

            this.WindowSystem.AddWindow(this.ConfigWindow);
            this.WindowSystem.AddWindow(this.TranslatorOverlay);

            this.pluginInterface.UiBuilder.Draw += DrawUI;
            this.pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.pluginInterface.UiBuilder.OpenMainUi += DrawMainUI;

            translationManager.OnTranslationFinished += (msg) => 
            {
                this.TranslatorOverlay.AddTranslation(msg);
            };
        }

        public void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            this.ConfigWindow.IsOpen = true;
        }

        public void DrawMainUI()
        {
            this.TranslatorOverlay.IsOpen = true;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.ConfigWindow.Dispose();
            
            this.pluginInterface.UiBuilder.Draw -= DrawUI;
            this.pluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            this.pluginInterface.UiBuilder.OpenMainUi -= DrawMainUI;
        }
    }
}

