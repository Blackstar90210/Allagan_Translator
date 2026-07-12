using System;
using System.Numerics;
using System.Collections.Generic;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using AllaganTranslator.Services;

namespace AllaganTranslator.Windows
{
    public class TranslatorOverlay : Window
    {
        private readonly Configuration configuration;
        private readonly Action openConfigAction;
        
        // Lista che contiene lo storico delle traduzioni
        public List<TranslationMessage> TranslationHistory { get; } = new();
        private bool scrollToBottom = false;

        public void AddTranslation(TranslationMessage message)
        {
            this.TranslationHistory.Add(message);
            
            // Mantiene un massimo di 100 righe in memoria per non appesantire il plugin
            if (this.TranslationHistory.Count > 100)
                this.TranslationHistory.RemoveAt(0);

            this.scrollToBottom = true;
        }

        public TranslatorOverlay(Configuration configuration, Action openConfigAction) 
            : base("Allagan Translator (Local AI & Online)", ImGuiWindowFlags.None)
        {
            this.configuration = configuration;
            this.openConfigAction = openConfigAction;

            this.TitleBarButtons.Add(new TitleBarButton
            {
                Icon = Dalamud.Interface.FontAwesomeIcon.Cog,
                IconOffset = new Vector2(1, 1),
                Click = (button) => this.openConfigAction?.Invoke(),
                ShowTooltip = () => { ImGui.SetTooltip("Impostazioni"); }
            });
            
            // Posizione e dimensione di default al primo avvio
            this.PositionCondition = ImGuiCond.FirstUseEver;
            this.Position = new Vector2(200, 200);
            
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.Size = new Vector2(400, 150);
            
            this.IsOpen = true;
        }

        public override void Draw()
        {
            if (this.TranslationHistory.Count == 0)
            {
                ImGui.TextDisabled("In attesa di dialoghi...");
            }
            else
            {
                int renderedCount = 0;
                for (int i = 0; i < this.TranslationHistory.Count; i++)
                {
                    var msg = this.TranslationHistory[i];

                    bool shouldShow = msg.ChatType switch
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

                    if (!shouldShow) continue;
                    
                    renderedCount++;
                    var color = GetColorForChatType(msg.ChatType);
                    
                    if (!string.IsNullOrWhiteSpace(msg.Sender))
                    {
                        // Disegniamo il nome del mittente con un colore brillante/diverso per "evidenziarlo"
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); 
                        ImGui.Text($"{msg.Sender}:");
                        ImGui.PopStyleColor();

                        // Disegniamo il messaggio a capo con il suo colore di Chat e TextWrapped
                        ImGui.PushStyleColor(ImGuiCol.Text, color);
                        ImGui.TextWrapped(msg.TranslatedText);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, color);
                        ImGui.TextWrapped(msg.TranslatedText);
                        ImGui.PopStyleColor();
                    }
                    
                    ImGui.Separator();
                }

                if (renderedCount == 0)
                {
                    ImGui.TextDisabled("Nessun messaggio nei canali attivi...");
                }

                if (this.scrollToBottom)
                {
                    ImGui.SetScrollHereY(1.0f);
                    this.scrollToBottom = false;
                }
            }
        }

        private Vector4 GetColorForChatType(XivChatType type)
        {
            return type switch
            {
                XivChatType.Say => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                XivChatType.Yell => new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                XivChatType.Shout => new Vector4(1.0f, 0.4f, 0.0f, 1.0f),
                XivChatType.Party => new Vector4(0.4f, 0.7f, 1.0f, 1.0f),
                XivChatType.CrossParty => new Vector4(0.4f, 0.7f, 1.0f, 1.0f),
                XivChatType.Alliance => new Vector4(1.0f, 0.5f, 0.0f, 1.0f),
                XivChatType.FreeCompany => new Vector4(0.6f, 0.8f, 1.0f, 1.0f),
                XivChatType.TellIncoming => new Vector4(1.0f, 0.4f, 0.8f, 1.0f),
                XivChatType.NPCDialogue => new Vector4(0.6f, 1.0f, 0.6f, 1.0f),
                XivChatType.NPCDialogueAnnouncements => new Vector4(0.6f, 1.0f, 0.6f, 1.0f),
                _ => new Vector4(0.8f, 0.8f, 0.8f, 1.0f)
            };
        }
    }
}

