# -*- coding: utf-8 -*-
"""Generates Phase C trainer configs into BOTH the ml-agents config dir and the
repo archive, mirroring experiments/gen_gamma_configs.py.

Diffs vs TagMApoca_sparse_5M.yaml are ONLY: trainer_type (poca|ppo), the
individual_terminal_reward env-param (PPO arm), and num_chasers/num_runners.
"""
import io, os

MLAGENTS_CFG = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca"
ARCHIVE      = r"c:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\experiments\configs"

COMPOSITIONS = {"2v2": (2, 2), "3v3": (3, 3)}


def behavior(name, trainer, max_steps, summary_freq, ckpt, save_steps, swap_steps, team_change):
    return f"""  {name}:
    trainer_type: {trainer}
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
        gamma: 0.99
        strength: 1.0
    max_steps: {max_steps}
    time_horizon: 256
    summary_freq: {summary_freq}
    checkpoint_interval: {ckpt}
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: {save_steps}
      swap_steps: {swap_steps}
      team_change: {team_change}
      initial_elo: 1200.0
"""


def config(header, trainer, nc, nr, indiv_term, smoke=False):
    if smoke:
        max_steps, summary, ckpt = 50000, 10000, 25000
        save_steps, swap_steps, team_change = 5000, 3000, 20000
    else:
        max_steps, summary, ckpt = 5000000, 50000, 250000
        save_steps, swap_steps, team_change = 50000, 50000, 100000
    body = header + "behaviors:\n"
    body += behavior("Chaser", trainer, max_steps, summary, ckpt, save_steps, swap_steps, team_change)
    body += behavior("Runner", trainer, max_steps, summary, ckpt, save_steps, swap_steps, team_change)
    body += f"""
environment_parameters:
  distance_shaping_coef: 0.0
  shaping_gamma: 0.99
  num_obstacles: 4
  obstacle_layout: 1
  num_chasers: {nc}
  num_runners: {nr}
  individual_terminal_reward: {indiv_term}
"""
    return body


FILES = {}

for tag, (nc, nr) in COMPOSITIONS.items():
    FILES[f"TagMApoca_team_{tag}_poca.yaml"] = config(
        f"# PHASE C (RQ-D) — MA-POCA, {tag}, sparse, 4 randomized pillars, gamma 0.99.\n"
        f"# Terminal reward flows through the GROUP channel only.\n",
        "poca", nc, nr, "0.0")
    FILES[f"TagMApoca_team_{tag}_ppo.yaml"] = config(
        f"# PHASE C (RQ-D) — PPO baseline, {tag}, sparse, 4 randomized pillars, gamma 0.99.\n"
        f"# PPO ignores group rewards, so the shared team reward is mirrored individually\n"
        f"# via individual_terminal_reward. Agents deactivated earlier miss later events —\n"
        f"# that is the documented limitation under test, not an implementation shortcut.\n",
        "ppo", nc, nr, "1.0")

FILES["TagMApoca_team_smoke.yaml"] = config(
    "# SMOKE GATE for the Phase C binary: 50k, MA-POCA, 2v2, randomized pillars.\n"
    "# Gate criterion 3: Losses/Baseline Loss / Losses/Value Loss must exceed 1.05.\n",
    "poca", 2, 2, "0.0", smoke=True)

for target in (MLAGENTS_CFG, ARCHIVE):
    os.makedirs(target, exist_ok=True)
    for name, text in FILES.items():
        with io.open(os.path.join(target, name), "w", encoding="utf8", newline="\n") as f:
            f.write(text)
        print("wrote", os.path.join(target, name))
