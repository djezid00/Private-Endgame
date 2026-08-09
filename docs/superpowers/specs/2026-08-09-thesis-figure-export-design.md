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
are; Phase B — not run, and dropped from the thesis per `EVALUACIJA_diplomskog_rada.md` (N2); the
13 hand-drawn diagrams and screenshots the evaluation doc lists for chapters 3–5.

## Numbering

Figures are numbered to match the chapter-6 outline in `VODIC_ZA_DOVRSETAK_RADA.md`, which counts
11 numbered figures in the chapter. Under FESB, figures run sequentially within a chapter
regardless of subsection, so the three figures produced elsewhere take the first slots:

| Slot | Figure | Owner |
|---|---|---|
| 6.1 | `Losses/Baseline Loss` beside `Losses/Value Loss` (§6.1) | not this work |
| 6.2 | 400k validation, ELO divergence (§6.2) — `figures/validation/tb_elo.png` | not this work |
| 6.3 | 400k validation, catch rate + episode length — `tb_catch_episodelen.png` | not this work |
| 6.4 – 6.11 | 5M main figure, PPO, γ | **this work** |

> **Accepted risk:** the numbers are baked into filenames. If chapter 6 is restructured again — for
> example if the optional `tb_overview.png` / `tb_policy.png` are also inserted at §6.2 — every
> file from that point on must be renamed and its caption re-edited by hand. This was chosen over
> an index-driven scheme for simplicity of insertion.

FESB requires `Slika 6.4.` (dot separators, caption **below** the image, centred). Filenames use a
dash — `Slika_6-4_…` — because a trailing dot in a filename is fragile on Windows. The caption text
in `INDEX.md` uses the correct dotted form.

The thesis currently writes `Slika 2-1` with a dash throughout and needs a global Find & Replace to
dots (`USKLADENOST_s_uputama_FESB.md` §4.2). That fix is *not* part of this work.

## Deliverables

```
docs/slike/
├── INDEX.md                    # number → Croatian caption → source runs → producing script
├── poglavlje6/                 # PNG 300 DPI + SVG per figure
│   ├── Slika_6-4_sparse_vs_shaped_5M.{png,svg}
│   ├── Slika_6-5_ppo_stopa_hvatanja.{png,svg}
│   ├── Slika_6-6_ppo_elo.{png,svg}
│   ├── Slika_6-7_sonda_isporuke.{png,svg}
│   ├── Slika_6-8_gama_sonde_zetva.{png,svg}
│   ├── Slika_6-9_gama_krivulje_ucenja.{png,svg}
│   ├── Slika_6-10_gama_osjetljivost.{png,svg}
│   └── Slika_6-11_gama_elo_9_pokreta.{png,svg}
└── prilog_tensorboard/         # PNG only
    └── Slika_P-1_… … Slika_P-n_…
```

### Figure content

| № | Content | Scalar tags |
|---|---|---|
| 6.4 | **Main figure.** Three stacked panels — `Self-play/ELO`, group reward, individual reward — sparse vs shaped, 3 seeds, min–max band. The individual-reward panel is the visual proof of farming: it stays high while group reward sits at −1. | `Self-play/ELO`, `Environment/Group Cumulative Reward`, `Environment/Cumulative Reward` |
| 6.5 | Catch rate for all four 2×2 cells; only POCA+shaped stays at ~0.01 | `Environment/Catch` |
| 6.6 | Chaser ELO, 2×2 | `Self-play/ELO` |
| 6.7 | Delivery-channel probe, three curves: group-only ~0.01 / group+individual ~0.12 / individual-only 0.98 | `Environment/Catch` |
| 6.8 | γ probes: catch below 1 % for every γ, with the harvest ladder (+122.8 / +50.8 / +4.5) on the same figure | `Environment/Catch`, `Environment/Cumulative Reward` |
| 6.9 | Learning curves per γ with min–max band — the γ=0.995 bimodality is visible *only* graphically | `Environment/Catch` |
| 6.10 | Sensitivity: final catch rate and ELO gap vs γ, per-seed points | `Environment/Catch`, `Self-play/ELO` |
| 6.11 | `Self-play/ELO` for all 9 sweep runs; `g0995_s1` is flat to ~4.3M | `Self-play/ELO` |

6.9 and 6.10 are re-emissions of existing `plot_gamma.py` output at the new size and naming. The
other six are new plotting code — notably 6.5–6.7, which exist today only as hand-taken
TensorBoard screenshots with no generating script.

### Rendering parameters

- Width 6.3 in ≈ 16 cm, matching the A4 text column under FESB margins; 6.4 is taller (3 panels).
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
├── plot_5m.py               NEW — 6.4
├── plot_ppo.py              NEW — 6.5–6.7
├── plot_gamma.py            EXTENDED — 6.8–6.11
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

| № | Run filter | Panels |
|---|---|---|
| P.1 | `POCA_(sparse\|shaped)_s[123]` | `Environment/Catch`, group reward, individual reward |
| P.2 | `(POCA\|PPO)_(sparse\|shaped)_s1` | `Environment/Catch`, `Self-play/ELO` |
| P.3 | `POCA_sparse_obsF_g.*` | `Environment/Catch`, `Self-play/ELO` |
| P.4 | `POCA_sparse_s1` and `PPO_sparse_s1` | `Losses/Baseline Loss` beside `Losses/Value Loss` — the POCA run logs both, the PPO run logs no baseline loss at all, which is the visual proof the trainer really was POCA (evaluation doc, chapter 6.1/6.2 slot) |

**Open assumption:** appendix figures are numbered `Slika P.1.`; the FESB Upute may prescribe a
different convention for prilozi. To be confirmed against `Upute za pisanje diplomskog rada.doc`
before the numbers are baked in.

## Boundaries

- `docs/figures/` and `Theory.md` are **not** modified. Theory.md's Fig. 1–11 numbering is the lab
  notebook's own sequence and is intentionally independent of thesis numbering.
- A parallel Claude session is editing `docs/` on this same branch and working tree. All commits
  here stage **explicit paths only** (`docs/slike/`, `experiments/analysis/`,
  `docs/superpowers/specs/`) — never `git add -A` — so the other session's in-progress work cannot
  be swept into these commits.

## Success criteria

1. `make_thesis_figures.py --check` passes for all 8 figures.
2. All 8 figures render as PNG + SVG; in 6.4 the individual-reward panel visibly separates from the
   group-reward panel, which is the farming result.
3. `INDEX.md` gives, for each figure, a caption that can be pasted into Word unchanged.
4. Appendix plates exist for all three phases at identical capture settings.
5. Re-running the generator reproduces every figure from the event files alone.
