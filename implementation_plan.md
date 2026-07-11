# Dalamud Translator Plugin - Implementation Plan (A Fasi)

Questo piano descrive la creazione da zero del plugin Dalamud divisa per fasi di importanza, partendo dalle fondamenta fino ad arrivare alle rifiniture.

## User Review Required

> [!IMPORTANT]
> Le **Fasi 1, 2 e 3** risultano completate.
> Procederemo alla **Fase 4 (Integrazione Motore di Traduzione)** mantenendo l'approccio originale basato su modello locale tramite `Microsoft.ML.OnnxRuntime`. Nessuna chiamata esterna o API!

## Fasi di Implementazione

### Fase 1: Fondamenta del Progetto (Completata)
**Obiettivo:** Avere un plugin base (vuoto ma funzionante) che Dalamud riesce a caricare.

### Fase 2: Intercettazione della Chat (Completata)
**Obiettivo:** Permettere al plugin di leggere cosa viene scritto in gioco.
- Iscrizione all'evento dei messaggi di chat, filtraggio dei tipi interessati.

### Fase 3: Interfaccia Utente (UI) (Completata)
**Obiettivo:** Mostrare qualcosa a schermo.
- Integrazione di `ImGuiNET` completata (`ConfigWindow` e `TranslatorOverlay`).

### Fase 4: Integrazione del Motore di Traduzione (ONNX) (IN CORSO)
**Obiettivo:** Trasformare il testo inglese/giapponese in italiano, offline.
- Aggiunta della libreria `Microsoft.ML.OnnxRuntime` via NuGet.
- Creazione della logica per il caricamento o download automatico dei modelli linguistici nella cartella di configurazione.
- Inizializzazione del modello in memoria.
- Processamento delle frasi catturate e restituzione della frase italiana da mostrare nell'Overlay.

### Fase 5: Ottimizzazione e Multithreading
**Obiettivo:** Evitare che il gioco "scatti" mentre traduce.
- Esecuzione dell'inferenza ONNX in background worker (Task asincroni) per non bloccare il rendering (UI thread).

### Fase 6: Rifiniture e Polish (Il Tocco Finale)
**Obiettivo:** Rendere il plugin pronto e bello per la condivisione.
- Aggiunta di personalizzazioni visive nella configurazione.
- Resa dell'Overlay completamente invisibile/non cliccabile (aggiunta flag `ImGuiWindowFlags.NoInputs`).

## Proposed Changes

### Motore Traduzione
#### [NEW] `TranslationService.cs`
- Creazione della classe che caricherà la sessione ONNX e gestirà la pipeline di traduzione. Sarà iniettato e utilizzato in `Plugin.cs`.

### Interfaccia Utente
#### [MODIFY] [TranslatorOverlay.cs](file:///c:/Programmazione/DalamudTranslator/Windows/TranslatorOverlay.cs)
- Aggiungere eventuali flags (`ImGuiWindowFlags.NoInputs`) per rendere l'overlay click-through, se necessario.

## Verification Plan

Al termine della Fase 4, compileremo il plugin e lo testeremo all'interno del gioco. Controlleremo i log per verificare che la sessione ONNX venga istanziata correttamente e che l'overlay si aggiorni in tempo reale senza cali di framerate.
