# Progress Log — Tag Game with MA-POCA (Unity ML-Agents)

Thesis: *Analysis of Competitive Interaction in Video Games using Multi-Agent Machine Learning.*
Each entry is one working session. Newest at the top.

---

## 2026-08-18 (cont. 2) — Phase B REDESIGNED as a multi-agent phase (brainstorm IN PROGRESS)

**Status: design paused mid-brainstorm, nothing implemented, nothing launched.** The original
`run_obs_phaseB.bat` (9 × 5M γ sweep at randomized layouts) was **not** started and is now superseded
by a redesign the user proposed. Resume at step 5 of the `superpowers:brainstorming` checklist.

**Why the redesign.** The user's observation: Phase A already established γ=0.99 as the operating
point, so re-sweeping γ under randomized pillars spends 9 runs re-answering a settled question.
Better use of the same compute: hold γ=0.99, keep randomized pillars, and add **team
configurations (2v2, 3v3, 3v2, up to 8 agents)**.

**This repairs a soft spot in the thesis, not just adds a variable.** §13 concluded "sparse
equivalence CONFIRMED — MA-POCA ≈ PPO at 1v1", which currently reads as *the algorithm choice did
not matter*. Confirmed again in today's smoke data: `Losses/Baseline Loss` 0.0205 vs
`Losses/Value Loss` 0.0204 — the counterfactual baseline is numerically **inert at group size 1**.
Teams are the condition under which MA-POCA is supposed to separate from PPO, so this phase can
answer "why MA-POCA at all?", which the thesis currently cannot.

**Decisions LOCKED so far (user-approved):**
1. **Termination rule:** a tagged runner is deactivated and the episode continues until all runners
   are caught or the clock expires. Chosen specifically because it is the only option that exercises
   **posthumous credit assignment** — the "P" in MA-POCA — which the thesis has never tested.
2. **Observations:** self stays in `VectorSensor` (9 floats); every other agent becomes a
   variable-length entity in a **`BufferSensorComponent`** consumed by the trainer's attention
   encoder. Permutation-invariant, tolerates the shrinking opponent set, and gives ONE behavior spec
   across 2v2/3v3/3v2 — so a 2v2 brain can be evaluated in a 3v3 arena.
3. **Architecture = Approach A**, generalize the existing classes in place (vs B: parallel
   `TeamArenaManager` classes, rejected — duplicates ~500 lines and would put the 1v1 control on
   different code from the 2v2 runs, defeating the control; vs C: hardcode 2v2, rejected — discards
   the scaling question).
4. **Run matrix DEFERRED** until the throughput bake-off + smoke gate measure real per-run
   wall-clock. Estimate 5–8 h/run is an extrapolation from Phase A's ~4–4.5 h at 16 arenas × 2
   agents; 8 agents/arena plus attention inference is a different load and arena count may need to drop.

**Verified against the actual ML-Agents source (do not re-derive):**
- `BufferSensorComponent` + `BufferSensor` exist in the local package
  (`com.unity.ml-agents/Runtime/Sensors/`), and the trainer has `torch_entities/attention.py`.
  No version blocker.
- **THE TRAP:** `SimpleMultiAgentGroup.RegisterAgent` does `agent.OnAgentDisabled += UnregisterAgent`
  (`SimpleMultiAgentGroup.cs:33`), so `SetActive(false)` **auto-unregisters** the agent. This is why
  the canonical DungeonEscape example never calls `UnregisterAgent` explicitly and why it MUST
  **re-register every agent on reset**. Miss that and the group silently drains to empty over
  episodes — rewards go nowhere, no error is raised. `RegisterAgent` is idempotent (HashSet +
  contains-check), so unconditional re-registration is safe.
- `EndGroupEpisode()` iterates registered agents calling `agent.EndEpisode()`;
  `GroupEpisodeInterrupted()` calls `EpisodeInterrupted()`. Registration is not cleared by either.
- Canonical death pattern (DungeonEscape `DungeonEscapeEnvController.cs`): on death just
  `agent.gameObject.SetActive(false)` and decrement a remaining-players counter; on reset
  `SetActive(true)` + `RegisterAgent`.

**Design Section 1 (architecture) presented, awaiting approval.** Five components:
`TagArenaManager` (refactor — owns `List<TagAgent>` per role, step clock, reset, termination),
`TeamManager` (new — activates N chasers + M runners from env-params, mirroring `ObstacleManager`),
`TagAgent` (modify — BufferSensor, no longer drives clock/reset), `TagReward` (extend —
team-normalized terminal math), `SpawnPlacement` (new pure module + tests).
- `TagArena.prefab` to carry **4 chasers + 4 runners authored inactive**, env-param activated ⇒
  **one scene, one binary for the whole matrix**, no rebuild between 2v2 and 3v3.
- New env-params `num_chasers` / `num_runners`, **both defaulting to 1 ⇒ all existing configs stay
  byte-identical** and Phase A / §12 / §13 remain reproducible (same discipline as `num_obstacles`).
- The step-clock move into the manager's `FixedUpdate` **fixes CLAUDE.md gotcha #3** (reset currently
  a side effect of the chaser's `OnEpisodeBegin`), and is mandatory once no chaser is privileged.
- **Regression run agreed as the safeguard:** because the refactor changes the arena loop, the new
  1v1 is not byte-identical to Phase A's. Plan includes 1v1 / fixed pillars / γ=0.99 on the new code
  compared against the existing `POCA_sparse_obsF_g099_s1`; must land in the seed band.

**Sections still to present:** 2 reward structure with teams (most consequential remaining — team
normalization is what keeps 2v2 comparable to the 1v1 baselines), 3 staged execution, 4 testing.
Then: write spec to `docs/superpowers/specs/`, self-review, user review, then `writing-plans`.

**Not superseded:** the rebuilt binary and the passed smoke gate from the entry below remain valid
for the *current* code. The team refactor will require a new build and a new smoke gate.

---

## 2026-08-18 (cont.) — Phase B resumed: rebuild + obstacle smoke gate PASSED

**Experiments resume.** User declared the write-up satisfactory for now and chose to run Phase B.
New branch **`feat/obstacles-phase-b`** off `docs/thesis-completion-guide` (which already carries all
obstacle code incl. the `ca64ed0` RNG fix). `98b9ab6` commits Unity's `preloadedAssets: []` churn so
the tree was clean before building.

**The stale-binary gate was real, not precautionary.** `Build/…/Assembly-CSharp.dll` was dated
**2026-07-07 21:46** while the RNG fix `ca64ed0` landed **2026-07-10 12:29** — the binary predated the
fix, so Phase B layouts would still have been seeded from wall-clock `Environment.TickCount`.
Rebuilt (Assembly-CSharp.dll, `level0`, `globalgamemanagers` all 2026-08-18 21:09; the `.exe` keeps
its old date, which is expected — it is only the launcher stub).

**Run-id collision, not a failure:** the first launch aborted because `results/ObsSmoke_01/` exists
from Jul 7. Kept (it is the provenance record for Phase A's nine 5M runs); re-ran as `ObsSmoke_02`,
then `ObsSmoke_03` at the same seed as a determinism check.

**Gate: all 4 criteria PASS.**
1. `[ObstacleManager] num_obstacles=4, layout=random` present in both Player logs, alongside
   `[TagAgent] distance_shaping_coef=0,00, shaping_gamma=0,990` (sparse arm confirmed).
2. Both behaviours completed 50k with `.onnx`; **Baseline Loss finite** (Chaser 0.0205 / Runner
   0.0214 ⇒ genuine MA-POCA); **zero non-finite values across all 20 scalar tags**; no Unity errors.
3. `Environment/Catch` 0.069 / 0.047, `TimeToCatch` 519 / 572 — catches remain possible among pillars.
4. Episode length ~393 decision steps (cap 400), Group Reward −0.92 / +0.96 — normal untrained
   baseline; catch ~5–11 % matches the documented ~5–15 % random baseline. No cross-arena anomalies.

**RNG fix VERIFIED.** `ObsSmoke_02` and `ObsSmoke_03` (same seed, separate launches) are **identical
to four decimals on every metric including the losses** (Baseline Loss 0.0205, Catch 0.1143, Episode
Length 393.8030). Under the pre-fix wall-clock seeding two launches could not have matched, so Phase
B's per-episode layouts are now genuinely `--seed`-reproducible. `ObsSmoke_01` (July binary, same
config/seed) differs (Baseline Loss 0.0200, Catch 0.1132, Ep.Len 387.58), consistent with the old
seeding — recorded as *suggestive, not conclusive*, since the code change could also have shifted RNG
stream ordering; the 02≡03 identity is the airtight half.

Non-anomaly worth not re-investigating: Runner logs to step 60000 while Chaser stops at 50000 —
self-play `team_change`/`swap_steps` boundaries differ per behaviour. `ObsSmoke_01` shows it too.

**Next:** `experiments
un_obs_phaseB.bat` — 9 × 5M, sparse, `num_obstacles: 4`,
`obstacle_layout: 1` (γ=0.8 ×3 seeds, 0.9/0.95/0.99 ×1, 0.995 ×3 seeds), ~36–42 h unattended.
No `POCA_sparse_obsR_*` results exist ⇒ clean start, no `--resume`. Then analysis into Theory §14
against the pre-registered RQ-C prediction (randomized layouts learn slower and end lower than
fixed at matched γ).

---

## 2026-08-18 — Duplication + attribution audit of the docx; Theory.md figure block rebuilt

**Docx rescan** (4.9 MB, 784 body elements, 32 figures, 11 tables, 3 code listings — up from 725
paragraphs / 27 figures on 08-13). Big wins since last pass: **§6.2 "Rezultat POCA VS PPO" now
exists** with the 2×2 table, closing the last "discussion cites data it never shows" gap; all three
field indexes were refreshed (`F9`); table numbering went per-chapter; the γ-table got its caption
and lost the literal `**` markdown; caption typos and missing spaces fixed. Zaključak / Sažetak /
Dodatak A remain intentionally empty (deferred).

**New deliverable: `DUPLIKATI_u_poglavljima_5_i_6.md`.** The user's sense that §5 and §6 repeat
themselves is correct, with two distinct causes. (1) **§5.5 contains two complete versions of the
same experiment** — a newer paste was never followed by deleting the older one, and the older copy
is precisely where all three broken empty-formula spots live (`prema formuli .`, `dok pri  pada`,
`(; ; )`), so deleting it clears a long-standing fix-list item for free. (2) **Results are written
into chapter 5** ("Dizajn eksperimenta"), so chapter 6 necessarily repeats them. Six numbered fixes,
all deletions/moves plus three bridging sentences — no rewriting of the user's prose. Moving the
§5.5 results into 6.5.3 also repairs a cross-reference in 6.4.2 that currently points at a section
which does not contain what it claims.

**New: attribution audit** (added as EVALUACIJA §4). Five findings: five reproduced figures carry no
source (`Slika 3.1` PPO pseudocode and `3.2` MuJoCo comparison → Schulman et al. [9]; `3.4` test
environments → Cohen et al. [11]; `2.1`/`2.2` → [4]) while 3.3/3.5/4.2 are correctly attributed; the
**ELO expected-score and update formulas have no citation** (Elo 1978); the "successive
approximations" claim invokes Skinner uncited; and **two pursuit-evasion claims — one of which
carries the RQ-C conclusion — are credited to a field rather than a work** (Isaacs 1965). Also
flagged that the standing-term limitation is presented purely as own analysis when prior work
(Wiewiora 2003, Grześ 2017) documents the general result; recommendation is to keep the derivation
as own and claim the *quantification* as the contribution.

**Theory.md rebuilt where it had gone stale:**
- **§11 figure block replaced.** The four old references (`tb_overview`, `tb_elo`,
  `tb_catch_episodelen`, `tb_policy`) were dangling — I deleted those files on 08-13 without
  updating Theory. Now points at the five `tb_val_*.png` single-card captures. Downstream figures
  renumbered (old 5–11 → 6–12); verified no prose cross-references existed, all 13 image links
  resolve, numbering is a clean 1–12.
- **§11 colour key corrected** — the Time Series UI assigns its own colours (orange/green/purple/
  yellow); the documented "blue/cyan = Chaser, red/pink = Runner" no longer applies.
- **§11 `Lesson Number` claim corrected.** It previously said that curve confirms coef 0.0 vs 0.5.
  It does not — it reads flat 0 for **both** arms, because `Lesson Number` tracks the curriculum
  stage index, not the sampled value. Arm selection is evidenced by the configs instead.
- **Baseline/Value collapse documented** — `Extrinsic Baseline Estimate` is numerically identical to
  `Extrinsic Value Estimate` (and `Baseline Loss` ≈ `Value Loss`) because at **group size 1** the
  counterfactual baseline has no teammate to condition on. Previously unexplained; reads as a
  duplicated metric otherwise.
- **§12 item 2 closed** — the 5M captures exist for both arms; noted they are per-seed views, not
  the seed-aggregated bands of item 1, which still need the aggregation script.
- **§14: the `g090_s1` ELO worry is resolved, negatively.** ML-Agents docs warn that resuming
  self-play resets reported ELO, which would have made that (paused/resumed) run incomparable.
  Reading its tfevents directly shows the series is **continuous across the resume** — Chaser
  1511.9 @1.90M → 1526.4 @1.95M, Runner 997.5 @1.80M → 983.8 @1.85M. No reset. Recorded as a
  checked-and-dismissed caveat rather than left as an open doubt.

**Still the top risk:** the Uvod is unchanged across four evaluations — taxi/Kafka/WebSocket/Firebase
paragraphs from an unrelated thesis, plus `Error! Reference source not found.` in place of source [3].

---

## 2026-08-13 — TensorBoard figures captured (400k + 5M); docx re-evaluated after major expansion

**Figures.** Recaptured the §11 validation set as **individually maximised single-card screenshots**
(new TensorBoard "Time Series" UI, smoothing 0.8) instead of the old grouped multi-card grabs, which
were unreadable at thesis print size. Picked the 5 load-bearing graphs rather than dumping every
scalar: `Self-play/ELO`, `Environment/Catch`, `Episode Length`, `Group Cumulative Reward`,
`Policy/Entropy` — pinned in TensorBoard so they survive reloads. Deliberately excluded
`TimeToCatch` (known all-zeros bug in the 400k runs), individual `Cumulative Reward` (not comparable
across arms), and `Lesson Number/distance_shaping_coef` (reads flat 0 for both arms — the curve does
*not* show "coef 0 vs 0.5" as `Theory.md` §11 claims; `Lesson Number` tracks curriculum stage index,
not the sampled value. **Theory.md §11's wording needs a fix**).

All 5 verified against `Theory.md` §11 (ELO 1236.4/1163.7 vs 1212.6/1190.7; catch ~0.21 vs ~0.08;
ep. length 374 vs 386; GroupR −0.75 vs −0.91) — exact matches. Files renamed and moved into
`docs/figures/validation/tb_val_*.png`, replacing the 4 stale grouped ones. Croatian captions
written for each. Then the same treatment for the §12 5M runs: 6 graphs × sparse + 6 × shaped in
`docs/figures/5M_RUN/{sparse,shaped}/`, sparse set verified (ELO 1890.7/685.5/661.1, GroupR
+1.45/−0.87/−0.94) and captioned in Croatian.

Note for whoever writes §11's figure block: the new UI assigns its own run colours (orange/green/
purple/yellow), so `Theory.md`'s "blue/cyan = Chaser, red/pink = Runner" colour key is now wrong and
must be rewritten to match. Also `Policy/Extrinsic Baseline Estimate` is numerically **identical** to
`Extrinsic Value Estimate` in every run — an artifact of 1v1 groups (the counterfactual baseline has
no teammate to condition on), worth stating explicitly so it doesn't read as a duplicated metric.

**Docx re-evaluation.** The user expanded the thesis substantially (XML 478 kB → 968 kB; 392 → 725
paragraphs; 14 → 27 figures; 5 → 8 tables; 0 → 3 code listings; 15 → 33 media files). Rescanned it
and rewrote all three working docs against actual current content:
`EVALUACIJA_diplomskog_rada.md` (→ revizija 3), `USKLADENOST_s_uputama_FESB.md` (→ revizija 2),
`VODIC_ZA_DOVRSETAK_RADA.md` (→ revizija 2).

- **Resolved:** abbreviations list (25 entries), per-chapter figure numbering (`Slika 2.1`…`6.17`),
  uppercase chapter titles, self-play params filled (50k/50k/100k; 400k validation 25k/25k/50k),
  γ-peak corrected to 0.95, 78 % consistency, code listings, 17 chapter-6 figures, Tables 5–7 + the
  Phase A γ-table. **Every transferred number re-verified against `Theory.md` — all correct.**
- **New defects, all from markdown→Word paste:** literal `**1,00**` in the γ-table; that table left
  uncaptioned (won't enter Kazalo tablica); Tablica 5 missing its `Environment/Catch` row; decimal
  points vs commas; 15 captions missing the space after the number (`Tablica 5400k`); typos
  (`Envirnoment` ×4, `Lenght` ×2, `Culmutive`, `Rezultait`). All three field indexes stale → `F9`.
- **Still open and highest-risk:** Uvod ¶115–120 is still the taxi/Kafka text from an unrelated
  thesis (unchanged across three evaluations) + `Error! Reference source not found.` in ¶113; and
  **Experiment 2 (MA-POCA vs PPO) still has a design section but no results anywhere**, while §6.4.2
  cites "0.12 vs PPO 0.98" — a number the thesis never shows. Ready-to-paste tables are in `VODIC` §4.
- **Phase B correction propagated:** EVALUACIJA revizija 2 wrongly recorded Phase B as dropped;
  N1/N2 formally withdrawn. Phase B is **deferred** — it runs after the Phase A write-up is done,
  and rebuild + `TagMApoca_obs_smoke` remains the first action when experiments resume.

**Housekeeping.** User deleted `NACRT_konfiguracijska_datoteka.md` and `PARAMETRI_samoigre.md` (their
content is now in the docx) and discarded `specs/2026-08-09-thesis-figure-export-design.md`; deletions
staged and committed. `NACRT_3.5_…md` also removed — its §3.5/§5.2 draft text is now merged into the
docx.

**Branch state:** `docs/thesis-completion-guide`, **77 commits ahead of `main`, never pushed** — no
remote branch exists yet. Next: work through `VODIC` §2 (paste artifacts) and §3 (Uvod, Experiment 2
results), then the deferred whole-work sections (Zaključak, Sažetak/Abstract, Dodatak A).

---

## 2026-08-09 — Figure export pipeline designed + planned (branch `docs/thesis-completion-guide`)

**Paused before implementation.** This session ran *concurrently* with the thesis-guide session
below, in the same working tree and branch; both stayed clean by staging explicit paths only and
never `git add -A`.

**Goal:** turn the finished results into insert-ready chapter-6 figures. Two tracks — matplotlib
plots rendered from the raw `tfevents` files as the body figures, plus four uniform TensorBoard
screenshot plates for the appendix. Scope is `Theory.md` §12 (5M sparse vs shaped), §13 (PPO 2×2 +
delivery probe) and §14 (γ sweep Phase A); the 400k validation figures stay as they are.

**Figure numbers are deliberately not baked in** — the author assigns them in Word. The pipeline's
deliverable is *placement*: `docs/slike/INDEX.md` leads with which section each figure belongs in
and which sentence it illustrates, alongside a ready-to-paste Croatian caption.

**Findings that shaped the design, all verified against the data rather than assumed:**

- The §6.3 **main figure does not exist**. Both `EVALUACIJA_diplomskog_rada.md` and earlier notes
  marked it 📁 "ready to insert"; in fact only an untitled image sits in the docx, and no script
  produces it. It needs new plotting code (Task 5).
- The three PPO figures likewise have **no generating script** — they were hand-taken TensorBoard
  screenshots. Only `plot_gamma.py` produces real plots.
- **PPO runs never log `Environment/Group Cumulative Reward`.** Expected (PPO ignores group
  rewards), but it means the 2×2 figures must be built from `Catch` and `Self-play/ELO` alone.
- Series lengths differ per tag and per run (`Self-play/ELO` 23–101 points vs `Catch` ~100;
  `PPO_shaped_s1` runs to step 5,050,000, the rest stop at 5,000,000). `plot_gamma.py` currently
  truncates its x-axis with whichever run the loop finished on — a latent bug the new shared
  `aggregate()` fixes by taking the shortest seed's steps explicitly.
- System Python 3.12.4 already has numpy 2.2.4 and matplotlib 3.10.1, so **the plotting scripts
  need no conda** — only TensorBoard does.

**Artefacts:** spec `docs/superpowers/specs/2026-08-09-thesis-figure-export-design.md`
(`4dae83d`, renumbered `888d375`, placement-first `b0456fb`); plan
`docs/superpowers/plans/2026-08-09-thesis-figure-export.md` (`be10581`) — 10 TDD tasks using
stdlib `unittest` (no pytest in this environment), ending in a `--check` gate that resolves every
run and tag and fails loudly rather than shipping an empty axis.

**Next session:** execute the plan subagent-driven (the user's choice). Nothing is implemented yet;
no file under `experiments/analysis/` or `docs/slike/` has been created.

**Correction to the entry below:** Phase B is **deferred, not dropped**. The order the user set is
finish the theory and write-up for Phase A and everything preceding it, *then* run Phase B. The
rebuild + `TagMApoca_obs_smoke` gate remains the first action when experiments resume.
`EVALUACIJA_diplomskog_rada.md` N1/N2 ("Faza B otpada / se ne piše") records this wrongly and needs
fixing; `VODIC` A4 — describe Phase B in the conditional as not-yet-executed — is still correct.

---

## 2026-08-09 — Thesis completion guide consolidated (branch `docs/thesis-completion-guide`)

**Scope clarified with user:** Phase B (randomized obstacle layouts) is **dropped from the thesis
plan** — stays future-work-only, never executed. This session did not touch the `.docx` (user
edits it by hand); it produced `docs/VODIC_ZA_DOVRSETAK_RADA.md`, a single consolidated,
re-verified completion guide merging two prior-session docs that had gone untracked:
`EVALUACIJA_diplomskog_rada.md` (content/organization review) and `USKLADENOST_s_uputama_FESB.md`
(FESB format compliance) — both now committed. `NACRT_3.5_samoigra_i_5.2_mjerne_velicine.md`'s
draft text (self-play/ELO §3.5, metrics §5.2) has already been merged into the docx by the user.

**Fresh docx extraction (zip → `word/document.xml`, 458 paragraphs) found the evaluation docs partly
stale** — several items already fixed (Ch.6 title, table titles, "shaping-farming" naming, §6.4.2
sparse→shaped fix), one major claim now wrong (the "ready to insert" main 5M sparse-vs-shaped figure
doesn't exist — `Theory.md` §12 says that seed-aggregation is still pending, no script for it exists
yet), and a new problem introduced by the Phase-B-drop decision: the docx still frames Phase B as
upcoming active research (odl. 371, 369, 402) rather than dropped future work. Also cross-checked
γ-sweep numbers against `Theory.md` §14 and caught a live inaccuracy in the docx (peak catch rate
1.00 is at γ=0.95, not γ=0.99 as currently written) plus a still-unfixed conflated arena-throughput
stat (4-arena smoke value mixed with the 12→16-arena bake-off pair).

**Guide includes:** a merged, re-verified priority checklist (✅/🔴/⚠️ status per item); the T12
(all 9 Phase A runs), T-val (400k), and T-ppo/T-probe/T-g-probe tables pre-filled from `Theory.md`,
ready to paste; a figure map (7 of 11 numbered TensorBoard figures exist and are ready to insert;
the main 5M figure needs a new analysis script first; ~8 more need diagrams/screenshots); a full
23-entry Popis oznaka i kratica draft; a code-listing shortlist for the empty "Ostali prilozi"
section. **Explicitly deferred** (per user instruction — these summarize the whole work and
shouldn't be written until Phase B's framing is fully resolved): Zaključak, Sažetak/Ključne riječi,
Title/Summary/Keywords, and the Uvod "seven chapters" count fix.

**Git:** new branch `docs/thesis-completion-guide` off `feat/obstacles-gamma-sweep`; committed the
3 previously-untracked markdown docs + `.gitignore` entries for the two binary Office files (the
thesis `.docx` and the FESB `Upute za pisanje diplomskog rada.doc` stay untracked). Left an
unrelated pre-existing unstaged `ProjectSettings.asset` Unity-editor diff untouched. **Next:** once
the user has worked through the guide's priority list and the Phase B framing is settled, draft the
deferred whole-work sections; no merge to main without explicit approval.

---

## 2026-07-10 — Phase A COMPLETE + analyzed: γ=0.99 vindicated; 0.995 bimodal; pillars cost ~nothing

All 9 fixed-pillar runs finished 5M (incl. the paused-and-resumed `g090_s1`). **Validity:** every
player log shows `num_obstacles=4, layout=fixed` + correct gamma. Results (Theory §14, Figs 8–11,
`docs/figures/gamma/`, data via new `experiments/analysis/{parse_tb,plot_gamma}.py`):

- **Rise-to-0.99 CONFIRMED** (catch 0.86→0.96→1.00→0.99; ELO gap 946→1249; monotone).
- **γ=0.8 myopia tax:** slowest learning, catch 0.74–0.93, TimeToCatch 2.5–4× — but ALL seeds
  still beat the runner (outcome part of the prediction refuted: myopia degrades, doesn't prevent).
- **γ=0.995 BIMODAL:** s2/s3 = best of the whole sweep (catch ≈1.0, gap ≈1255); s1 stuck at ~0
  catch for ~3.5M steps, late partial recovery (gap 395) → long horizon = high-risk/high-reward.
- **Fixed cover costs ~nothing at γ≥0.95** (catch ≈ open-arena ~1.0; only ~25 % longer episodes) —
  negative result for "cover helps the evader"; Phase B (randomization) is the real test.
- **Empirical answer to "why γ=0.99":** fastest learner, plateau performance, no instability.

Also: gamma-probe figure added (Fig 8, the (1−γ) harvest ladder); Theory contradiction pass
(fixed §10 stale arena count; §11/§12 "reward for being close" formally sign-corrected → §14);
§8 status refreshed; **§15 added** (future work + project verdict). **Final whole-branch code
review:** no critical issues; important catch = random layouts weren't `--seed`-covered → **fixed
(`ca64ed0`, seeded from UnityEngine.Random)** ⇒ **rebuild + obs smoke REQUIRED before Phase B.**
Decision gate: criteria met, fallback not triggered → **GO for Phase B** after the rebuild gate.

---

## 2026-07-07 — Sprint 2 code done; gamma probes DONE: no rescue, (1−γ) scaling confirmed

**Probes finished (5M each).** `POCA_shaped_g{080,090}_s1` vs the γ=0.99 3-seed baseline: catch
rate pinned ~0.01 in ALL three shaped conditions (Group Reward ≈ −1, episodes at cap) — **low γ
does not rescue the farming trap** (§14 primary prediction confirmed, falsification condition
absent). Quantitative fingerprint: step-penalty-corrected shaping harvest 6.5 / 52.8 / 124.8 ≈
**1 : 8 : 19** vs pre-registered 1 : 10 : 20 from the (1−γ) standing term; implied mean
chaser–runner distance ~0.5–0.65 of the diagonal ⇒ the chaser **farms from afar** (§11's
"reward for being close" formally corrected in §14). Full write-up: Theory §14 probe results.

**Sprint 2 (subagent-driven, all reviewed 2-stage):** `ObstaclePlacement` pure math + 8 EditMode
tests (`8ed2857`+`ad7be39` — review hardened the failure contract: clears result on failure);
`ObstacleManager` (`89f05b3`+`1553514`+`1addd84` — review caught 2 real batch-killers: RNG seed
correlation across 16 arenas + stale raycasts from `autoSyncTransforms=0`, both fixed);
`TagArenaManager` obstacle-aware spawns (`a58ae02` — RNG-stream-identical for num_obstacles=0,
protects all baselines); Phase A/B batch scripts (`043cfb6`). Scene note: `Scene_V2` holds **16**
arena instances (docs previously said 8).

**Next:** USER authors 4 pillars in `TagArena.prefab` (guide provided) → Claude reviews prefab →
EditMode 13/13 → rebuild headless binary #2 → `TagMApoca_obs_smoke` gate (random mode) → launch
Phase A (9 runs, ~36–42 h) → decision gate → Phase B.

---

## 2026-07-06 — Obstacles × gamma sweep: spec + plan + Sprint 1 apparatus done

New branch `feat/obstacles-gamma-sweep` (off `feat/ppo-comparison`). Brainstormed + spec'd the
next phase (spec `2026-07-04-obstacles-gamma-sweep-design.md`, plan same date): **5-point gamma
sweep** (0.8–0.995, sparse, 4-pillar arenas fixed→random, 3 seeds at endpoints) + **2 shaped
low-γ probes** of the farming trap. Key pre-registered prediction (Theory §14): the PBS standing
term scales with (1−γ), so **lower γ should farm WORSE** — either outcome is citable.

Sprint 1 done (subagent-driven): `shaping_gamma` env-param in `TagAgent` (fallback = inspector ⇒
old configs byte-identical, one-time param log for smoke checks), config generator → 14 YAMLs
(2 probes, 10 sweep, 2 smoke gates; UTF-8 fix after review), `run_gamma_probes.bat`, Theory §14
expectations pre-registered. **Next:** USER rebuilds headless binary → `TagMApoca_gprobe_smoke`
gate → launch probes overnight; Sprint 2 (obstacle system) builds while they run.

---

## 2026-07-04 — Follow-up done: farming trap has TWO causes (delivery + algorithm)

`POCA_shaped_indivterm_s1` (POCA shaped + `individual_terminal_reward:1.0`, 5M) finished. **Partial
rescue:** catch rate ~0.01 → **~0.12** (≈10×, still rising to ~0.16 at 5M), chaser ELO 1259→1321, Group
Reward −0.99→−0.86 — but nowhere near PPO_shaped's 0.98 (episodes still ~393 near cap).

**Refined conclusion (Theory §13):** the shaped farming trap has **two contributing causes** — (1)
routing the terminal only via the group channel (fixing it helps ~10×, *necessary*) AND (2) an
algorithm-level susceptibility in MA-POCA's centralized-critic/counterfactual-baseline credit assignment
to dense shaping (PPO, with the same individual terminal, escaped by ~2M; POCA barely lifts even with the
terminal in BOTH channels — *not sufficient*). Design takeaway: under dense PBS shaping in grouped
MA-POCA, deliver the terminal individually too (it helps) but expect slow escape; for this task the
robust choice is simply the **sparse** reward (§12).

New figure `docs/figures/ppo/tb_probe_delivery.png` (Fig 7): 3 shaped conditions, catch rate 1%→12%→98%.
§13 now complete (2×2 Figs 5–6 + probe Fig 7). Next: `finishing-a-development-branch`.

---

## 2026-07-03 — PPO 2×2 complete: farming trap is a reward-DELIVERY effect, not algorithm

Both PPO runs finished full 5M (`PPO_sparse_s1`; `PPO_shaped_s1` paused overnight at ~300k, resumed via
`--resume`). Pulled final metrics from tfevents (dependency-free parser, since default python lacks
tensorboard) — verified with catch rate + ELO, not just ELO.

### The 2×2 (catch rate = shaping-independent outcome)
| | sparse | shaped |
|---|---|---|
| **POCA** | ~1.00 (chaser crushes) | **~0.01 — FARMING** (0/0/0.016, 399-step stalemates, GroupR −1) |
| **PPO**  | 0.90 (chaser wins) | **0.98 — chaser WINS** (58-step episodes, fastest catcher of all 4) |

Three cells: chaser dominates. Only **POCA_shaped** farms. **PPO_shaped escaped the trap.**

### Finding (refined)
- **Sparse equivalence CONFIRMED:** MA-POCA ≈ PPO at 1v1 on pure reward (both catch ~0.9–1.0).
- **"Trap is algorithm-independent" REFUTED — and productively:** the cause is **where the terminal
  reward is delivered**, not the algorithm. POCA reads the terminal via the **group** channel
  (`individual_terminal_reward` off) → chaser's individual stream is step+shaping only → farms. PPO
  needs it **individually** (ignores group rewards) → +1-per-catch competes with shaping → catches.
  Invisible in the sparse arm (no shaping); only bites under dense shaping. Full write-up: Theory §13.

### Follow-up designed + configured (isolates the cause within MA-POCA)
Created `config/poca/TagMApoca_shaped_indivterm.yaml` (+ archived) = POCA shaped **+
`individual_terminal_reward: 1.0`**. Run-id `POCA_shaped_indivterm_s1`, seed 1. **Prediction:** if
delivery channel is the cause, this POCA run escapes farming (catch rate ~0 → toward ~0.98). Command:
```
mlagents-learn config/poca/TagMApoca_shaped_indivterm.yaml --env="...\Build\TagMApoca_V1.exe" --no-graphics --run-id=POCA_shaped_indivterm_s1 --seed 1
```
(uses the SAME rebuilt headless binary — flag code already in it.)

### Still to add
Figures (TB→Playwright) for the four cells; the follow-up run result; then `finishing-a-development-branch`.

---

## 2026-07-02 (cont.) — PPO apparatus built + smoke gate PASSED (GO)

Executed the PPO plan via subagent-driven development (branch `feat/ppo-comparison`):
- **`b281945`** — `individual_terminal_reward` guarded mirror in `TagArenaManager` (spec + code-quality
  review both approved). Mirrors terminal ±1 (+bonuses) via `Agent.AddReward`; flag defaults off ⇒ POCA
  path byte-identical.
- **`bac632b`** — PPO configs `TagMApoca_ppo_{sparse,shaped,smoke}.yaml` (ml-agents `config/ppo/` +
  archived in `experiments/configs/`; spec review passed).
- **`e0b5a2a`** — `experiments/run_ppo.bat` (2 runs, seed 1, `--no-graphics`).

**Smoke gate `PPO_smoke_01` (50k, in-Editor, shaped) — all 3 criteria PASSED → GO:**
1. Genuinely PPO — tags show `Policy Loss`+`Value Loss`, **no `Baseline Loss`** (POCA-only term absent).
2. Tolerates grouped agents — clean run, checkpoints+`.onnx`, ELO computed (Chaser 1203.5 / Runner 1199.0).
3. Individual terminal reward reaches PPO — **Runner reward ≈ +2.9** vs ~+2.0 expected without the flag
   (runner has no shaping; the extra ≈+1.0 = the individual +1 survival terminal). Chaser ≈ +0.70.
Approach 1 kept (no per-agent-EndEpisode fallback needed). Full method + evidence: `docs/Theory.md` §13.

**Next:** USER rebuilds the headless player (binary must contain the new flag code) → runs
`experiments/run_ppo.bat` (2×5M, ~1 overnight) → then 2×2 analysis + Theory §13 results + finish branch.

---

## 2026-07-02 — Results committed; PPO comparison designed (2×2), plan next

- **5M results committed + pushed** (`ae77192` on `feat/sparse-vs-shaped-comparison`): 12 brains in
  `Assets/Models/5M/` + Theory §12. User watched the brains in the Editor (Inference wiring committed
  `3f16f13`).
- **New branch `feat/ppo-comparison`** (off the results branch).
- **Brainstormed the PPO arm** and re-scoped it in light of the 5M reversal: the parent spec's single
  "PPO shaped" run became a **full 2×2 (algorithm × reward)** — add `PPO_sparse_s1` + `PPO_shaped_s1`
  (1 seed each, seed 1, 5M) vs the 3-seed POCA bands. Decisions: **Approach 1** guarded
  `individual_terminal_reward` flag (mirror terminal ±1 via `AddReward`, POCA path byte-identical);
  **mandatory PPO smoke-test gate** before 5M; separate `TagMApoca_ppo_{sparse,shaped}.yaml` configs.
  Spec: `docs/superpowers/specs/2026-07-02-ppo-comparison-design.md` (`d178867`).
- **Next:** writing-plans → implement (flag + configs + `run_ppo.bat`), smoke-test PPO, then the two 5M
  runs, then 2×2 write-up (Theory §13) + seed-aggregation figures, then finish the branch.

---

## 2026-06-22 — 5M batch COMPLETE; result inverts vs 400k (shaping farms, sparse wins)

All 6 runs finished a full 5M steps (batch 00:25 21st → 01:36 22nd). All have `Chaser.onnx` +
`Runner.onnx`; final brains copied into `Assets/Models/5M/` (12 files, `{arm}_s{n}_{role}.onnx`).

### THE finding — the 400k ranking reversed at 5M
- **Sparse (no shaping):** chaser **dominates** — s1 Chaser ELO 1890.7 / GroupR **+1.45**; s2 Runner
  ELO 685 / GroupR −0.87; s3 Runner ELO 661 / GroupR −0.94. Emergent pursuit from pure terminal reward.
- **Shaped (PBS coef 0.5):** chaser **loses every seed** — s1 GroupR −0.98 (MeanR 5.38), s2 GroupR
  −1.00 (MeanR 3.93, paired Runner +1.00), s3 GroupR −0.96 (MeanR 4.29). High Mean Reward + ≈−1 Group
  Reward = **proximity-farming local optimum**: chaser hovers near runner harvesting PBS reward, never
  commits to catches.
- **Why it matters (thesis):** (1) short-horizon validation (400k, where shaping looked ~3× better)
  *inverted* at scale — vindicates the §11 "400k is short" caveat; (2) PBS policy-invariance guarantees
  the optimum, not that the learner reaches it — the pre-registered γ<1 "standing reward for being
  close" + weakened invariance under self-play is exactly what manifested; (3) sparse terminal reward is
  sufficient AND superior here. Full write-up: `docs/Theory.md` §12 (rewritten).

### Next steps (this session's plan)
1. **USER watches brains** in Editor (Inference Only; pair Chaser+Runner from same run): sparse_s1 →
   chaser chases & catches; shaped_s1 → chaser hovers/farms. Visual confirmation of the farming story.
2. **Commit results to a NEW branch** (brains in `Assets/Models/5M/` + doc updates).
3. **One PPO run** for the MA-POCA-vs-PPO comparison arm (needs PPO config + `individual_terminal_reward`
   handling since PPO ignores group rewards — verify early).
4. Seed-aggregation script (mean ± std, error-band figures) + Playwright TB figures into §12; then
   `finishing-a-development-branch`.

---

## 2026-06-21 — 5M overnight batch running; sparse arm done = emergent chaser pursuit

The unattended 6-run batch (`experiments/run_overnight_poca.bat`, headless `--no-graphics`, 16 arenas,
5M steps/behavior each) is running. Started ~00:25; ~4.6 h/run.

### Batch status (read from `batch_logs/` + `results/`)
| Run | Status |
|---|---|
| `POCA_sparse_s1` / `s2` / `s3` | ✅ **Done** (5M, `Chaser.onnx` + `Runner.onnx` exported) |
| `POCA_shaped_s1` | 🔄 Running (~1.8M / 5M at check time) |
| `POCA_shaped_s2` / `s3` | ⏳ Queued (105-byte stub logs are harmless leftovers from an earlier non-conda attempt; overwritten when reached) |

Whole batch expected to finish ~03:00–03:30 on 2026-06-22. The terminal stays on the "Starting…" line
by design (each run's output is redirected to `batch_logs\*.log`; `.bat` prints "Batch complete" only
after all 6).

### HEADLINE — sparse arm (pure terminal reward) produces decisive emergent pursuit at 5M
Final console figures (per-behavior, single seed each — preliminary until aggregated):
- `POCA_sparse_s1`: **Chaser ELO 1890.7, Mean Group Reward +1.45** (catches, and catches *fast*).
- `POCA_sparse_s2`: Runner ELO 685.5, Runner Group Reward −0.87 (runner loses ⇒ chaser dominates).
- `POCA_sparse_s3`: Runner ELO 661.1, Runner Group Reward −0.94 (runner loses ⇒ chaser dominates).
- **ELO gap ≈ 1200 pts chaser-favored** (vs ~22 pts at the 400k validation horizon), **consistent
  across all 3 seeds.** This answers the research question's first half: **emergence happens without
  shaping** given enough steps. Full write-up: `docs/Theory.md` §12.

### Caveats recorded in Theory.md §12
Console numbers are per-behavior/single-seed; catch rate, episode length, and mean ± std must come from
aggregated TensorBoard data. Shaped arm not yet in (early shaped_s1 still mid-climb). No sparse-vs-shaped
5M claim until shaped seeds finish.

### Next session (resume here)
1. Confirm all 6 runs complete (6 `results/POCA_*` folders, each with `Chaser.onnx` + `Runner.onnx`).
2. Build the **seed-aggregation script** (mean ± std across seeds → error-band figures) and run it on
   both arms → sparse-vs-shaped 5M figures + headline numbers into `docs/Theory.md` §12.
3. Capture TensorBoard figures (Playwright) for the 5M runs; verify `Environment/TimeToCatch` now nonzero.
4. Then: PPO sanity run (deferred), then `superpowers:finishing-a-development-branch`.
5. (Fun) import a sparse 5M `.onnx` into the prefab (Behavior Type = Inference) and watch the trained
   chaser play — **only after the batch finishes** (Editor Play competes with the headless batch for CPU).

---

## 2026-06-20 — Validation analysis, TimeToCatch fix, arena bake-off, 5M setup

(Validation sparse-vs-shaped results captured + analyzed — see the 2026-06-17 "Task 5" section and
`docs/Theory.md` §11; figures in `docs/figures/validation/`.)

### New brainstorm → spec: the "1v1 rigor phase"
Decided (phased) to lock a thesis-grade 1v1 result before any environment expansion. Spec:
`docs/superpowers/specs/2026-06-20-1v1-rigor-phase-design.md` (commit `2351981`). Matrix = MA-POCA
{sparse, shaped} × 3 seeds + 1 PPO sanity (PPO **deferred** to run after POCA). Key insight: at 1v1
(singleton groups) MA-POCA ≈ PPO, so the *real* PPO-vs-POCA comparison belongs in the team-expansion
phase.

### TimeToCatch bug — FIXED + verified (commit `090f4b5`)
Root cause (systematic-debugging): `stats.Add("Environment/TimeToCatch", stepCount)` ran *after*
`EndGroupEpisode()`, which synchronously triggers the chaser's `OnEpisodeBegin → ResetArena` and
zeroes `stepCount` → always logged 0. Fix: record `Catch`/`TimeToCatch` *before* the group-end calls
(both `OnAgentTagged` and `TriggerStalemate`). Verified by the bake-off smoke runs (now logs ~290–585
physics steps, not 0).

### Arena bake-off — 16 chosen
50k smoke, in-Editor: **12 arenas = 495 steps/s, 16 arenas = 553 steps/s (+12%)**; per-arena
efficiency 41→35 (near the saturation knee). **Sticking with 16.** Caveat: in-Editor measurement;
headless frees rendering CPU so its knee is higher (worth re-checking ≥16 against the headless build).

### Artifacts created (commit `f487803`)
- `TagMApoca_sparse_5M.yaml` / `TagMApoca_shaped_5M.yaml` (ml-agents `config/poca/` + archived in
  `experiments/configs/`).
- `experiments/run_overnight_poca.bat` — unattended 6-run batch (sparse×3, shaped×3) against the
  headless build, per-run logs.

### Next session (resume here)
1. **User builds the headless player** (Unity: File > Build Settings > Windows Standalone > Build),
   scene at 16 arenas, agents in **Default** behavior type with empty Model. Set the `ENV=` path in
   `run_overnight_poca.bat`.
2. (Optional) re-measure ~20–24 arenas against the headless build to see if a higher count wins.
3. Run the overnight batch → 6 runs → trained `.onnx` per run + 250k-step checkpoints.
4. Capture/aggregate (mean ± std across seeds), write results into `docs/Theory.md`.
5. Then wire + run the deferred PPO sanity run; then `finishing-a-development-branch`.

**Watching progress in Unity:** the validation `.onnx` already exist (e.g.
`results/TagVal_shaped_01/Chaser.onnx`/`Runner.onnx` + checkpoints at 100k/200k/300k/400k under
`TagVal_shaped_01/Chaser/`). Import into the prefab's Behavior Parameters > Model, set Behavior Type =
Inference, press Play (no trainer) to watch. The 5M runs will produce stronger brains + more checkpoints.

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

### Task 5 — BOTH arms DONE + analyzed (2026-06-20)
`TagVal_sparse_01` (coef 0) and `TagVal_shaped_01` (coef 0.5), same seed 12345, 400k, 8 arenas.
Data pulled via the TensorBoard data API; curves captured with Playwright →
`docs/figures/validation/{tb_overview,tb_elo,tb_catch_episodelen}.png`. Full write-up in
**`docs/Theory.md` §11**.

**Result — both arms learn; shaping clearly accelerates it (no fallback needed):**

| metric (final window) | Sparse | Shaped |
|---|---|---|
| ELO gap (Chaser−Runner) | +21.9 | **+72.7** (≈3×) |
| Catch rate (Chaser) | ~0.08 | **~0.21** (≈2.5–3×) |
| Episode length (Chaser) | 386 | **374** |
| Group Cum. Reward — Chaser (shaping-independent) | −0.91 | **−0.75** |

Key point: `GroupCumulativeReward` (the ±1 game outcome, identical across arms, NOT inflated by the
shaping term) improved more in the shaped arm → genuinely more wins, not just bigger reward numbers.
Caveats: 400k is short (both still near baseline absolutely, entropy ~1.43); `CumulativeReward` not
comparable across arms (includes shaping); γ<1 weakens strict PBS invariance slightly.

**Known bug found:** `Environment/TimeToCatch` logs all-zeros — the value written at catch isn't the
intended step count. Episode Length is the working time-to-catch proxy. **Fix before it's citable.**

### Next session (resume here)
1. **Fix the `TimeToCatch` stat** (it writes 0 — investigate what `stepCount` holds at `OnAgentTagged`).
2. Build a **headless `--no-graphics` standalone** (Theory.md §10) for the long run.
3. Run the **multi-M comparison** (optionally + seeds for variance, + PPO-vs-MA-POCA arm).
4. When ready to integrate this branch → `superpowers:finishing-a-development-branch`.

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
