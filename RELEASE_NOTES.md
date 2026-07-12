## [1.0.0.22] - Hotfix: Bug Traduzione Llama Locale / Local Llama Translation Bug

### 🇮🇹 Italiano
- **[Bug Fix]** Risolto un problema critico introdotto nell'ultima patch in cui le IA locali (Llama) smettevano di tradurre, causato dal modo in cui il codice lingua "it" veniva interpretato nel prompt come la parola inglese "it" al posto di "Italian".

---

### 🇬🇧 English
- **[Bug Fix]** Fixed a critical issue introduced in the last patch where local AI models (Llama) completely stopped translating. This was caused by the language code "it" being literally interpreted as the English pronoun "it" in the prompt instead of "Italian".

---

## [1.0.0.21] - Aggiornamento UI: Menu a tendina per la lingua / UI Update: Language Dropdown

### 🇮🇹 Italiano
- **[UI]** Sostituita la casella di testo "Lingua di destinazione" con un comodo menu a tendina.
- **[UI]** Impostata l'opzione "Italiana" come predefinita.

---

### 🇬🇧 English
- **[UI]** Replaced the "Target Language" text box with a convenient dropdown menu.
- **[UI]** Set the "Italiana" option as default.

---

## [1.0.0.20] - Bug Fix: Download Modelli Multipli

### 🇮🇹 Italiano
- **[Bug Fix]** Risolto un problema critico che impediva il download del secondo modello se ne era già stato scaricato un altro in precedenza. (Il pulsante "Avvia Download Modello" falliva silenziosamente a causa di un'eccezione nella registrazione dei log nativi).

---

### 🇬🇧 English
- **[Bug Fix]** Fixed a critical issue preventing the download of the second model if another model had already been downloaded. (The "Start Model Download" button would silently fail due to an exception during native log registration).

---

## [1.0.0.18] - Refactoring Modelli & Download

### 🇮🇹 Italiano
- Rimosso completamente il supporto GPU/Vulkan a causa di instabilità con Dalamud
- Declassato il modello Llama 3.1 8B a esecuzione esclusivamente CPU (AVX2)
- Implementato un sistema di avvisi esplicito sul peso computazionale del modello 8B
- Aggiunto il download manuale del modello: i file non verranno più scaricati in background all'avvio, ma richiederanno l'azione esplicita dell'utente
- Corretto bug logico che impediva il corretto caricamento di un nuovo modello se ne era già stato caricato uno in precedenza
- Ottimizzate le dimensioni del file `.zip` (librerie Vulkan scartate)
- Ottimizzazione prestazionale: StringBuilder per l'inferenza e Regex pre-compilate con caching

* **[Feature]** Mantenuto il modello leggero **Llama 3.2 3B** (su CPU) per computer meno performanti.
* **[Miglioramento]** Nuove opzioni nel menù per lo switch a caldo tra IA Cloud e IA Locale CPU (3B o 8B).
* **[Miglioramento]** Parametri di generazione ("Temperature" e "Top-P") ottimizzati per impedire all'IA di "inventare" frasi non presenti nell'originale.

---

### 🇬🇧 English
- Completely removed GPU/Vulkan support due to instability issues with Dalamud
- Downgraded the Llama 3.1 8B model to CPU-only execution (AVX2)
- Implemented an explicit warning system regarding the computational weight of the 8B model
- Added manual model download: files will no longer download in the background at startup, but will require explicit user action
- Fixed a logic bug that prevented the correct loading of a new model if one was already previously loaded
- Optimized `.zip` file size (discarded Vulkan libraries)
- Performance optimization: StringBuilder for inference and pre-compiled Regex with caching

* **[Feature]** Kept the lightweight **Llama 3.2 3B** model (on CPU) for less powerful computers.
* **[Improvement]** New menu options for hot-swapping between Cloud AI and Local CPU AI (3B or 8B).
* **[Improvement]** Generation parameters ("Temperature" and "Top-P") optimized to prevent the AI from "inventing" phrases not present in the original text.
