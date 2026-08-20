# Phase C — Multi-agent teams: does MA-POCA beat PPO at N > 1?

**Status:** design approved 2026-08-20. Branch `feat/phase-c-multiagent`.
**Predecessors:** Phase A (fixed pillars, γ sweep, Theory §14), Phase B (randomized pillars, γ=0.99,
Theory §14 "Phase B results"). Both complete.
**Next artefact:** implementation plan via `superpowers:writing-plans`.

---

## 1. Why this phase exists

The thesis is named after MA-POCA, but **no result in it can distinguish MA-POCA from PPO.**

§13 established sparse equivalence at 1v1 and read it as a positive ("MA-POCA ≈ PPO"). The mechanism
behind that equivalence is now measured directly: across every 1v1 run in the project the ratio
`Losses/Baseline Loss ÷ Losses/Value Loss` sits at **1.002–1.006**.

| Run group | Baseline/Value ratio |
|---|---|
| `ObsSmoke_02` (50k, obstacles) | 1.0017 |
| `POCA_sparse_obsR_g099_s1/s2/s3` (5M, Phase B) | 1.0017 / 1.0052 / 1.0062 |

At group size 1 the counterfactual baseline has no teammate to condition on, so it collapses onto
the ordinary value function. MA-POCA's distinguishing machinery has been **idling through every
experiment in this thesis**. Obstacles did not change it; layout randomization did not change it.
Only teams can.

Phase C is therefore the phase that answers **"why MA-POCA at all?"** — and it is the final
experimental phase, so it must leave the thesis defensible whichever way the result lands.

### Research question

> **RQ-D:** At team sizes greater than one, does MA-POCA's centralized counterfactual baseline and
> its handling of agents that terminate mid-episode produce measurably better play than PPO given
> the identical environment, identical reward schedule, and a shared team reward?

---

## 2. Locked decisions

| # | Decision | Rationale |
|---|---|---|
| D1 | A tagged runner **deactivates**; the episode continues until all runners are caught or the clock expires | The only option that exercises **posthumous credit assignment** — the "P" in MA-POCA — which the thesis has never tested |
| D2 | Self observations in `VectorSensor` (9 floats); **all other agents** in a `BufferSensorComponent` | Permutation-invariant, tolerates a shrinking opponent set for free, and yields ONE behavior spec across every team size |
| D3 | **Approach A** — generalize `TagArenaManager` / `TagAgent` in place | Alternatives rejected: parallel classes would put the 1v1 control on different code from the team runs, defeating the control; hardcoding 2v2 discards the scaling question |
| D4 | Primary claim is the **POCA-vs-PPO comparison**, not demonstration or ablation | It is the gap the thesis cannot currently fill; the `Baseline/Value` diagnostic is a guaranteed secondary result even if catch rates tie |
| D5 | PPO baseline = **shared team reward, individually delivered; departed agents miss subsequent events** | The standard "independent learners with shared reward" baseline, and precisely the documented limitation under test |
| D6 | Team sizes **fixed per run** | Like-for-like comparison, interpretable ELO, only the algorithm differs between arms |
| D7 | Run matrix **deferred** until the throughput bake-off measures per-run wall-clock | 5–8 h/run is an extrapolation from 1v1's measured 4.3 h; 8 agents plus attention inference is a different load |

### Why D5 is fair, and how it must be written up

`SimpleMultiAgentGroup.AddGroupReward` iterates **current** membership:

```csharp
public void AddGroupReward(float reward) {
    foreach (var agent in m_Agents) agent.AddGroupReward(reward);
}
```

A deactivated agent is auto-unregistered (`RegisterAgent` subscribes
`agent.OnAgentDisabled += UnregisterAgent`, `SimpleMultiAgentGroup.cs:33`), so it cannot receive any
reward issued after it leaves — **in either arm**. The Unity-side plumbing is identical for POCA and
PPO. The difference lives entirely in the trainer: MA-POCA's centralized critic evaluates the
*group's* return, so an early-terminated trajectory bootstraps from a group-state value that
reflects what the team went on to do, whereas PPO's per-agent critic sees only that agent's own
future, which after deactivation is empty.

This is what makes the comparison clean rather than rigged: **the reward wiring is the same; the
credit assignment is the only difference.** The thesis must state this explicitly, because the
design is otherwise open to a "you crippled PPO" objection. It is the limitation under test, quoted
in the thesis already from ML-Agents' own documentation (¶643): *"agents will not be able to learn
from group rewards after deactivation/removal, nor will they behave as cooperatively."*

---

## 3. Architecture

Five components, following existing project patterns.

| Component | Responsibility | Status |
|---|---|---|
| `TagArenaManager` | `List<TagAgent>` per role, both groups, step clock, reset, termination, rewards, stats | refactor |
| `TeamManager` | Activates N chasers + M runners from env-params — **mirrors `ObstacleManager`** | new |
| `TagAgent` | Self → `VectorSensor`; all other agents → `BufferSensorComponent`; no longer drives clock or reset | modify |
| `TagReward` | Team-normalized terminal math, pure, unit-tested | extend |
| `SpawnPlacement` | N-agent spawn sampling with pairwise + obstacle rejection, pure, unit-tested | new |

### Episode data flow

```
Manager.FixedUpdate()   → tick clock; timeout → TriggerStalemate()
Manager.ResetArena()    → obstacles.ResetObstacles()
                        → teamManager.Activate(num_chasers, num_runners)
                        → SpawnPlacement.Sample(N) → place, zero velocities
                        → RE-REGISTER every active agent in its group   ← see trap below
Runner tagged           → group rewards + stats  → THEN runner.SetActive(false)
All caught              → EndGroupEpisode()          (true terminal)
Timeout                 → GroupEpisodeInterrupted()  (truncation, bootstraps)
```

### The registration trap

`SetActive(false)` auto-unregisters an agent from its group. DungeonEscape — the canonical ML-Agents
posthumous-credit example — therefore never calls `UnregisterAgent` explicitly, and **re-registers
every agent on each reset**. Missing that re-registration drains the group to empty over successive
episodes, delivering rewards nowhere, **with no error raised**. `RegisterAgent` is idempotent
(HashSet + contains-check), so unconditional re-registration of all active agents is safe.

### Scene and configuration

- `TagArena.prefab` carries **4 chasers + 4 runners authored inactive**, activated by env-param —
  identical to how the 4 pillars already work. **One scene, one binary for the entire matrix**, no
  rebuild between compositions.
- New env-params **`num_chasers` / `num_runners`, both defaulting to 1** ⇒ every existing config
  stays byte-identical and Phase A / B / §12 / §13 remain reproducible. Same defaulting discipline
  as `num_obstacles` and `shaping_gamma`.
- Arena count becomes a bake-off output, not a constant: 16 arenas × 8 agents plus attention
  inference is a very different load from 16 × 2.

### Fixing gotcha #3 in passing

Moving the step clock and arena reset into the manager's own `FixedUpdate` is mandatory once no
chaser is privileged, and it resolves the long-standing issue logged in `CLAUDE.md`: reset is
currently a side effect of the chaser's `OnEpisodeBegin`, which fires synchronously inside the first
`EndGroupEpisode()`, so the runner is repositioned *before* its own episode ends. Because
`NotifyAgentDone` collects a fresh terminal observation (`Agent.cs:609-614`), the runner's terminal
observation is currently its **next-episode spawn position**. On a catch this is harmless (true
terminal, no bootstrap); on a **stalemate it is not**, since truncation bootstraps the value
estimate from that observation. The refactor fixes this; the regression run measures the impact.

---

## 4. Reward structure

Per-event delivery, normalized by runner count `N_r`.

```
Runner i tagged at arena step t:
    timeBonus     = (1 − clamp01(t/maxEpisodeSteps)) * 0.5
    survivalBonus = clamp01(t/maxEpisodeSteps) * 0.5
    chaserGroup.AddGroupReward((+1 + timeBonus)     / N_r)
    runnerGroup.AddGroupReward((−1 + survivalBonus) / N_r)
    [PPO arm] mirror to every still-active agent via AddReward
    → THEN runner_i.SetActive(false)        // reward BEFORE deactivation — order is load-bearing

Timeout, per surviving runner:
    chaserGroup.AddGroupReward(−1 / N_r)
    runnerGroup.AddGroupReward(+1 / N_r)

Step rewards (unchanged, per-agent, unnormalized):
    chaser −0.001/step,  runner +0.001/step;  a deactivated runner stops accruing
```

**Two properties make this the correct generalization:**

1. **Scale preserved.** All runners caught → chaser total ∈ [+1, +1.5]; none caught → exactly −1.
   Identical to the 1v1 axis, so Phase C sits on the same scale as §12/§14 and can be plotted with
   them.
2. **At `N_r = 1` it reduces algebraically to the current formula** — `(1+tb)/1`, `(−1+sb)/1`,
   timeout ∓1. Not merely equivalent in spirit; the same expression. The regression run therefore
   isolates the arena-loop refactor rather than testing new reward math.

**Rewarding before deactivation is deliberate.** A tagged runner receives its own outcome while
still registered; what it misses is its *teammates'* later outcomes. If runner 1 is caught early
while runners 2 and 3 survive, runner 1's trajectory says "I lost" while the team did well —
reconciling those is exactly the posthumous problem. The rejected alternative (deactivate first,
deliver everything at episode end) maximizes the posthumous burden but risks leaving the runner team
with no learnable signal in *either* arm, producing a floor effect instead of a comparison.

---

## 5. Pre-registered expectations

*Written 2026-08-20, before any Phase C run. Standing project rule since Theory §14.*

**P1 — The baseline engages.** `Baseline Loss ÷ Value Loss` departs from unity at group size > 1,
exceeding **1.05**. *Falsified if* it stays within the 1.002–1.006 band observed at 1v1 — which
would mean MA-POCA's centralized baseline is inert even with teammates, and RQ-D is unanswerable in
this environment. This is checked at the 50k smoke gate, not after 5M steps.

**P2 — The effect is asymmetric, favouring the runner side.** Only runners deactivate; chasers
remain active for the whole episode. Posthumous credit therefore applies to the **runner group**,
while the chaser group differs between arms only via the centralized baseline. Prediction: MA-POCA's
advantage over PPO is **larger in runner-side metrics** (runner survival, runner ELO, the fraction of
runners surviving to timeout) than in chaser-side metrics. *Falsified if* the arms differ mainly on
the chaser side, or equally on both.

**P3 — MA-POCA outperforms PPO at N > 1 on the team outcome.** Prediction: at matched composition,
the MA-POCA runner team achieves a **higher survival fraction** than the PPO runner team.
*Falsified if* the two arms are indistinguishable within seed ranges — a legitimate and reportable
outcome that would generalize §13's equivalence from 1v1 to teams, and which the thesis must be
prepared to state plainly rather than explain away.

**P4 — Magnitude scales with runner count**, since each deactivation is one more posthumous window
per episode. Only testable if the matrix affords more than one composition; recorded now so it
cannot be claimed post hoc.

**Guard against a known confound:** ELO is self-play-relative and uncalibrated across runs
(standing caveat since §14). Cross-arm comparisons must be carried by **survival fraction, catch
rate and episode length**, not by ELO.

---

## 6. Staged execution

| # | Stage | Cost | Gate |
|---|---|---|---|
| 0 | Implement (TDD, subagent-driven) | — | spec + quality reviews pass |
| 1 | EditMode tests | minutes | green, including the 13 existing tests |
| 2 | **Throughput bake-off** | ~1 h | arena count + per-run wall-clock measured |
| 3 | **Smoke gate, 50k** | ~10 min | 6 criteria below |
| 4 | **1v1 regression run** | ~4.3 h | lands inside Phase B's 3-seed band |
| 5 | Matrix decision | — | user's call, informed by stage 2 |
| 6 | Training runs | set at stage 5 | — |
| 7 | Analysis → Theory §16 → Croatian draft sections | — | — |

### Stage 3 — smoke gate criteria

1. `[TeamManager] num_chasers=N, num_runners=M` present in the Player log
2. Both behaviours complete 50k; no NaN; no Unity errors
3. **`Baseline Loss ÷ Value Loss` > 1.05** — the phase's premise (P1)
4. ONNX export succeeds **with a `BufferSensor` in the graph** — never exercised in this project
5. **Group membership does not drain**: log `GetRegisteredAgents().Count` every N resets; must
   return to full team size
6. `Environment/Catch` > 0 with multiple runners; runner-survival stat is recorded and non-trivial

### Stage 4 — regression run

1v1, **randomized** pillars, γ=0.99, new code, compared against `POCA_sparse_obsR_g099_s1/s2/s3`
(catch 0.999, ELO gap 1257, range 34). A 3-seed band, far stronger than Phase A's single fixed seed.
Must land inside it. If it does not, the arena-loop refactor changed 1v1 behaviour — which, given
the terminal-observation bug described in §3, is a *possible and reportable* outcome rather than a
failure, but it must be measured, not assumed.

### Stage 5 — matrix shape

To be filled from stage 2's measurements. Indicative:

| Arm | Runs | Est. at 2v2 |
|---|---|---|
| MA-POCA @ one composition × 3 seeds | 3 | ~24 h |
| PPO @ same composition × 3 seeds | 3 | ~24 h |

If the budget must shrink, drop to a single composition first and cut seeds only after that — and
**never below 2 seeds per arm**. The comparison is the deliverable, and Phase A demonstrated that a
single seed in this environment can collapse and mislead.

### Pre-committed fallbacks

| Trigger | Response |
|---|---|
| Criterion 3 fails (baseline inert at N>1) | **Stop.** Report "the baseline is inert even at N>1" as the finding; pivot the budgeted runs to the demonstration claim |
| Criterion 4 fails (ONNX + attention) | Fall back to fixed-max padded observations; costs permutation-invariance, not the experiment |
| Throughput too low | Reduce arena count first, seeds second, composition last |
| Catch pins at 1.0 immediately | Comparison floors out; raise `N_r` relative to `N_c` before spending seeds |
| Spawn infeasible at 8 agents | Reduce max composition to whatever the feasibility test establishes (see §7) |

---

## 7. Testing

Extends the existing pattern: **13 EditMode tests** (8 `ObstaclePlacementTests`, 5
`TagRewardTests`) in the `TagGame.Reward` asmdef.

| Module | Tests |
|---|---|
| `TagReward` (extended) | Scale preservation (all caught → [+1, +1.5]; none → −1); partial outcomes graded monotonically in the number caught; bonus clamping at both ends |
| `TagReward` — **regression** | At `N_r = 1`, team formulas return **bit-identical** values to the existing 1v1 functions across a swept range of `t` |
| `SpawnPlacement` (new) | N agents placed with pairwise separation ≥ `minSpawnDistance`; all clear of obstacle clearance; chasers left half, runners right half; bounded retries terminate; **feasibility at maximum composition** |
| `TeamManager` | `Activate(n, m)` yields exactly n + m active; clamps to the authored 4+4 maximum; defaults to 1+1 when env-params are absent |

**The feasibility test is a hard constraint, not a formality.** Eight agents at `minSpawnDistance`
3, plus four pillars with clearance, inside a 16×16 usable area is not obviously satisfiable — and
the current `SampleSpawn` returns its last candidate after exhausting the retry budget rather than
failing loudly, so an infeasible composition would silently spawn agents inside one another and
produce instant tags. The test must establish the true maximum feasible agent count, and the run
matrix may not exceed it.

**The 13 existing tests must stay green untouched.** That is the cheap counterpart to the
4.3-hour regression run: together they prove the generalization did not alter 1v1 behaviour.

**Deferred to the smoke gate** (require the trainer in the loop): BufferSensor observation shape,
ONNX export, `Baseline ≠ Value`, group-membership drain, and all learning dynamics. The drain check
is a logged count assertion rather than a PlayMode test, since a silent drain raises no exception.

---

## 8. New stats to record

Phase C needs outcome metrics that 1v1 did not:

- `Environment/RunnersSurvived` — fraction of runners alive at episode end. **The primary
  cross-arm metric** for P3, and the one that carries the comparison since ELO cannot.
- `Environment/Catch` — retained; at N>1 it becomes "all runners caught", so it is a stricter event
  than at 1v1 and is **not** directly comparable to Phase A/B values without saying so.
- `Environment/TimeToCatch` — retained, recorded per catch event.
- `Losses/Baseline Loss` ÷ `Losses/Value Loss` — the diagnostic, reported per run.

---

## 9. Deliberately out of scope

- **Randomized composition per episode.** The strongest exercise of MA-POCA's variable-group
  machinery and supported by the BufferSensor design, but it makes performance a function of
  composition, adding variance a two-arm comparison must average over. Recorded as future work.
- **Asymmetric compositions (3v2, 2v3)** unless the bake-off leaves budget.
- **A shaped-reward arm.** The obstacle programme is sparse-only for reasons documented in Theory
  §14; the PBS potential depends on Euclidean distance alone, so teams do not change the farming
  mechanism.
- **γ variation.** Fixed at 0.99, settled by Phase A.
