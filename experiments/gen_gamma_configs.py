"""Generate the obstacles x gamma sweep trainer configs (spec 2026-07-04).

Writes each YAML to the ml-agents config dir AND archives a copy in
experiments/configs/. Diffs vs TagMApoca_{sparse,shaped}_5M.yaml are ONLY:
gamma (both behavior blocks), shaping_gamma, and the obstacle env-params.
Run:  python experiments/gen_gamma_configs.py
"""
import os

MLAGENTS_CFG = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca"
ARCHIVE      = os.path.join(os.path.dirname(__file__), "configs")

GAMMAS = {"g080": 0.8, "g090": 0.9, "g095": 0.95, "g099": 0.99, "g0995": 0.995}


def behavior_block(name, gamma, max_steps, summary_freq, checkpoint_interval,
                   save_steps, swap_steps, team_change):
    return f"""  {name}:
    trainer_type: poca
    hyperparameters:
      batch_size: 2048
      buffer_size: 40960
      learning_rate: 3.0e-4
      learning_rate_schedule: linear
      beta: 5.0e-3
      beta_schedule: constant
      epsilon: 0.2
      epsilon_schedule: linear
      lambd: 0.95
      num_epoch: 5
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 2
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: {gamma}
        strength: 1.0
    max_steps: {max_steps}
    time_horizon: 256
    summary_freq: {summary_freq}
    checkpoint_interval: {checkpoint_interval}
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: {save_steps}
      swap_steps: {swap_steps}
      team_change: {team_change}
      initial_elo: 1200.0
"""


def config(header, gamma, env_params, smoke=False):
    if smoke:  # 50k budget, self-play cadences scaled down (same as TagMApoca_smoke.yaml)
        kw = dict(max_steps=50000, summary_freq=10000, checkpoint_interval=25000,
                  save_steps=5000, swap_steps=3000, team_change=20000)
    else:      # 5M rigor settings — identical to TagMApoca_{sparse,shaped}_5M.yaml
        kw = dict(max_steps=5000000, summary_freq=50000, checkpoint_interval=250000,
                  save_steps=50000, swap_steps=50000, team_change=100000)
    body = "behaviors:\n"
    body += behavior_block("Chaser", gamma, **kw)
    body += behavior_block("Runner", gamma, **kw)
    body += "\nenvironment_parameters:\n"
    for k, v in env_params.items():
        body += f"  {k}: {v}\n"
    return header + body


def env(coef, gamma, n_obs, layout):
    return {"distance_shaping_coef": coef, "shaping_gamma": gamma,
            "num_obstacles": n_obs, "obstacle_layout": layout}


FILES = {}

# Probes: shaped, no obstacles, low gamma (RQ-B)
for tag in ("g080", "g090"):
    g = GAMMAS[tag]
    FILES[f"TagMApoca_shaped_{tag}.yaml"] = config(
        f"# GAMMA PROBE (RQ-B) — MA-POCA shaped (PBS coef 0.5), NO obstacles, gamma {g}.\n"
        f"# Identical to TagMApoca_shaped_5M.yaml except gamma + shaping_gamma.\n"
        f"# Spec: docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md\n",
        g, env(0.5, g, 0, 0))

# Phase A (fixed pillars) + Phase B (randomized): sparse sweep (RQ-A / RQ-C)
for mode, layout, word in (("obsF", 0, "FIXED"), ("obsR", 1, "RANDOM-PER-EPISODE")):
    for tag, g in GAMMAS.items():
        FILES[f"TagMApoca_sparse_{mode}_{tag}.yaml"] = config(
            f"# GAMMA SWEEP (RQ-A/RQ-C) — MA-POCA sparse, 4 {word} pillars, gamma {g}.\n"
            f"# Identical to TagMApoca_sparse_5M.yaml except gamma + obstacle env-params.\n"
            f"# Spec: docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md\n",
            g, env(0.0, g, 4, layout))

# Smoke gates (50k)
FILES["TagMApoca_gprobe_smoke.yaml"] = config(
    "# SMOKE GATE for the gamma probes: 50k, shaped, gamma 0.8 — verifies the binary\n"
    "# reads shaping_gamma (check the [TagAgent] log line in the run log).\n",
    0.8, env(0.5, 0.8, 0, 0), smoke=True)
FILES["TagMApoca_obs_smoke.yaml"] = config(
    "# SMOKE GATE for the obstacle binary: 50k, sparse, 4 pillars in RANDOM mode\n"
    "# (exercises the placement code path harder than fixed mode).\n",
    0.99, env(0.0, 0.99, 4, 1), smoke=True)

for target in (MLAGENTS_CFG, ARCHIVE):
    os.makedirs(target, exist_ok=True)
    for name, text in FILES.items():
        with open(os.path.join(target, name), "w", newline="\n", encoding="utf-8") as f:
            f.write(text)
    print(f"wrote {len(FILES)} configs -> {target}")
