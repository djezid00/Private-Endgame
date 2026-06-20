# Design — 1v1 Rigor Phase (scaled MA-POCA runs, sparse-vs-shaped with seeds, PPO sanity)

**Date:** 2026-06-20
**Branch:** `feat/sparse-vs-shaped-comparison` (new work may branch from it)
**Status:** approved design, pending implementation plan
**Related:** `docs/Theory.md` §8–§11 (validation results), `docs/superpowers/specs/2026-06-17-chaser-reward-shaping-design.md`

## Purpose

The 400k validation (Theory.md §11) showed the pipeline works and PBS shaping helps, but at 400k both
policies are still near the random baseline. This phase produces the **thesis-grade 1v1 result**: scale
the runs up enough to grow genuine emergent pursuit/evasion, and add the statistical rigor (repeated
seeds → error bars) needed to defend the claim. Environment expansion (cubes, obstacles, 2v2 teams) and
the *meaningful* PPO-vs-MA-POCA comparison are explicitly a **later phase** (see "Deferred").

This was decided as a **phased** approach: lock the rigorous 1v1 result first, then expand.

## Key background decision: why this phase is POCA-focused (not a PPO showdown)

MA-POCA's value is **multi-agent group credit assignment** (sharing a team reward among teammates,
incl. agents that leave mid-episode). In the current game each `SimpleMultiAgentGroup` holds **one
agent** (1 chaser vs 1 runner), so that machinery is **dormant** — at group-size-1, MA-POCA is
functionally almost identical to PPO. Therefore the genuine PPO-vs-MA-POCA comparison belongs in the
**team-expansion phase** (≥2 agents per group). Here we include only **one PPO run as a sanity check**
to empirically show the 1v1 near-equivalence and motivate the expansion.

## Experiment matrix (7 runs)

| Algorithm | Reward arm | Seeds | Runs |
|---|---|---|---|
| MA-POCA (poca) | sparse (`distance_shaping_coef = 0`) | 3 | 3 |
| MA-POCA (poca) | shaped (`distance_shaping_coef = 0.5`) | 3 | 3 |
| PPO | shaped (`distance_shaping_coef = 0.5`) | 1 (sanity) | 1 |

- **Seeds:** 3 distinct `--seed` values per condition (e.g. 1, 2, 3) → mean ± std error bars.
- **Per-run budget:** **up to 5M steps, with early stop on plateau.** 5M is a ceiling, not a fixed
  length. Stop a run early when ELO *and* catch-rate have been flat for a long stretch (converged);
  if a run is still clearly improving at 5M, resume from its checkpoint and extend. Checkpoints are
  written periodically so both early-stop and resume are possible.
- Everything else identical to the validation configs (obs, network, self-play), so the only variables
  are reward arm and algorithm.

## Prerequisite code changes

1. **Fix `Environment/TimeToCatch`** — currently logs all-zeros. Investigate what `stepCount` holds at
   `TagArenaManager.OnAgentTagged` (the value written) and correct it so the stat records the actual
   step at which the catch occurred. Episode Length remains the cross-check.
2. **PPO terminal-reward toggle** — PPO ignores group rewards (`AddGroupReward`/`EndGroupEpisode` are
   POCA-only), so the PPO run would train with no win/lose signal. Add a **config-driven flag**
   (an `environment_parameters` value, e.g. `individual_terminal_reward`) that, when set, also delivers
   the terminal ±1 (and stalemate ±1) via each agent's `AddReward` instead of only the group API. At
   group-size-1 this is exactly equivalent, so the PPO comparison stays fair. POCA configs leave it off;
   the PPO config turns it on.
   - **Implementation risk to resolve EARLY (in the plan):** ML-Agents may also tie *episode ending*
     to the group for grouped agents. The flag may therefore need to switch the PPO arm from
     `EndGroupEpisode()`/`GroupEpisodeInterrupted()` to per-agent `EndEpisode()` as well. The plan must
     **verify ML-Agents' actual behavior with a short PPO smoke test** (does it train on the individual
     reward, warn, or error?) *before* launching the full 5M PPO run — so we don't burn hours on a
     mis-wired run. If PPO + grouped agents proves awkward, the fallback is to drop the PPO sanity run
     (it is the lowest-priority item) rather than over-engineer.
3. **PPO config file** — a `TagMApoca_ppo.yaml` (or similar) with `trainer_type: ppo`, standard PPO
   hyperparameters, the same `network_settings`/`self_play`, `max_steps: 5000000`, and
   `environment_parameters: { distance_shaping_coef: 0.5, individual_terminal_reward: 1.0 }`.

## Infrastructure (throughput)

The workload is environment/IPC-bound, not compute-bound — **GPU is irrelevant** (PyTorch is the CPU
build; the network is tiny). The two levers are *not rendering* and *more parallel arenas*.

1. **Headless standalone build** — build the scene as a Windows player. Training then runs against the
   executable with `--env=<build> --no-graphics`, with no Editor open and no per-frame rendering. This
   is the main speedup (~2× expected; confirm empirically).
2. **Arena-count bake-off** — measure **steps/second** (from the ML-Agents console summary:
   `steps ÷ Time Elapsed`) on a short ~50k-step run at **8, 12, and 16 arenas**. Steps/sec rises with
   arenas until the 6-core/12-thread CPU saturates, then flattens/drops. Adopt the count with the best
   steps/sec for all 7 runs (held constant so it does not confound the comparison).
3. **Batch runner script** — a Windows `.bat` listing all 7 `mlagents-learn` commands (unique run-ids,
   distinct seeds) that execute **sequentially and unattended** ("overnight"). It must continue to the
   next run if one fails and tee each run's output to a per-run log file.

At ~3 h/run headless, 7 runs ≈ ~20 h ≈ 3–4 overnight sessions (less with early-stops).

## Metrics & analysis

- Per run (already emitted): `Self-play/ELO`, `Environment/Catch` (rate), `Environment/Episode Length`
  (time-to-catch proxy), `Environment/Group Cumulative Reward` (true win/lose outcome),
  `Policy/Entropy`, and the loss terms (incl. `Losses/Baseline Loss`).
- **Across the 3 seeds per condition:** compute **mean ± standard deviation** per metric (pull series
  via the TensorBoard data API, aggregate, plot mean line + shaded band). This produces the error bars.
- **Comparisons:** sparse vs shaped at scale (does the shaping advantage persist or close as both
  converge?); PPO-shaped vs POCA-shaped (expect near-equal → confirms 1v1 equivalence).
- Capture curves (TensorBoard → Playwright) into `docs/figures/` and write the results into a new
  `Theory.md` section.

## Success criteria

- **Headline (emergence):** at least the shaped MA-POCA condition shows clear learning at scale —
  catch-rate rising well above baseline, episode length falling substantially, ELO strongly diverging —
  i.e. visible pursuit/evasion, not the ~stalemate baseline.
- **Shaping claim:** sparse-vs-shaped difference characterized with error bars (whether shaping mainly
  speeds early learning and both converge, or shaping stays ahead).
- **Sanity:** PPO-shaped ≈ POCA-shaped at 1v1, supporting the "teams needed for MA-POCA" argument.

## Deferred (explicitly out of scope for this phase)

- Team expansion (2 chasers sharing a group reward), grabbable cubes, obstacles.
- The **meaningful PPO-vs-MA-POCA comparison** (only informative with multi-agent groups).
- A scripted **heuristic bot** baseline for the "comparable to human heuristics" claim.
- The reset-ordering cleanup (chaser `OnEpisodeBegin` drives arena reset) — still a latent follow-up.
