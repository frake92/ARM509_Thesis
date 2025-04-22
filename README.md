
# ARM509 Szakdolgozat – Erdész Réka  
**Unity ML-Agents projekt: Három különböző tanítási rendszer összehasonlítása játékfejlesztésben**

## 📁 Projektstruktúra

A projekt három különböző tanítási rendszert tartalmaz, mindegyik külön mappában található:

### 1. Agent vs Agent (Self-Play)
- **Projektkód:** `AgentVsAgent/`
- **Modellek:**
  - *Full self-play modell:* `Model/ZolaRLAgentV4-fullSelfplay/`
  - *Kezdetleges intelligenciával tanított modell:* `Model/ZolaRLAgentV3-InitialIntelligence/`

### 2. Player vs Agent (Játékossal szembeni tanulás)
- **Projektkód:** `PlayerVsAgent/`
- **Modell:** `Model/ZolaRLAgentV3vsPlayer/`

### 3. Agent vs Hard-Coded AI
- **Projektkód és modell:** `Model/ZolaRLAgentV2vsArtint/`

---

## 🧠 Betanított modellek

A `Models/` mappában minden betanított modell megtalálható, tartalmaznak:
- Checkpoint fájlokat
- ONNX exportokat

A modellek újrataníthatók vagy közvetlenül kipróbálhatók.

---

## ⚙️ Tanítás lépései

### 1. Projekt importálása Unity-be
- Unity verzió: `6000.0.28f1` (telepítve a Unity Hubból)
- Lépések:
  - Unity Hub → `Add` → `Add project from disk`
  - Válaszd ki a kívánt mappát → `Add`

---

### 2. Virtuális környezet (venv) létrehozása
A projekt mappájában hozz létre két külön venv-et:

```bash
# ML-Agents környezet
python -m venv mlagents-env

# TensorBoard környezet
python -m venv tensorboard-env
```

### 3. Virtuális környezet aktiválása

```bash
# ML-Agents környezet
mlagents-env\Scripts\activate

# Telepítések
pip install mlagents
pip install torch torchvision torchaudio
```

---

## 🏋️‍♀️ Modell tanítása

### Alap parancs:

```bash
mlagents-learn --run-id DesiredRunID
```

> **Fontos:** A Unity Editorben az *Agent Behaviour Name* értéke **meg kell hogy egyezzen** a `--run-id` paraméterrel.

Ez a parancs létrehozza a `results/DesiredRunID/` mappát és egy `configuration.yaml` fájlt, ahol a hiperparamétereket testre lehet szabni.

### Folytatás szerkesztett konfiggal:

```bash
mlagents-learn results/DesiredRunID/configuration.yaml --resume
```

Ezután Unity-ben a **Play** gombbal indítható a szimuláció.

---

## 📊 TensorBoard használata

A tanulási folyamat vizualizálásához:

```bash
# TensorBoard környezet aktiválása
tensorboard-env\Scripts\activate

# Telepítés és indítás
pip install tensorboard
tensorboard --logdir results/DesiredRunID
```

---

## 🎯 Jutalmazási logika

A jutalmazási rendszer minden esetben a Zola Agent kódjában van definiálva, az adott projekt `Agent` osztályában. Ezt szükség esetén érdemes testre szabni a kívánt tanulási viselkedés eléréséhez.

---

## 📌 Összefoglalás

Ez a projekt a **Unity ML-Agents** keretrendszerre épül, és három különböző tanítási módszert vizsgál:
- Self-play
- Játékossal való tanulás
- Hard-coded AI elleni tanulás

Célja, hogy feltérképezze, melyik módszerrel tanított mesterséges intelligencia válik a legélvezetesebb és legintelligensebb ellenféllé a játékos számára.


