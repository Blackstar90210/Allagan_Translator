# Allagan Translator (Local AI & Online) 🌍
*A seamless, hybrid translation plugin for Final Fantasy XIV (via Dalamud).*
*Built with the support of Antigravity IDE.*

[🇮🇹 Leggi in Italiano](#italiano)

Welcome to **Allagan Translator (Local AI & Online)**, the ultimate translation overlay for FFXIV. Whether you're playing on international servers or simply want to read the game's rich lore in your native language, this plugin delivers lightning-fast, highly contextual translations directly to your screen.

## ✨ Features

* **Hybrid Translation Engine:** Choose between powerful engines seamlessly through the configuration menu:
  * **Google Translate API:** Cloud-based, instant, and with absolutely zero impact on your CPU.
  * **Llama 3.2 3B (Local AI — CPU):** A completely offline, private AI model (~2.1 GB). Lightweight and fast, ideal for most hardware.
  * **Llama 3.1 8B (Local AI — CPU):** A larger, more accurate model (~4.9 GB). Delivers superior translation quality but requires significantly more processing power and longer inference times.
* **Manual Download Control:** When you select a local AI model, the plugin shows you the expected file size and a dedicated **"Start Model Download"** button. No surprise background downloads — you decide when to download.
* **Lumina Context Injection:** The translator isn't blind! It dynamically reads the game's internal database (Lumina) to detect active zone names, characters, and duties, forcing the AI to preserve FFXIV-specific proper nouns instead of ruining them with literal translations.
* **Custom User Glossary:** Define your own rules. Want "Healer" to stay "Healer" or translate specifically to your liking? Add it to the glossary and the engine will strictly obey.
* **Granular Chat Filters:** Choose exactly what gets translated. Filter by `Say`, `Yell`, `Shout`, `Party`, `Free Company`, `Tell`, or exclusively `NPC Dialogues` for MSQ immersion.
* **Immersive Overlay:** A clean, draggable ImGui overlay that mirrors the native game chat colors. Sender names are brilliantly highlighted so you can effortlessly keep track of who is talking during fast-paced cutscenes.

## 🚀 How to Use

### Installation
1. Open **FFXIV** and open the Dalamud Settings by typing `/xlsettings` in chat.
2. Go to the **Experimental** tab.
3. Scroll down to **Custom Plugin Repositories** and paste the following link:
   `https://raw.githubusercontent.com/Blackstar90210/Allagan_Translator_Test/main/pluginmaster.json`
4. Click the **+** button to add it, then click **Save and Close**.
5. Open the Dalamud Plugin Installer, search for **Allagan Translator (Local AI & Online)** and click Install.

### Usage
1. Type `/translator` in the game chat to open the Configuration Menu.
2. Choose your target language and preferred engine.
   * *Note: If you select a local AI model (Llama 3.2 3B or Llama 3.1 8B), the plugin will show a download button with the expected file size. Press the button to start the download. The translation overlay will notify you when the model is ready to use!*
3. Enjoy the MSQ in your language!

---
<a name="italiano"></a>

# Allagan Translator (Local AI & Online) 🌍
*Un plugin di traduzione ibrido e integrato per Final Fantasy XIV (tramite Dalamud).*
*Creato con il supporto di Antigravity IDE.*

Benvenuto in **Allagan Translator (Local AI & Online)**, l'overlay di traduzione definitivo per FFXIV. Che tu stia giocando su server internazionali o semplicemente desideri goderti la trama nella tua lingua madre, questo plugin offre traduzioni istantanee e super-contestualizzate direttamente a schermo.

## ✨ Funzionalità

* **Motore Ibrido:** Scegli tra più motori di traduzione direttamente dal menu:
  * **Google Translate API:** Basato su cloud, immediato e con impatto zero sulle prestazioni del tuo PC.
  * **Llama 3.2 3B (IA Locale — CPU):** Un modello IA completamente offline e privato (~2.1 GB). Leggero e veloce, adatto alla maggior parte dei PC.
  * **Llama 3.1 8B (IA Locale — CPU):** Un modello più grande e preciso (~4.9 GB). Offre traduzioni di qualità superiore, ma richiede molta più potenza di calcolo e tempi di elaborazione più lunghi.
* **Download Manuale:** Quando selezioni un modello locale, il plugin ti mostra la dimensione attesa del file e un pulsante dedicato **"Avvia Download Modello"**. Nessun download a sorpresa in background: decidi tu quando scaricare.
* **Context Injection (Lumina):** Il traduttore sa a cosa stai giocando! Leggendo dinamicamente i dati di gioco tramite Lumina, riconosce i nomi delle zone, dei personaggi e dei Dungeon, forzando l'IA a mantenere i nomi propri di FFXIV in lingua originale (evitando traduzioni letterali ridicole).
* **Glossario Personale:** Detta le tue regole. Vuoi che la parola "Healer" rimanga intatta o venga tradotta in un modo specifico? Aggiungila al glossario e il motore ubbidirà.
* **Filtri Chat Granulari:** Scegli esattamente cosa tradurre. Attiva o disattiva i canali `Say`, `Yell`, `Shout`, `Party`, `Free Company`, `Tell` o isola esclusivamente i `Dialoghi degli NPC` per un'immersione totale nella Storia Principale.
* **Overlay Immersivo:** Un riquadro grafico pulito che rispetta fedelmente i colori nativi della chat di gioco. I nomi dei mittenti vengono isolati ed evidenziati in bianco brillante, permettendoti di seguire i dialoghi frenetici con un solo colpo d'occhio.

## 🚀 Come si usa

### Installazione
1. Avvia **FFXIV** e apri le Impostazioni di Dalamud scrivendo `/xlsettings` in chat.
2. Vai nella scheda **Experimental**.
3. Scorri fino a **Custom Plugin Repositories** e incolla questo link:
   `https://raw.githubusercontent.com/Blackstar90210/Allagan_Translator_Test/main/pluginmaster.json`
4. Clicca il tasto **+** per aggiungerlo, poi fai clic su **Save and Close**.
5. Apri il Plugin Installer di Dalamud, cerca **Allagan Translator (Local AI & Online)** e installalo.

### Utilizzo
1. Scrivi `/translator` nella chat di gioco per aprire il Menu di Configurazione.
2. Scegli la tua lingua di destinazione e il motore che preferisci.
   * *Nota: Se selezioni un modello locale (Llama 3.2 3B o Llama 3.1 8B), il plugin mostrerà un pulsante di download con la dimensione attesa del file. Premi il pulsante per avviare il download. L'overlay delle traduzioni ti avviserà non appena il modello sarà pronto all'uso!*
3. Goditi la trama di FFXIV nella tua lingua!
