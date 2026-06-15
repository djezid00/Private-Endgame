# Progress Log — Tag Game with MA-POCA (Unity ML-Agents)

Thesis: *Analysis of Competitive Interaction in Video Games using Multi-Agent Machine Learning.*
Each entry is one working session. Newest at the top.

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
