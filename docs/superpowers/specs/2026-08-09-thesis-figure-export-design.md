# Design — Thesis Figure Export (numbered figures for the .docx / .pptx)

> Turns the finished experimental results (Theory.md §12–§14) into a numbered, ready-to-insert
> figure set for the Croatian thesis `ANALIZA KOMPETITIVNE INTERAKCIJE…docx`, plus a TensorBoard
> screenshot appendix. Branch: `docs/thesis-completion-guide`.

## Goal

Produce every chapter-6 figure as a print-quality file whose **filename already carries its FESB
number**, together with ready-to-paste Croatian captions, so inserting them into Word or
PowerPoint is mechanical. Two tracks:

- **Primary track** — matplotlib plots rendered from the raw `tfevents` files. These are the
  figures that go in the body of the thesis.
- **Appendix track** — TensorBoard dashboard screenshots captured at uniform settings, as
  evidence that the plots come from real training runs.

## Scope

Covers three experimental phases:

| Theory § | Phase | Runs |
|---|---|---|
| §12 | 5M sparse vs shaped, 3 seeds | `POCA_{sparse,shaped}_s{1,2,3}` |
| §13 | PPO 2×2 + delivery-channel probe | `PPO_{sparse,shaped}_s1`, `POCA_shaped_indivterm_s1` |
| §14 | γ sweep Phase A + shaped γ probes | `POCA_sparse_obsF_g{080,090,095,099,0995}_s*`, `POCA_shaped_g{080,090}_s1` |

**Out of scope:** the 400k validation figures (§11, `docs/figures/validation/`) — they stay as they
are; Phase B — **deferred, not cancelled**: it runs after the Phase A theory and write-up are
finished, so its figures join this pipeline in a later pass (note that
`EVALUACIJA_diplomskog_rada.md` N1/N2 wrongly record it as dropped); the
13 hand-drawn diagrams and screenshots the evaluation doc lists for chapters 3–5.

## Placement, not numbering

Figure numbers are assigned by the author in Word, not by this pipeline. Filenames are therefore
**descriptive and stable** — they never need renaming when chapter 6 is reordered.

What this work owes the author instead is **placement**: for every figure, which section it belongs
in and which sentence it illustrates. `INDEX.md` carries that as its primary column, sourced from
the chapter-6 outline in `VODIC_ZA_DOVRSETAK_RADA.md`:

| File | Belongs in | Illustrates |
|---|---|---|
| `sparse_vs_shaped_5M` | §6.3 Glavni rezultati | the untitled image currently sitting under the heading |
| `ppo_stopa_hvatanja`, `ppo_elo` | new §6.5 (PPO 2×2) | the missing results for the §5.4 experiment design |
| `sonda_isporuke` | new §6.5 | the delivery-channel conclusion in §6.4.2 |
| `gama_sonde_zetva` | new §6.6 (γ probes) | resolves the dangling `[pogl. X.Y]` references A10/A11 |
| `gama_krivulje_ucenja`, `gama_osjetljivost`, `gama_elo_9_pokreta` | new §6.6 (γ sweep) | the γ discussion in §6.4.3 / §5.6, which currently has no data |

For reference only, non-binding: under the current outline these would fall at `Slika 6.4.`–`6.11.`,
since §6.1 takes the BaselineLoss plate and §6.2 the two 400k validation figures.

FESB style points that do apply: captions go **below** the image, centred, with dot separators
(`Slika 6.4.`). The thesis currently writes `Slika 2-1` with a dash throughout and needs a global
Find & Replace (`USKLADENOST_s_uputama_FESB.md` §4.2) — that fix is *not* part of this work.

## Deliverables

```
docs/slike/
├── INDEX.md                    # placement → Croatian caption → source runs → producing script
├── poglavlje6/                 # PNG 300 DPI + SVG per figure
│   ├── sparse_vs_shaped_5M.{png,svg}
│   ├── ppo_stopa_hvatanja.{png,svg}
│   ├── ppo_elo.{png,svg}
│   ├── sonda_isporuke.{png,svg}
│   ├── gama_sonde_zetva.{png,svg}
│   ├── gama_krivulje_ucenja.{png,svg}
│   ├── gama_osjetljivost.{png,svg}
│   └── gama_elo_9_pokreta.{png,svg}
└── prilog_tensorboard/         # PNG only
    ├── tb_5M_sparse_vs_shaped.png
    ├── tb_ppo_2x2.png
    ├── tb_gama_sweep.png
    └── tb_baseline_vs_value_loss.png
```

### Figure content

| File | Content | Scalar tags |
|---|---|---|
| `sparse_vs_shaped_5M` | **Main figure.** Three stacked panels — `Self-play/ELO`, group reward, individual reward — sparse vs shaped, 3 seeds, min–max band. The individual-reward panel is the visual proof of farming: it stays high while group reward sits at −1. | `Self-play/ELO`, `Environment/Group Cumulative Reward`, `Environment/Cumulative Reward` |
| `ppo_stopa_hvatanja` | Catch rate for all four 2×2 cells; only POCA+shaped stays at ~0.01 | `Environment/Catch` |
| `ppo_elo` | Chaser ELO, 2×2 | `Self-play/ELO` |
| `sonda_isporuke` | Delivery-channel probe, three curves: group-only ~0.01 / group+individual ~0.12 / individual-only 0.98 | `Environment/Catch` |
| `gama_sonde_zetva` | γ probes: catch below 1 % for every γ, with the harvest ladder (+122.8 / +50.8 / +4.5) on the same figure | `Environment/Catch`, `Environment/Cumulative Reward` |
| `gama_krivulje_ucenja` | Learning curves per γ with min–max band — the γ=0.995 bimodality is visible *only* graphically | `Environment/Catch` |
| `gama_osjetljivost` | Sensitivity: final catch rate and ELO gap vs γ, per-seed points | `Environment/Catch`, `Self-play/ELO` |
| `gama_elo_9_pokreta` | `Self-play/ELO` for all 9 sweep runs; `g0995_s1` is flat to ~4.3M | `Self-play/ELO` |

`gama_krivulje_ucenja` and `gama_osjetljivost` are re-emissions of existing `plot_gamma.py` output
at the new size and naming. The other six are new plotting code — notably the three PPO figures,
which exist today only as hand-taken TensorBoard screenshots with no generating script.

### Rendering parameters

- Width 6.3 in ≈ 16 cm, matching the A4 text column under FESB margins; the main 5M figure is
  taller (3 stacked panels).
- PNG at 300 DPI for insertion; SVG of the same figure for lossless rescaling.
- Colour-blind-safe palette already used in `plot_gamma.py` (Okabe–Ito), reused unchanged so the
  new figures match the two existing ones.
- Seed aggregation: rolling mean of the per-seed curves, min–max band across seeds (the
  convention `plot_gamma.py` already established).

## Code

```
experiments/analysis/
├── parse_tb.py              unchanged — dependency-free tfevents parser
├── tb_style.py              NEW — palette, style_ax, rolling, seed-band helper
├── figspec.py               NEW — numbering manifest + Croatian caption strings
├── plot_5m.py               NEW — sparse_vs_shaped_5M
├── plot_ppo.py              NEW — ppo_stopa_hvatanja, ppo_elo, sonda_isporuke
├── plot_gamma.py            EXTENDED — the four gama_* figures
└── make_thesis_figures.py   NEW — runs all producers, regenerates INDEX.md
```

`figspec.py` holds one record per figure (number, slug, Croatian caption, source runs, producer),
so the caption text and the filename can never drift apart, and `INDEX.md` is generated rather
than maintained by hand.

`tb_style.py` is extracted from the styling already inside `plot_gamma.py`; `plot_gamma.py` is then
rewritten to import it, keeping its current two figures byte-comparable in appearance.

### Error handling

The main risk is silent failure: 30 run directories, hand-typed run IDs, and scalar tags that
differ per trainer. A typo currently yields an empty axis rather than an error.

- `make_thesis_figures.py --check` resolves every run directory and every required tag and prints
  a table, exiting non-zero on the first missing run or tag. No plotting.
- The shared series loader raises on an empty result instead of returning an empty array.
- `Self-play/ELO` and `Environment/TimeToCatch` are logged far more sparsely than the rest
  (≈23 and ≈34 points vs 100 over a 5M run), so the loader must not assume equal series lengths
  across tags, and rolling means must degrade gracefully on short series.

### Environment

System Python 3.12.4 has numpy 2.2.4 and matplotlib 3.10.1, so **the plotting scripts run without
conda**. Only TensorBoard itself needs the `mlagents` environment, and it is already running.

## Appendix capture

TensorBoard is served at `localhost:6006` from the user's Anaconda Prompt. Capture is driven with
Playwright at uniform settings — smoothing 0.8, light theme, fixed 1600×1000 viewport, runs
selected by regex per phase — so the plates form a consistent set.

Four plates:

| File | Run filter | Panels |
|---|---|---|
| `tb_5M_sparse_vs_shaped` | `POCA_(sparse\|shaped)_s[123]` | `Environment/Catch`, group reward, individual reward |
| `tb_ppo_2x2` | `(POCA\|PPO)_(sparse\|shaped)_s1` | `Environment/Catch`, `Self-play/ELO` |
| `tb_gama_sweep` | `POCA_sparse_obsF_g.*` | `Environment/Catch`, `Self-play/ELO` |
| `tb_baseline_vs_value_loss` | `POCA_sparse_s1` and `PPO_sparse_s1` | `Losses/Baseline Loss` beside `Losses/Value Loss` — the POCA run logs both, the PPO run logs no baseline loss at all, which is the visual proof the trainer really was POCA. Also fills the §6.1 slot the evaluation doc asks for. |

## Boundaries

- `docs/figures/` and `Theory.md` are **not** modified. Theory.md's Fig. 1–11 numbering is the lab
  notebook's own sequence and is intentionally independent of thesis numbering.
- A parallel Claude session is editing `docs/` on this same branch and working tree. All commits
  here stage **explicit paths only** (`docs/slike/`, `experiments/analysis/`,
  `docs/superpowers/specs/`) — never `git add -A` — so the other session's in-progress work cannot
  be swept into these commits.

## Success criteria

1. `make_thesis_figures.py --check` passes for all 8 figures.
2. All 8 figures render as PNG + SVG; in the main 5M figure the individual-reward panel visibly
   separates from the group-reward panel, which is the farming result.
3. `INDEX.md` gives, for each figure, the section it belongs in and a caption that can be pasted
   into Word unchanged.
4. Appendix plates exist for all three phases at identical capture settings.
5. Re-running the generator reproduces every figure from the event files alone.
