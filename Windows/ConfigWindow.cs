using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGui;
using DalamudTranslator.Services;

namespace DalamudTranslator.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        private readonly Configuration configuration;
        private readonly TranslationService translationService;
        private float downloadProgress = 0.0f;
        
        private string newGlossaryOriginal = "";
        private string newGlossaryTranslation = "";

        public ConfigWindow(Configuration configuration, TranslationService translationService) 
            : base("Allagan Translator (Local AI & Online) - Configurazione", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.configuration = configuration;
            this.translationService = translationService;
            this.Size = new Vector2(400, 450);
            this.SizeCondition = ImGuiCond.FirstUseEver;

            this.translationService.OnDownloadProgress += (progress) => 
            {
                this.downloadProgress = progress;
            };
        }

        public void Dispose() { }

        public override void Draw()
        {
            ImGui.Text("Impostazioni Traduttore");
            ImGui.Separator();
            ImGui.Spacing();

            var targetLanguage = this.configuration.TargetLanguage;
            if (ImGui.InputText("Lingua di destinazione", ref targetLanguage, 10))
            {
                this.configuration.TargetLanguage = targetLanguage;
                this.configuration.Save();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Motore di Traduzione:");
            
            var currentEngine = (int)this.configuration.TranslationEngine;
            var engineNames = new[] { "Google Translate API (Cloud, Gratis)", "Llama 3.2 3B (Locale, CPU)" };
            if (ImGui.Combo("##EngineCombo", ref currentEngine, engineNames, engineNames.Length))
            {
                this.configuration.TranslationEngine = (TranslationEngineType)currentEngine;
                this.configuration.Save();
                _ = this.translationService.InitializeModelAsync();
            }

            if (this.configuration.TranslationEngine == TranslationEngineType.LocalLlamaVulkan)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                ImGui.TextWrapped("Nota: l'IA locale elabora il testo sul tuo PC. La traduzione potrebbe richiedere qualche istante e la sua velocità dipende esclusivamente dalla potenza del tuo processore.");
                ImGui.PopStyleColor();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Filtri Canali Chat:");
            
            bool changed = false;
            
            bool say = this.configuration.TranslateSay;
            if (ImGui.Checkbox("Say", ref say)) { this.configuration.TranslateSay = say; changed = true; }
            ImGui.SameLine();
            bool yell = this.configuration.TranslateYell;
            if (ImGui.Checkbox("Yell", ref yell)) { this.configuration.TranslateYell = yell; changed = true; }
            ImGui.SameLine();
            bool shout = this.configuration.TranslateShout;
            if (ImGui.Checkbox("Shout", ref shout)) { this.configuration.TranslateShout = shout; changed = true; }
            
            bool party = this.configuration.TranslateParty;
            if (ImGui.Checkbox("Party", ref party)) { this.configuration.TranslateParty = party; changed = true; }
            ImGui.SameLine();
            bool cross = this.configuration.TranslateCrossParty;
            if (ImGui.Checkbox("Cross-World Party", ref cross)) { this.configuration.TranslateCrossParty = cross; changed = true; }
            
            bool alliance = this.configuration.TranslateAlliance;
            if (ImGui.Checkbox("Alliance", ref alliance)) { this.configuration.TranslateAlliance = alliance; changed = true; }
            ImGui.SameLine();
            bool fc = this.configuration.TranslateFreeCompany;
            if (ImGui.Checkbox("Free Company", ref fc)) { this.configuration.TranslateFreeCompany = fc; changed = true; }
            
            bool tell = this.configuration.TranslateTell;
            if (ImGui.Checkbox("Tell", ref tell)) { this.configuration.TranslateTell = tell; changed = true; }
            ImGui.SameLine();
            bool npc = this.configuration.TranslateNPCDialogue;
            if (ImGui.Checkbox("NPC Dialogues", ref npc)) { this.configuration.TranslateNPCDialogue = npc; changed = true; }

            if (changed)
            {
                this.configuration.Save();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Glossario Personale:");
            ImGui.TextWrapped("Le parole inserite qui verranno tradotte rigorosamente come deciso da te (o lasciate in inglese se la traduzione è vuota).");
            
            ImGui.InputText("Termine Originale (EN)", ref this.newGlossaryOriginal, 50);
            ImGui.InputText("Traduzione o Forza Inglese", ref this.newGlossaryTranslation, 50);
            
            if (ImGui.Button("Aggiungi Termine"))
            {
                if (!string.IsNullOrWhiteSpace(this.newGlossaryOriginal))
                {
                    this.configuration.CustomGlossary[this.newGlossaryOriginal.Trim()] = this.newGlossaryTranslation.Trim();
                    this.configuration.Save();
                    this.newGlossaryOriginal = "";
                    this.newGlossaryTranslation = "";
                }
            }

            ImGui.Spacing();
            if (this.configuration.CustomGlossary.Count > 0)
            {
                if (ImGui.BeginTable("glossaryTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Originale");
                    ImGui.TableSetupColumn("Traduzione");
                    ImGui.TableSetupColumn("Azione", ImGuiTableColumnFlags.WidthFixed, 60);
                    ImGui.TableHeadersRow();

                    string? toRemove = null;
                    foreach (var kvp in this.configuration.CustomGlossary)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(kvp.Key);
                        ImGui.TableNextColumn();
                        ImGui.Text(kvp.Value);
                        ImGui.TableNextColumn();
                        if (ImGui.Button($"Rimuovi##{kvp.Key}"))
                        {
                            toRemove = kvp.Key;
                        }
                    }
                    ImGui.EndTable();

                    if (toRemove != null)
                    {
                        this.configuration.CustomGlossary.Remove(toRemove);
                        this.configuration.Save();
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            if (this.configuration.TranslationEngine == TranslationEngineType.LocalLlamaVulkan)
            {
                ImGui.Text("Stato Motore di Traduzione (Llama 3.2 3B - CPU):");
                
                if (this.translationService.IsDownloading)
                {
                    ImGui.TextColored(new Vector4(1, 1, 0, 1), "Scaricamento in corso (Circa 2 GB)...");
                    ImGui.ProgressBar(this.downloadProgress, new Vector2(-1, 0));
                }
                else if (this.translationService.IsReady)
                {
                    ImGui.TextColored(new Vector4(0, 1, 0, 1), "Modello caricato nella CPU e pronto!");
                }
                else
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "In attesa di inizializzazione...");
                }
            }
            else
            {
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "Google Translate API Connesso e Pronto.");
            }
        }
    }
}
