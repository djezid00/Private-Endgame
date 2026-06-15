# CLAUDE.md — Master Thesis: Tag Game with MA-POCA (Unity ML-Agents)

## Project Overview

This is a **Master's thesis project** implementing a competitive tag (pursuit-evasion) game using **Unity ML-Agents** with the **MA-POCA (Multi-Agent POsthumous Credit Assignment)** algorithm. The goal is to train a **ChaserAgent** (teamId=0) and a **RunnerAgent** (teamId=1) to play tag against each other, reaching the level of emergent behaviour demonstrated in the reference video "AI Learns to Play Tag (and breaks the game)" by AI Warehouse (https://www.youtube.com/watch?v=hCmrMOzx5VA ).
Also here are some usefull links: 
- https://github.com/Unity-Technologies/ml-agents/tree/develop

### Environment Setup
- **OS**: Windowswith **Conda** shell
- **Active conda env**: `mlagents`
- **ML-Agents**: Cloned repo (local), not pip-only install
- **Unity Editor**: Project with `TagArena` scene
- **Claude Code plugins active**: Playwright, Context7, Superpowers

---

## MANDATORY WORKFLOW — READ FIRST

### Step 0 — Always Start with Plan Mode
**Before touching any code**, enter Plan mode (`Shift+Tab` to toggle, or use `/plan`). Lay out:
1. What needs to be done
2. Which files will be affected
3. What the expected outcome is
4. Any risks or blockers

Only proceed to implementation after the plan is confirmed.

### Step 1 — Git: Commit All Existing Work First
Remote repository name: Private-Endgame
**Before making ANY changes**, run the following to snapshot current state:

```bash
cd <YOUR_UNITY_PROJECT_ROOT>
git status
git add -A
git commit -m "chore: snapshot existing work before Claude session $(date +%Y-%m-%d)"
git log --oneline -5
```

If git is not initialized:
```bash
git init
git add -A
git commit -m "feat: initial commit — tag game MA-POCA thesis project"
```

**Never skip this step.** Every session must begin with a clean git state.

---

## Project Architecture

```
<UnityProjectRoot>/
├── Assets/
│   ├── Scripts/
│   │   ├── TagAgent.cs           # Agent logic (Chaser + Runner shared)
│   │   └── TagArenaManager.cs    # Episode management, rewards, reset
│   ├── Prefabs/
│   │   ├── ChaserAgent.prefab    # teamId=0, red, starts at (-4, 1, 0)
│   │   ├── RunnerAgent.prefab    # teamId=1, blue, starts at (+4, 1, 0)
│   │   └── TagArena.prefab       # 20x20 floor + 4 walls + 2 agents
│   └── Scenes/
│       └── TagArena.unity
├── results/                      # Training outputs (auto-created by mlagents-learn)
├── CLAUDE.md                     # This file
└── .gitignore


├── C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/config/poca/
│   └── TagMApoca.yaml            # MA-POCA trainer config
```

---

## About that video
Here is the authors comment on how the models are trained.
More information about how Albert and Kai were trained:

Time it took to train AlbertKai:
Room 1: 12h 30m (though I stopped the recording after Albert broke the game)
Room 2: 13h 40m
Room 3: 1d 20h 2m
Final Battle: 6h 48m (this wasn’t shown but was needed since the agents weren’t used to seeing other teammates)

We continue training on top of the previous brains, meaning by the end of the video Albert and Kai both have trained for 3 days and 5 hours 


Thank you so much for watching! These short videos take literally hundreds of hours to make, if you want to help allow us to make them faster, please consider becoming a channel member! By becoming a member, your name can be in future videos, you can see behind-the-scenes things that don’t fit in the regular videos, you can also use stickers of Albert, Kai and some other characters our team made in comments AlbertKaiTyler□□ (more coming) :D


NOTES
When I mention it took x days to train, that’s in game time, and much larger than the displays indicate since there are 200 copies training simultaneously.

This is a very long comment going over more of the details of how Albert and Kai works, issues they’ve had, unexpected results etc.


THE BASICS: 
Albert and Kai were trained using reinforcement learning, meaning they were rewarded for doing things correctly and punished for doing them incorrectly (the reward is just increasing their score, and the punishment is decreasing it). After they finish each attempt, the actions they took are analyzed and the weights in their neural networks (brains) are adjusted using an algorithm called MA-POCA to try to prioritize the actions that led to the most reward. The agents start off making essentially random decisions until Kai accidentally tags Albert in the first room and is rewarded, then, as mentioned above, the weights in his neural network brain are adjusted in order to try to replicate that reward (it wasn’t this simple for this video since we use self-play to train both agents at the same time, more on that later). This leads to Kai learning that tagging Albert is good, and since Albert is punished when he’s tagged, it also leads to Albert learning that getting tagged by Kai isn’t good. This process continues through 10s of millions of steps until one of the agents consistently loses, or the agents are able to counter each other well enough to where it’s a draw.


REWARD FUNCTION: 
Albert and Kai are given two types of rewards, group rewards and individual rewards. When Albert gets tagged he’s punished by getting a -1 group reward and Kai is rewarded by getting a +1 group reward and vice versa, encouraging Kai to tag Albert, and Albert to avoid being tagged by Kai. Additionally, Albert is given an individual reward of 0.001 for each frame he’s alive (0.6 total in a room lasting 10s), and Kai -0.001, to encourage Kai to try to tag Albert as quickly as possible. When we introduce the grabbable cubes we also give Albert an individual reward of +1 the first time he picks up the cube to make sure Albert actually starts using the cube (since without this, the rewards were too infrequent for Albert to learn to use it effectively).


BRAIN: 
Albert and Kai’s brains are neural networks with 4 layers each (one input layer, 2 hidden layers and one output layer).

The agents collect information about the scene through direct values and raycasts. Every 5 frames they’re fed data about their position in the room, the opponent’s position, velocity, direction etc., and they also collect information through raycasts (a simplified version of eyes). The agent's eyes (raycasts) can differentiate between walls, ground, moveableObjects and Kai/Albert.

The agents' brains (neural networks) are given the data the agents collect from direct values and raycasts and use them to predict 4 numbers for the respective agent which control how that agent moves. An example of an output of one of the neural networks is: [1, 2, 0, 1], this would be interpreted as [1=move forward, 2=turn right, 0=don’t jump, 1=try to grab], so the agent being controlled by this neural network would try to move forward while turning right and grabbing.

The fact that we have two agents training simultaneously complicates things a bit, normally we’re able just update the agents brains every x steps, but if we did that for both brains at the same time then they would struggle developing multiple strategies, since reinforcement learning tends to be best at finding a single solution, that would lead to the winner dominating and the loser stuck doing the same strategy over and over. The way we tackle this issue is by using something called self-play. Since we use self-play, we technically only train one agent at a time, and swap which is being trained every 100k steps. When we’re training Albert, we use a recent model of Kai’s brain as his opponent, and to avoid there only being one strategy, we store 10 recent brains to use as opponents, swapping them out every couple thousand steps so that Albert learns to beat all of them and not just one. This results in a much more general AI that’s hard to exploit.


UNEXPECTED BEHAVIORS:
In room 1 Albert manages to break out of the room by exploiting a small hole in the hitbox near the top of the room, which was there because I didn’t make the hitboxes on the walls tall enough. Though Albert used it to escape, I’m not convinced he actually would learn to do it consistently. The challenge with this video is that it can be difficult to interpret the agent’s behaviors; Albert could be making certain unexpected moves as a way to exploit Kai’s poorly trained brain to get him to make bad moves, or Albert could just be making these unexpected moves because he hasn't trained enough. Albert was able to find the hole a few times, however he wasn’t able to do it consistently, this could be from either him not training long enough, his observations not making it easy to detect when he can jump out, or Kai quickly learning to counter him getting to the display in time.

In room 2 Albert also manages to glitch out of the room, and he was able to do this consistently. We made sure the cube grabbing functionality was coded as rigorously as possible, even with it automatically detaching the grab if the force exerted is too high, I couldn’t find a single way of exploiting it in testing, but Albert certainly didn’t have issues finding it.

Albert also had a couple moments of throwing the cubes at Kai and spinning with the cube to throw Kai out of the room, we didn’t even consider this being a possibility before training, AI’s able to come up with some really clever solutions to problems.


## Core Code Reference

### TagAgent.cs — Key Facts
- Inherits from `Unity.MLAgents.Agent`
- **teamId**: `0` = Chaser (pursues), `1` = Runner (evades)
- **Observation space**: 18 floats (vector) + RayPerceptionSensor3D (wall/agent raycasts)
  - Self: localPosition (3), linearVelocity (3), forward (3)
  - Opponent: relative position (3), linearVelocity (3), forward (3)
- **Action space**: 2 continuous actions — `[0]` = move (forward/back), `[1]` = turn (left/right)
- **Step rewards**: Chaser gets `-0.001f` per step, Runner gets `+0.001f` per step
- **Episode end**: Triggered via `TagArenaManager.OnAgentTagged()` or stalemate timeout
- **Heuristic**: WASD keys (w/s = move, a/d = turn) for manual play testing

### TagArenaManager.cs — Key Facts
- **Arena size**: 20×20 floor, 4 walls tagged "Wall"
- **arenaRadius**: 8f (spawn range)
- **maxEpisodeSteps**: 2000
- **Random spawn**: Chaser on X ∈ [-8, -1], Runner on X ∈ [+1, +8], both random Z ∈ [-8, 8]
- **Stalemate**: After 2000 steps → Chaser gets -1, Runner gets +1
- **Tag reward**: Chaser catches Runner → Chaser: `+1 + timeBonus`, Runner: `-1 - survivalBonus`
  - `timeBonus = clamp(1 - steps/maxSteps, 0, 0.5)` — reward faster catches
  - `survivalBonus = clamp(steps/maxSteps, 0, 0.5)` — penalize early catches

### TagMApoca.yaml — Trainer Config
```yaml
behaviors:
  TagMApoca:
    trainer_type: poca
    hyperparameters:
      batch_size: 2048           # was 256 — more stable gradients in non-stationary self-play
      buffer_size: 40960                # was 20480 — ~80x batch_size, more diverse experience per update
      learning_rate: 3.0e-4
      learning_rate_schedule: linear
      beta: 5.0e-3
      beta_schedule: constant
      epsilon: 0.2
      epsilon_schedule: linear
      lambd: 0.95
      num_epoch: 5               # was 3 — more passes per buffer when steps are the bottleneck
    network_settings:
      normalize: true
      hidden_units: 256          # was 128 — observation space doubled (9→18), network needs more capacity
      num_layers: 2
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 5000000           # was 500000 — self-play arms race needs ~10x more steps
    time_horizon: 256            # was 128 — agents need to plan multi-second pursuit/evasion sequences
    summary_freq: 20000          # was 10000 — reduces I/O overhead over the longer run
    checkpoint_interval: 250000  # was 50000 — proportional to new max_steps (20 checkpoints total)
    keep_checkpoints: 5

    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 50000
      swap_steps: 30000
      team_change: 200000
```

---

## Environment Activation

All terminal commands must be run in the **conda `mlagents` environment**:

```bash
conda activate mlagents
# Verify
python -m mlagents --version
mlagents-learn --help
```

---

## Training Commands

### Start a New Training Run
```bash
conda activate mlagents
cd <ML_AGENTS_REPO_ROOT>
mlagents-learn config/TagMApoca.yaml --run-id=TagRun_01 --train
```

### Resume a Training Run
```bash
mlagents-learn config/TagMApoca.yaml --run-id=TagRun_01 --resume
```

### Override Parameters (Quick Test)
```bash
mlagents-learn config/TagMApoca.yaml --run-id=TagTest_01 \
  --train \
  --torch-settings.max-steps=100000
```

### Monitor with TensorBoard
```bash
conda activate mlagents
tensorboard --logdir results/
# Open http://localhost:6006 in browser
```

### Training Milestones to Watch
| Steps | Expected Behaviour |
|-------|--------------------|
| 0–500k | Random / chaotic movement |
| 500k–1.5M | Basic pursuit/evasion emerges |
| 1.5M–3M | Cornering and wall-dodging strategies |
| 3M–5M | Emergent advanced tactics (target video level) |

---

## Unity Editor Workflow

1. Open the `TagArena` scene
2. Press **Play** — the scene waits for the Python trainer
3. In terminal (conda mlagents): run `mlagents-learn` command above
4. Unity connects automatically via port 5004
5. Agents begin training; watch TensorBoard for progress

### Inference (Testing Trained Model)
1. Export `.onnx` model from `results/TagRun_01/` 
2. Drag `.onnx` into the `Model` field on each agent's `Behavior Parameters`
3. Set **Behavior Type** to `Inference`
4. Press Play — agents run the trained policy

---

## Git Workflow (Per Session)

```bash
# Start of session
git status
git add -A
git commit -m "chore: pre-session snapshot"

# After meaningful work
git add -A
git commit -m "feat: <describe what changed>"

# After training run completes
git add results/TagRun_XX/  # optional — large; consider .gitignore
git commit -m "results: TagRun_XX — 5M steps, final checkpoint"
```

### Recommended .gitignore additions
```
results/*/
*.onnx
Library/
Temp/
obj/
Build/
Logs/
```

---

## Plugins & Tools Usage

### Context7
Use to look up Unity ML-Agents API documentation inline:
- Query: `mlagents Agent API`, `BehaviorParameters`, `VectorSensor`, `ActionBuffers`
- Query: `poca trainer configuration options`

### Playwright
Use for browser automation if needed:
- Scraping TensorBoard metrics
- Taking screenshots of training curves for thesis

### Superpowers
Use for long-running tasks:
- Running training scripts in background
- Batch file operations on result directories

---

## Known Issues & Gotchas

1. Agents are floating, and random spawning when learning is active. It is not like the video

---

## Final Goal

Train the Chaser and Runner agents to produce the **emergent adversarial behaviour** shown in the reference video "AI Learns to Play Tag (and breaks the game)" by AI Warehouse — sophisticated pursuit, evasion, corner-trapping, and wall-using strategies emerging purely from the reward signal over 5M steps of self-play MA-POCA training.

---

## Thesis Context

- **Author**: Computer Science Master's student, University of Split
- **Algorithm**: MA-POCA (Multi-Agent POsthumous Credit Assignment) via Unity ML-Agents
- **Game**: Competitive Tag — 1 Chaser (Team 0) vs 1 Runner (Team 1)
- **Arena**: 20×20 enclosed space, random spawns, continuous action space
- **Research question**: Does MA-POCA with self-play produce emergent pursuit-evasion strategies comparable to human-designed heuristics?
