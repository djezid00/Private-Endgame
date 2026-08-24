# Tag with MA-POCA — emergent pursuit and evasion in Unity ML-Agents

A master's thesis project (FESB, University of Split) studying **competitive multi-agent
reinforcement learning** in a game of tag. A *chaser* and a *runner* are trained against each other
with self-play, using Unity ML-Agents, and the resulting behaviour is characterised across
**29 training runs of 5 million steps each** (≈ 145 M simulation steps, ≈ 132 h of compute).

The original goal was to reproduce the emergent tag behaviour shown in AI Warehouse's video
*"AI Learns to Play Tag (and breaks the game)"*. It became a pre-registered experimental programme
about **when and why** that behaviour appears — and when it doesn't.

---

## Findings

Full derivations, tables and figures are in [`docs/Theory.md`](docs/Theory.md).

**1. Emergence needs no reward shaping.** A pure terminal reward (±1 at the end of the episode)
is enough for decisive pursuit to emerge: catch rate ≈ 1.00 and an ELO gap above 1200 points, in
every seed.

**2. Potential-based shaping is a trap — in practice, not in theory.** Ng et al. (1999) prove that
potential-based shaping leaves the *optimal policy* unchanged. It does not leave the *learning
trajectory* unchanged: the shaped chaser collapsed into reward farming — group reward ≈ −1 and a
catch rate of ≈ 0.01, while its individual reward stayed high. Two separable causes were isolated:
the reward-delivery channel, and the algorithm's own susceptibility.

**3. A short-budget experiment gave the opposite answer to the full one.** At 400 k steps the
shaped arm looked better. At 5 M steps the ranking inverted completely. Conclusions about reward
design drawn on small step budgets are not reliable.

**4. γ = 0.99 has an empirical justification, not just a conventional one.** A five-point sweep
found a myopia penalty below it, an instability risk above it, and the fastest learning at it.

**5. Randomising the obstacle layout changed nothing.** The pre-registered prediction that random
layouts would learn slower and finish worse was falsified on both halves — which also rules out the
"it just memorised the arena" objection.

**6. MA-POCA at group size 1 is PPO in disguise.** Derived from the trainer source and confirmed by
measurement: with no groupmates, the counterfactual baseline receives the same arguments as the
value function, making MA-POCA equivalent to PPO up to a single hyperparameter. The measured
`Baseline Loss ÷ Value Loss` ratio is **1.002–1.006** in every 1v1 run, minimum exactly 1.000. The
baseline only becomes active with teams (2v2: no overlap with 1v1 across twelve measurements) —
but at 2v2 that activation does **not** produce a better game outcome.

**7. Emergence is real but not reliable.** In the 2v2 phase, **one run in three never left the
ground** — the chaser never discovered a single catch. Shown to be caused by self-play training
dynamics, not by unlucky arena geometry: seed 1 collapsed under MA-POCA but learned under PPO,
while seed 2 did the reverse, on identical layouts.

---

## Repository layout

```
Assets/
  Scripts/
    TagAgent.cs              # observations (18-float vector + raycasts + BufferSensor), actions
    TagArenaManager.cs       # episode clock, reward delivery, arena reset, custom TensorBoard stats
    TeamManager.cs           # activates N chasers / N runners from environment parameters
    ObstacleManager.cs       # pillar placement, fixed or randomised per episode
    Reward/
      TagReward.cs           # reward mathematics, isolated for unit testing
      ObstaclePlacement.cs   # rejection sampling for obstacle layouts
      SpawnPlacement.cs      # N-agent spawn placement with minimum separation
  Tests/EditMode/            # 33 unit tests over reward, obstacle and spawn logic
  Prefabs/                   # ChaserAgent, RunnerAgent, TagArena
  Models/                    # trained policies (.onnx), ready for inference in the Editor
experiments/
  configs/                   # archived trainer configs, one per experiment (reproducibility)
  analysis/parse_tb.py       # dependency-free TensorBoard scalar extractor
  analysis/plot_gamma.py     # figure generation
  run_*.bat                  # batch runners, with preflight checks
docs/
  Theory.md                  # the scientific record: derivations, results, pre-registered predictions
  progress.md                # session log, newest first
  figures/                   # exported result figures
```

## Environment

| Component | Version |
|---|---|
| Unity Editor | 6000.4.0f1 |
| ML-Agents (Unity package) | `com.unity.ml-agents` from a local clone of the `develop` branch |
| ML-Agents (Python) | 1.2.0.dev0 |
| Python | 3.10.12 (conda env `mlagents`) |
| PyTorch | 2.11.0 — CPU build for 1v1, CUDA 12.6 build for the 2v2 phase |

> **Note for anyone cloning this repository:** `Packages/manifest.json` references the ML-Agents
> package by an **absolute local path** to a clone of the ML-Agents repository. You must clone
> ML-Agents yourself and repoint that entry, or install the package from the registry, before the
> project will open.

## Reproducing a training run

```bash
conda activate mlagents
cd <your ml-agents clone>
mlagents-learn config/poca/TagMApoca_sparse.yaml --run-id=my_run --seed 1
```

Then press **Play** in the Unity Editor when the trainer asks for it. For the long runs, build a
headless player and pass it directly, which roughly doubles throughput:

```bash
mlagents-learn config/poca/TagMApoca_sparse.yaml --env=Build/TagMApoca_V1.exe \
    --no-graphics --run-id=my_run --seed 1
```

Monitor with `tensorboard --logdir results/`. To inspect results without TensorBoard, use
`experiments/analysis/parse_tb.py`, which reads the event files directly and has no dependencies.

Configurations differ only in **environment parameters**, so one build covers every experiment:
`distance_shaping_coef`, `shaping_gamma`, `num_obstacles`, `obstacle_layout`, `num_chasers`,
`num_runners`, `individual_terminal_reward`. All default to the 1v1 sparse baseline.

### Running trained policies

Drag any `.onnx` file from `Assets/Models/` onto the agent's **Behavior Parameters → Model**, set
**Behavior Type** to *Inference*, and press Play.

---

## Acknowledgements and third-party work

**Unity ML-Agents Toolkit** — the training framework, the MA-POCA and PPO implementations, and the
`SimpleMultiAgentGroup` / `BufferSensorComponent` APIs this project builds on. Developed by Unity
Technologies and released under the Apache License 2.0. This repository does not redistribute any
ML-Agents source; it depends on it.
<https://github.com/Unity-Technologies/ml-agents>

**AI Warehouse** — the video *"AI Learns to Play Tag (and breaks the game)"*
(<https://www.youtube.com/watch?v=hCmrMOzx5VA>) motivated this work and set the behavioural target
it was measured against. No code, assets, or footage from that project are used or reproduced here;
the environment, agents, reward design and experiments are original work.

**Academic references** for the methods used:

- Cohen, A. et al. (2022). *On the Use and Misuse of Absorbing States in Multi-agent Reinforcement
  Learning.* — MA-POCA. <https://arxiv.org/abs/2111.05992>
- Schulman, J. et al. (2017). *Proximal Policy Optimization Algorithms.*
  <https://arxiv.org/abs/1707.06347>
- Ng, A. Y., Harada, D., Russell, S. (1999). *Policy Invariance Under Reward Transformations.*
- Juliani, A. et al. (2018). *Unity: A General Platform for Intelligent Agents.*
  <https://arxiv.org/abs/1809.02627>

## Licence

Code and configurations in this repository are released under the MIT Licence (see `LICENSE`).
Trained model weights in `Assets/Models/` are released under the same terms. Figures and the
contents of `docs/` are © David Jezidžić and may be reused with attribution (CC BY 4.0).

The thesis manuscript itself is not part of this repository.

## Citing

> Jezidžić, D. (2026). *Analysis of Competitive Interaction in Video Games Using Multi-Agent
> Machine Learning.* Master's thesis, Faculty of Electrical Engineering, Mechanical Engineering and
> Naval Architecture (FESB), University of Split.
