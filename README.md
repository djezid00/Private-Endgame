# TagMApoca V1

> Branch: `david_first_iteration`  
> Last updated: 18.5.2026

## 📖 Project Overview
ANALYSIS OF COMPETITIVE INTERACTION IN VIDEO GAMES USING MULTI-AGENT MACHINE LEARNING

Develop a system of agents using a video game development environment and RML techniques to simulate an interactive game of tag. Implement a self-playing training method, and understand the dynamics of strategy development in a closed system without direct human supervision. The focus is on studying how two opposing agents force each other to constantly evolve and find new, unexpected tactics that would ultimately result in victory, i.e. maximizing their own reward.

## 🧠 ML-Agents Setup
- Unity version: `6000.4.0f1`
- ML-Agents package version: `Release 23`
- Python version: `3.12.4`

## 🚀 Getting Started

### Prerequisites
- Unity Hub + Unity Editor
- Python 3.12.4
- `mlagents` Python package available from https://github.com/Unity-Technologies/ml-agents 

### Installation

Step 1. Follow instructions from https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Installation.html

Step 2. Place provided (via this branch) TagMApoca.yaml file into your cloned ml-agents folder to config e.g C:\Users\ml-agents\config\poca

Step 3. Open your Conda prompt and activate your ml-agents environment

Step 4. Open your Unity project 

Step 5. Click on your Chaser Agent, in Inspector make sure on "Behaviour Parameters" Model is not assigned.

Step 4. Run training with:

```bash
mlagents-learn config/your_config.yaml --run-id=run1
```
After command, in your Unity IDE, click play.
- NOTE: if you interrupt your training, add --resume tag
- NOTE: if you are not satisfied with results, after your changes, run command with DIFFERENT --run-id=...

Step 5. After you are satisfied with your trained model, you can assign it to your Chaser/Runner agent via "Behaviour Parameters", drag and drop your trained model.
