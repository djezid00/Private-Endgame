# Progress Log — Tag Game with MA-POCA (Unity ML-Agents)

Thesis: *Analysis of Competitive Interaction in Video Games using Multi-Agent Machine Learning.*
Each entry is one working session. Newest at the top.

---

## 2026-06-17 — Reward-shaping experiment: brainstorm → spec → plan

New branch: **`feat/sparse-vs-shaped-comparison`** (off `feat/ma-poca-asymmetric-refactor`).

### Decisions (full brainstorm)
- **Shaping IS the experiment:** run a **sparse vs shaped** comparison, identical except the
  chaser's distance-shaping term, and report both as a finding.
- **Shaped reward = potential-based shaping (PBS)**, Ng et al. 1999 — `Φ = −coef·dist/maxDist`,
  `F = γΦ′−Φ`, `coef = 0.5`, policy-invariant (defends the emergence claim).
- **Kinematics fixed equal 5/5** across both arms; **6/5 chaser edge** is a documented fallback.
- **Strict success rule:** an arm is healthy only if catch rate ↑ AND episode length ↓ AND ELO
  diverges; both arms flat ⇒ trigger the fallback.
- Arm is driven from **config** (`environment_parameters.distance_shaping_coef`), not an Editor toggle.

### Artifacts
- Spec: `docs/superpowers/specs/2026-06-17-chaser-reward-shaping-design.md` (commit `859b626`).
- Plan: `docs/superpowers/plans/2026-06-17-chaser-reward-shaping.md` (commit `b52adb3`) — 5 tasks:
  (1) pure `TagReward` PBS math + EditMode unit tests, (2) config-driven PBS in `TagAgent`,
  (3) `StatsRecorder` catch/time-to-catch, (4) sparse/shaped configs, (5) run both arms + capture.

### Hardware note (arena scaling)
i7-9750H (6c/12t), 16 GB RAM, GTX 1660 Ti (4 GB). Workload is environment/IPC-bound, not
compute-bound → **GPU is irrelevant** (CPU PyTorch, tiny net). Editor sweet spot ~8–12 arenas
(test up to 16). Biggest real win for the 5M run = **headless standalone build + `--no-graphics`**
(removes render overhead, enables `--num-envs`). Kept arena count unchanged for the 400k validation
(quick, and must stay constant across arms). Arena scaling = its own task before the 5M run.

### Execution (subagent-driven, same session)
Plan implemented task-by-task; all committed on `feat/sparse-vs-shaped-comparison`:
- `e9828ef` — Task 1: pure `TagReward` PBS math + asmdefs + 5 EditMode unit tests.
- `99bc2db` — Task 2: config-driven PBS shaping in `TagAgent` (chaser only; reads
  `distance_shaping_coef` per episode, telescoping `F = γΦ′−Φ`).
- `a335976` — Task 3: `StatsRecorder` logs `Environment/Catch` + `Environment/TimeToCatch`.
- `66b9b36` — Task 4: `TagMApoca_sparse.yaml` / `TagMApoca_shaped.yaml` (in ml-agents repo +
  archived in `experiments/configs/`); diff = only the comment + coef (0.0 vs 0.5).
- `028a468` — scene scaled to 8 parallel arenas.

**Verified in-Editor (human):** clean recompile (no Console errors); **EditMode tests 5/5 passed**
(results `Assets/Tests/EditMode/TestResults_20260617_172304.xml`); prefab shows Arena Diagonal 28.28 /
Shaping Gamma 0.99. Both branches pushed to `origin`.

### Task 5 — Arm A (sparse) DONE; Arm B pending
`TagVal_sparse_01` (coef 0, seed 12345, 400k, 8 arenas) completed. **Raw final-window metrics**
(snapshot from `results/TagVal_sparse_01/run_logs/`, NOT yet interpreted — full trajectory is in
TensorBoard, and the verdict needs Arm B for comparison):

| metric (final window) | Chaser | Runner |
|---|---|---|
| ELO | 1212.6 | 1189.3 |
| EpisodeLength.mean | 386.0 | 399.0 |
| CumulativeReward.mean | −1.937 | +1.998 |
| GroupCumulativeReward.mean | −0.91 | +0.94 |
| Catch.mean (catch rate) | ~0.077 | ~0.08 |
| Entropy.mean | 1.43 | — |

Read: at 400k the sparse arm is still near the random-baseline regime — episode length ≈ the 400-cap
(mostly stalemate), catch rate ~8%, ELO gap only ~23 pts (chaser ahead), entropy still ~1.43 (near
random). `Chaser.TimeToCatch.mean = 0.0` is a final-window artifact (no catch sampled in the last
window), not a real zero — use the TensorBoard curve. **Do not conclude yet** — compare against the
shaped arm tomorrow.

### Next session (resume here)
1. Run **Arm B**: `mlagents-learn config/poca/TagMApoca_shaped.yaml --run-id=TagVal_shaped_01 --seed 12345`
   (coef 0.5), press Play. ~25–35 min.
2. Launch `tensorboard --logdir results` (2nd Anaconda Prompt); Claude drives Playwright to screenshot
   ELO / EpisodeLength / Catch / TimeToCatch / CumulativeReward for **both** run-ids → `docs/figures/validation/`.
3. Apply the strict success rule (catch rate ↑ AND episode length ↓ AND ELO diverge) per arm; if both
   flat near baseline → 6/5 chaser-edge fallback. Write the sparse-vs-shaped comparison into `docs/Theory.md`.
4. Then `superpowers:finishing-a-development-branch`.

---

## 2026-06-16 — Editor verification + first smoke train

Branch: `feat/ma-poca-asymmetric-refactor` (still not merged). Commit `abe2a0b`.

### What we completed
- **Editor verification (step 1) done.** No Console errors; prefabs confirmed (one
  DecisionRequester, correct Behavior Name/TeamId, MaxStep=0, empty model).
- **Found + fixed a real movement bug.** Rigidbody `m_Constraints` was `10`
  (FreezePositionX|Z) → froze horizontal `rb.MovePosition`; A/D (rotation) worked but
  W/S (movement) didn't. Changed to `80` (FreezeRotationX|Z) on both agent prefabs.
  WASD verified moving across all 4 arenas in-Editor.
- **Fixed cosmetic float.** Authored agent `y` 1→0.5 on prefabs and the TagArena
  nested-instance `y` overrides 2→0.5 so cubes rest flush when stopped (runtime was
  already correct via spawnY 0.5).
- **First smoke train ran clean** (`TagTest_poca_01`, `config/poca/TagMApoca_smoke.yaml`,
  50k budget, 4 arenas, CPU). Both behaviours connected, checkpoints + `.onnx` exported,
  clean exit, **no NaNs**.

### Key findings (full write-up in `docs/Theory.md`)
- **Confirmed genuine MA-POCA, not PPO:** finite `BaselineLoss` (Chaser 0.0202 /
  Runner 0.0206) — the counterfactual baseline term PPO doesn't have.
- **Baseline regime ≈ 100 % stalemate** (mean episode length ≈393/400 decision steps,
  catch rate ~5–15 %); value estimates directionally correct (Chaser −0.23 / Runner +0.04).
- **Workload is environment-bound, not compute-bound:** ~79 % of wall-clock is Unity sim +
  IPC, only ~6 % is gradient updates → **more arenas, not a GPU**, is the lever.
  ~277 agent-steps/s at 4 arenas → ~10 h for the 5M run.
- **Principal risk:** sparse catch signal + identical kinematics may stall chaser learning;
  candidate levers = distance-closing shaping and/or slight chaser speed advantage
  (to justify/ablate — see Theory.md §6).

### Next steps
1. Short validation run (~300–500k) on full `TagMApoca.yaml` — confirm ELO diverges,
   rewards oppose, mean episode length drops (first real learning signal).
2. Decide the reward-shaping question (sparse vs shaped) and raise arena count before 5M.
3. (Optional, high thesis value) PPO-vs-MA-POCA comparison to justify the algorithm choice.

### Brainstorm started (PARKED — resume next session)
Began a `superpowers:brainstorming` session on the reward design. Decisions so far:
- **Scope:** reward + kinematics design for the *next* run only (not the full thesis
  experiment plan — that's a later brainstorm).
- **Open question we stopped on — "shaping stance":** how to treat chaser reward
  shaping given the thesis framing. Three options on the table:
  (a) shaping is fine if justified (pragmatic, add dense distance-closing reward);
  (b) protect purity, shape only if the pure terminal ±1 run demonstrably fails;
  (c) make sparse-vs-shaped a deliberate comparison and report both as a finding.
- **Resume here:** answer the shaping-stance question, then the second lever
  (equal vs unequal chaser/runner kinematics), then propose 2–3 approaches → design.
  Context for the levers is in `docs/Theory.md` §6.

---

## 2026-06-15 — MA-POCA asymmetric refactor

Branch: `feat/ma-poca-asymmetric-refactor` (not merged — awaiting approval).
Commit under review: `8a10140`.

### What we completed today
- **Locked the architecture** (with the user): split the single shared `TagMApoca`
  behaviour into **two behaviours** — `Chaser` (TeamId 0) and `Runner` (TeamId 1).
  This is the ML-Agents-documented approach for *asymmetric* games and removes role
  ambiguity without a role-observation hack. Observation size stays **18 floats**.
- **Made it genuine MA-POCA.** Added two `SimpleMultiAgentGroup`s in
  `TagArenaManager` (chaserGroup / runnerGroup), registered each agent, and routed all
  terminal team rewards + episode ends through the groups (was per-agent → behaved like
  PPO before).
  - Catch → `EndGroupEpisode()` (true terminal, no value bootstrap).
  - Stalemate → `GroupEpisodeInterrupted()` (truncation, bootstraps value — correct).
- **Fixed a latent reward sign bug** uncovered during code review: the old edge-case
  branch (when the runner's collider fired the collision first, ~half of catches)
  rewarded the *runner* +1 and *chaser* −1 — backwards. Now both branches score a
  catch as chaser +1 / runner −1.
- **Removed the MaxStep race:** set `MaxStep = 0` on both prefabs so the arena solely
  owns episode termination (was 2000 vs. the arena's 2000-step stalemate timer).
- **Added `DecisionRequester`** (period 5) to both agent prefabs so decisions are
  requested during training and the setup is reproducible.
- **Fixed "floating":** spawn height `spawnY` 1f → 0.5f so the 1×1×1 box rests flush
  on the floor.
- **Split the trainer config** `…/ML_AGENTS_GIT/ml-agents/config/poca/TagMApoca.yaml`
  into matching `Chaser` and `Runner` poca + self-play blocks. Validated in the conda
  `mlagents` env: both load as `poca`, `self_play=True`, `max_steps=5e6`,
  "RunOptions schema OK".
- **Code review pass** (high-effort, multi-angle) on the diff: no blocking bugs;
  two findings recorded (see Open issues).
- **Git hygiene:** pre-session snapshot committed; moved off the leaked-PAT repo to
  `Private-Endgame` with Git Credential Manager. Working on a feature branch, not
  merging until approved.

### Current status of each file
| File | Status |
|------|--------|
| `Assets/Scripts/TagArenaManager.cs` | ✅ Refactored — groups, group rewards, group end/interrupt, spawnY 0.5. Committed. |
| `Assets/Scripts/TagAgent.cs` | ➖ Unchanged (per-step shaping ±0.001 kept; rotation cleanup deferred). |
| `Assets/Prefabs/ChaserAgent.prefab` | ✅ Behavior `Chaser`, TeamId 0, MaxStep 0, DecisionRequester added. Committed. |
| `Assets/Prefabs/RunnerAgent.prefab` | ✅ Behavior `Runner`, TeamId 1, MaxStep 0, DecisionRequester added. Committed. |
| `Assets/Prefabs/TagArena.prefab` | ✅ Composite-prefab overrides fixed so base-prefab edits aren't undone. ⚠️ Has some component drift (re-added BoxColliders / orphaned removed-component refs) — verify in Editor. |
| `…/ml-agents/config/poca/TagMApoca.yaml` | ✅ Split into `Chaser`/`Runner` poca blocks. Validated. (Lives in the ML-Agents repo, outside this git project.) |
| `docs/progress.md` | ✅ Created (this file). |

### Next steps (for tomorrow)
1. **Editor verification — walk through step by step:**
   1. Compile, open `SampleScene`, confirm **no red Console errors**.
   2. Inspect each agent prefab: exactly **one** DecisionRequester, correct Behavior
      Name / TeamId / MaxStep=0, and **`m_Model` empty** (train from scratch).
   3. **Check Rigidbody `m_Constraints = 10`** — confirm it means Freeze Rotation X+Z
      (allowed) and NOT Freeze Position X/Z (which would break movement).
   4. Heuristic play (WASD): chaser moves, no floating, collision ends + resets the
      episode, stalemate fires at 2000 steps.
2. **Smoke train** (~50k–100k steps) from the ML-Agents repo root:
   `mlagents-learn config/poca/TagMApoca.yaml --run-id=TagTest_poca_01 --train`.
   Confirm both `Chaser` and `Runner` behaviours register, ELO + reward logged,
   episodes end via catch/stalemate (not MaxStep), no NaNs.
3. **Short validation run** (~300k–500k steps): ELO diverges from 1200, reward curves
   move in opposing directions, visible pursuit/evasion.
4. Only after we both agree the baseline is healthy → **launch the multi-day 5M run**.

### Open issues / decisions pending
- **Reset-ordering (non-blocking).** Reset happens inside the chaser's
  `OnEpisodeBegin`, which fires synchronously during the *first* `EndGroupEpisode()`,
  so the runner is repositioned before its own group episode ends (teleport in its
  final transition). Matches the original/working behaviour, but the canonical
  ML-Agents pattern ends both groups *then* resets once. **Decision pending:** clean
  up now vs. as a follow-up. Recommendation: follow-up, after the smoke run confirms
  the current version trains.
- **Rigidbody constraints (`m_Constraints = 10`)** — must be verified in the Editor
  (see Next steps 1.3) before trusting movement.
- **TagArena.prefab drift** — verify the orphaned removed-component references and
  duplicated BoxColliders don't cause Editor warnings.
- **Rotation physics** — `TagAgent.OnActionReceived` mixes `rb.MovePosition` with
  `transform.Rotate`; deferred (left movement untouched this session).
- **Out of scope for now:** cooperative teams (2 chasers), arena obstacles, optional
  PPO-vs-POCA comparison experiment.
