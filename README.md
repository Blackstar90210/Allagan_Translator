# Allagan Translator (Local AI & Online) 🌍
*A seamless, hybrid translation plugin for Final Fantasy XIV (via Dalamud).*
*Built with the support of Antigravity IDE.*

[🇮🇹 Leggi in Italiano](#italiano)

Welcome to **Allagan Translator (Local AI & Online)**, the ultimate translation overlay for FFXIV. Whether you're playing on international servers or simply want to read the game's rich lore in your native language, this plugin delivers lightning-fast, highly contextual translations directly to your screen.

## ✨ Features

* **Hybrid Translation Engine:** Choose between two powerful engines seamlessly through the configuration menu:
  * **Google Translate API:** Cloud-based, instant, and with absolutely zero impact on your CPU/GPU.
  * **Llama 3.2 3B (Local AI):** A completely offline, private AI model. The plugin automatically downloads the 2GB `.gguf` model and runs inference locally on your **CPU** for a seamless, private translation experience.
* **Plug-and-Play AI:** No manual setup, no Python scripts, no heavy dependencies. Select the local AI from the dropdown, and the plugin handles the download and CPU integration automatically.
* **Lumina Context Injection:** The translator isn't blind! It dynamically reads the game's internal database (Lumina) to detect active zone names, characters, and duties, forcing the AI to preserve FFXIV-specific proper nouns instead of ruining them with literal translations.
* **Custom User Glossary:** Define your own rules. Want "Healer" to stay "Healer" or translate specifically to your liking? Add it to the glossary and the engine will strictly obey.
* **Granular Chat Filters:** Choose exactly what gets translated. Filter by `Say`, `Yell`, `Shout`, `Party`, `Free Company`, `Tell`, or exclusively `NPC Dialogues` for MSQ immersion.
* **Immersive Overlay:** A clean, draggable ImGui overlay that mirrors the native game chat colors. Sender names are brilliantly highlighted so you can effortlessly keep track of who is talking during fast-paced cutscenes.

## 🚀 How to Use

### Installation
1. Open **FFXIV** and open the Dalamud Settings by typing `/xlsettings` in chat.
2. Go to the **Experimental** tab.
3. Scroll down to **Custom Plugin Repositories** and paste the following link:
   `https://raw.githubusercontent.com/Blackstar90210/Allagan_Translator/main/pluginmaster.json`
4. Click the **+** button to add it, then click **Save and Close**.
5. Open the Dalamud Plugin Installer, search for **Allagan Translator (Local AI & Online)** and click Install.

### Usage
1. Type `/translator` in the game chat to open the Configuration Menu.
2. Choose your target language and preferred engine.
   * *Note: If you select **Llama 3.2 3B**, the plugin will automatically start downloading the model (~2GB) in the background. The translation overlay will notify you when the download is complete and the AI is ready to use!*
3. Enjoy the MSQ in your language!

---
<a name="italiano"></a>

# Allagan Translator (Local AI & Online) 🌍
*Un plugin di traduzione ibrido e integrato per Final Fantasy XIV (tramite Dalamud).*
*Creato con il supporto di Antigravity IDE.*

Benvenuto in **Allagan Translator (Local AI & Online)**, l'overlay di traduzione definitivo per FFXIV. Che tu stia giocando su server internazionali o semplicemente desideri goderti la trama nella tua lingua madre, questo plugin offre traduzioni istantanee e super-contestualizzate direttamente a schermo.

## ✨ Funzionalità

* **Motore Ibrido:** Scegli tra due potenti motori di traduzione direttamente dal menu:
  * **Google Translate API:** Basato su cloud, immediato e con impatto zero sulle prestazioni del tuo PC.
  * **Llama 3.2 3B (Intelligenza Artificiale Locale):** Un modello IA completamente offline e privato. Il plugin scaricherà in automatico il modello da 2GB e gestirà il calcolo direttamente sulla tua **CPU**, garantendoti un'esperienza di traduzione fluida e privata.
* **IA "Plug-and-Play":** Nessuna configurazione manuale, niente Python, nessuna dipendenza esterna da installare. Seleziona l'IA dal menu a tendina e il plugin farà tutto da solo (download e inizializzazione sulla CPU).
* **Context Injection (Lumina):** Il traduttore sa a cosa stai giocando! Leggendo dinamicamente i dati di gioco tramite Lumina, riconosce i nomi delle zone, dei personaggi e dei Dungeon, forzando l'IA a mantenere i nomi propri di FFXIV in lingua originale (evitando traduzioni letterali ridicole).
* **Glossario Personale:** Detta le tue regole. Vuoi che la parola "Healer" rimanga intatta o venga tradotta in un modo specifico? Aggiungila al glossario e il motore ubbidirà.
* **Filtri Chat Granulari:** Scegli esattamente cosa tradurre. Attiva o disattiva i canali `Say`, `Yell`, `Shout`, `Party`, `Free Company`, `Tell` o isola esclusivamente i `Dialoghi degli NPC` per un'immersione totale nella Storia Principale.
* **Overlay Immersivo:** Un riquadro grafico pulito che rispetta fedelmente i colori nativi della chat di gioco. I nomi dei mittenti vengono isolati ed evidenziati in bianco brillante, permettendoti di seguire i dialoghi frenetici con un solo colpo d'occhio.

## 🚀 Come si usa

### Installazione
1. Avvia **FFXIV** e apri le Impostazioni di Dalamud scrivendo `/xlsettings` in chat.
2. Vai nella scheda **Experimental**.
3. Scorri fino a **Custom Plugin Repositories** e incolla questo link:
   `https://raw.githubusercontent.com/Blackstar90210/Allagan_Translator/main/pluginmaster.json`
4. Clicca il tasto **+** per aggiungerlo, poi fai clic su **Save and Close**.
5. Apri il Plugin Installer di Dalamud, cerca **Allagan Translator (Local AI & Online)** e installalo.

### Utilizzo
1. Scrivi `/translator` nella chat di gioco per aprire il Menu di Configurazione.
2. Scegli la tua lingua di destinazione e il motore che preferisci.
   * *Nota: Se selezioni **Llama 3.2 3B**, il plugin avvierà automaticamente il download del modello (~2GB) in background. L'overlay delle traduzioni ti avviserà non appena il download sarà completato e l'IA sarà pronta all'uso!*
3. Goditi la trama di FFXIV nella tua lingua!
