using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace DalamudTranslator
{
    public enum TranslationEngineType
    {
        GoogleCloudFree,
        LocalLlamaVulkan
    }

    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public TranslationEngineType TranslationEngine { get; set; } = TranslationEngineType.GoogleCloudFree;

        public string TargetLanguage { get; set; } = "it";

        public bool TranslateSay { get; set; } = true;
        public bool TranslateYell { get; set; } = true;
        public bool TranslateShout { get; set; } = true;
        public bool TranslateParty { get; set; } = true;
        public bool TranslateCrossParty { get; set; } = true;
        public bool TranslateAlliance { get; set; } = true;
        public bool TranslateFreeCompany { get; set; } = true;
        public bool TranslateTell { get; set; } = true;
        public bool TranslateNPCDialogue { get; set; } = true;

        public Dictionary<string, string> CustomGlossary { get; set; } = new Dictionary<string, string>();

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
