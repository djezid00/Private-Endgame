# Thesis Figure Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate the eight chapter-6 thesis figures as 300 DPI PNG + SVG from the raw TensorBoard event files, with a Croatian `INDEX.md` telling the author where each one belongs, plus four uniform TensorBoard screenshot plates for the appendix.

**Architecture:** A shared `tb_style.py` provides a strict series loader (raises rather than plotting an empty axis), pure aggregation math, and a save helper. `figspec.py` holds the manifest of figures — slug, section, Croatian caption, source runs — and renders `INDEX.md` from it. Three producer modules (`plot_5m.py`, `plot_ppo.py`, `plot_gamma.py`) each own one thesis section. `make_thesis_figures.py` runs them all and supports `--check`, which resolves every run and tag without plotting.

**Tech Stack:** Python 3.12.4 (system install, no conda needed), numpy 2.2.4, matplotlib 3.10.1, stdlib `unittest`. TensorBoard + Playwright MCP for the appendix plates only.

---

## Context the implementer needs

**Where the data lives.** Training outputs are *not* in this repo. They are at
`C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/<run-id>/<Behavior>/events.out.tfevents.*`,
where `<Behavior>` is `Chaser` or `Runner`. Both behaviours log the same tag names.

**Verified facts — do not re-derive:**

- Scalar tags present in POCA runs: `Environment/Catch`, `Environment/Cumulative Reward`,
  `Environment/Group Cumulative Reward`, `Environment/Episode Length`, `Environment/TimeToCatch`,
  `Self-play/ELO`, plus `Losses/*` and `Policy/*`.
- **PPO runs have no `Environment/Group Cumulative Reward`.** It is genuinely absent. Never request
  that tag for a `PPO_*` run — the loader will (correctly) raise.
- Series lengths differ per tag and per run: over a 5M run, `Environment/Catch` has ~100 points
  while `Self-play/ELO` has 23–101 and `Environment/TimeToCatch` 34–101. `PPO_shaped_s1` runs to
  step 5,050,000 while the others stop at 5,000,000.
- Reference values for smoke-checking output (last-5-point means, Chaser):

| Run | Catch | Cumulative Reward | Group Cum. Reward | ELO |
|---|---|---|---|---|
| `PPO_sparse_s1` | 0.9077 | 0.4205 | absent | 1724.7 |
| `PPO_shaped_s1` | 0.9888 | 1.4940 | absent | 1826.2 |
| `POCA_shaped_indivterm_s1` | 0.1836 | 1.6500 | −0.7489 | 1312.2 |
| `POCA_shaped_g080_s1` | 0.0096 | 122.7674 | −0.9895 | 1236.6 |

**Existing code to build on:** `experiments/analysis/parse_tb.py` is a dependency-free tfevents
parser. Its `extract_scalars(path) -> {tag: [(step, value), ...]}` is the only function you need.
Leave it unchanged.

**Known bug to fix while refactoring:** `plot_gamma.py` currently does
`st = st[:n]` where `st` is whatever the *last* loop iteration left behind, so if seeds have
different lengths the x-axis comes from the wrong run. The new `aggregate()` takes the shortest
seed's steps explicitly.

**Git discipline:** a second Claude session is editing `docs/` on this same branch and working
tree. Every commit in this plan stages **explicit paths**. Never run `git add -A` or `git add .`.

---

## File Structure

| File | Responsibility |
|---|---|
| `experiments/analysis/tb_style.py` | CREATE — palette, `style_ax`, `save_figure`, `load_series`, `rolling`, `aggregate`, `seed_band` |
| `experiments/analysis/figspec.py` | CREATE — `Figure` records (slug, section, caption, runs, producer) + `render_index()` |
| `experiments/analysis/plot_5m.py` | CREATE — the 3-panel main figure |
| `experiments/analysis/plot_ppo.py` | CREATE — PPO 2×2 catch, 2×2 ELO, delivery probe |
| `experiments/analysis/plot_gamma.py` | MODIFY — import `tb_style`, add two new figures, output to `docs/slike/` |
| `experiments/analysis/make_thesis_figures.py` | CREATE — driver with `--check` |
| `experiments/analysis/tests/test_tb_style.py` | CREATE — unit tests for pure math + loader errors |
| `experiments/analysis/tests/test_figspec.py` | CREATE — manifest/index tests |
| `docs/slike/poglavlje6/*.{png,svg}` | OUTPUT |
| `docs/slike/prilog_tensorboard/*.png` | OUTPUT |
| `docs/slike/INDEX.md` | OUTPUT — generated, never hand-edited |

Tests run with `python -m unittest discover -s experiments/analysis/tests -t .` from the repo root.
There is no pytest in this environment; do not add one.

---

### Task 1: Strict series loader

**Files:**
- Create: `experiments/analysis/tb_style.py`
- Create: `experiments/analysis/tests/test_tb_style.py`

- [ ] **Step 1: Write the failing test**

Create `experiments/analysis/tests/test_tb_style.py`:

```python
import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

import tb_style


class TestLoadSeries(unittest.TestCase):
    def test_missing_run_raises(self):
        with self.assertRaises(tb_style.MissingSeries) as ctx:
            tb_style.load_series("NoSuchRun_s9", "Chaser", "Environment/Catch")
        self.assertIn("NoSuchRun_s9", str(ctx.exception))

    def test_missing_tag_raises(self):
        with self.assertRaises(tb_style.MissingSeries) as ctx:
            tb_style.load_series("PPO_sparse_s1", "Chaser",
                                 "Environment/Group Cumulative Reward")
        self.assertIn("Group Cumulative Reward", str(ctx.exception))

    def test_loads_real_series(self):
        steps, values = tb_style.load_series("PPO_sparse_s1", "Chaser",
                                             "Environment/Catch")
        self.assertEqual(len(steps), len(values))
        self.assertGreater(len(steps), 50)
        self.assertEqual(steps[-1], 5000000)
        self.assertAlmostEqual(float(values[-5:].mean()), 0.9077, places=3)
        self.assertTrue((steps[1:] >= steps[:-1]).all(), "steps must be sorted")


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: FAIL — `ModuleNotFoundError: No module named 'tb_style'`

- [ ] **Step 3: Write minimal implementation**

Create `experiments/analysis/tb_style.py`:

```python
"""Shared helpers for the thesis figure pipeline: data loading, aggregation, styling."""
import glob
import os

import numpy as np

from parse_tb import extract_scalars

RESULTS_ROOT = r"C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results"


class MissingSeries(Exception):
    """Raised when a run directory or a scalar tag cannot be resolved.

    Loud failure is deliberate: a typo in a run id would otherwise render an
    empty axis that silently ships into the thesis.
    """


def load_series(run, behavior, tag, root=RESULTS_ROOT):
    """-> (steps, values) as float arrays, sorted by step."""
    pattern = os.path.join(root, run, behavior, "events.out.tfevents.*")
    files = sorted(glob.glob(pattern))
    if not files:
        raise MissingSeries(f"no event files for run {run!r} behavior {behavior!r} (looked in {pattern})")
    points = []
    for path in files:
        points.extend(extract_scalars(path).get(tag, []))
    if not points:
        raise MissingSeries(f"tag {tag!r} absent in run {run!r} behavior {behavior!r}")
    points.sort()
    steps = np.array([p[0] for p in points], dtype=float)
    values = np.array([p[1] for p in points], dtype=float)
    return steps, values
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: 3 tests, OK

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/tb_style.py experiments/analysis/tests/test_tb_style.py
git commit -m "feat(analysis): strict tfevents series loader that raises on missing run or tag"
```

---

### Task 2: Aggregation math

**Files:**
- Modify: `experiments/analysis/tb_style.py`
- Modify: `experiments/analysis/tests/test_tb_style.py`

- [ ] **Step 1: Write the failing test**

Append to `experiments/analysis/tests/test_tb_style.py`, above the `if __name__` block:

```python
import numpy as np


class TestRolling(unittest.TestCase):
    def test_preserves_length(self):
        y = np.arange(10, dtype=float)
        self.assertEqual(len(tb_style.rolling(y, 5)), 10)

    def test_short_series_returned_unchanged(self):
        y = np.array([1.0, 2.0, 3.0])
        np.testing.assert_array_equal(tb_style.rolling(y, 5), y)

    def test_smooths_tail(self):
        y = np.array([0.0, 0.0, 0.0, 0.0, 5.0])
        self.assertAlmostEqual(tb_style.rolling(y, 5)[-1], 1.0)


class TestAggregate(unittest.TestCase):
    def test_truncates_to_shortest_seed(self):
        steps = [np.array([0.0, 1.0, 2.0]), np.array([0.0, 1.0])]
        curves = [np.array([1.0, 2.0, 3.0]), np.array([3.0, 4.0])]
        st, mean, lo, hi = tb_style.aggregate(steps, curves)
        self.assertEqual(len(st), 2)
        np.testing.assert_array_equal(st, np.array([0.0, 1.0]))
        np.testing.assert_array_equal(mean, np.array([2.0, 3.0]))
        np.testing.assert_array_equal(lo, np.array([1.0, 2.0]))
        np.testing.assert_array_equal(hi, np.array([3.0, 4.0]))

    def test_single_seed_band_is_flat(self):
        st, mean, lo, hi = tb_style.aggregate([np.array([0.0, 1.0])],
                                              [np.array([2.0, 4.0])])
        np.testing.assert_array_equal(lo, hi)
        np.testing.assert_array_equal(mean, np.array([2.0, 4.0]))

    def test_rejects_empty_input(self):
        with self.assertRaises(ValueError):
            tb_style.aggregate([], [])
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: FAIL — `AttributeError: module 'tb_style' has no attribute 'rolling'`

- [ ] **Step 3: Write minimal implementation**

Append to `experiments/analysis/tb_style.py`:

```python
def rolling(y, window=5):
    """Rolling mean that keeps the original length (leading points unsmoothed)."""
    y = np.asarray(y, dtype=float)
    if len(y) < window:
        return y
    smoothed = np.convolve(y, np.ones(window) / window, mode="valid")
    return np.concatenate([y[: window - 1], smoothed])


def aggregate(steps_list, curves):
    """-> (steps, mean, min, max) truncated to the shortest seed.

    steps_list and curves are parallel lists, one entry per seed. The x-axis
    comes from the shortest seed so runs of unequal length cannot misalign.
    """
    if not curves:
        raise ValueError("aggregate() needs at least one curve")
    n = min(min(len(c) for c in curves), min(len(s) for s in steps_list))
    shortest = min(steps_list, key=len)
    arr = np.vstack([np.asarray(c, dtype=float)[:n] for c in curves])
    return shortest[:n], arr.mean(axis=0), arr.min(axis=0), arr.max(axis=0)


def seed_band(runs, behavior, tag, root=RESULTS_ROOT, window=5):
    """Load several seeds of one run family and aggregate them."""
    steps_list, curves = [], []
    for run in runs:
        steps, values = load_series(run, behavior, tag, root)
        steps_list.append(steps)
        curves.append(rolling(values, window))
    return aggregate(steps_list, curves)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: 9 tests, OK

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/tb_style.py experiments/analysis/tests/test_tb_style.py
git commit -m "feat(analysis): seed aggregation that truncates to the shortest seed"
```

---

### Task 3: Styling and save helper

**Files:**
- Modify: `experiments/analysis/tb_style.py`
- Modify: `experiments/analysis/tests/test_tb_style.py`

- [ ] **Step 1: Write the failing test**

Append to `experiments/analysis/tests/test_tb_style.py`, above the `if __name__` block:

```python
import tempfile


class TestSaveFigure(unittest.TestCase):
    def test_writes_png_and_svg(self):
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt

        fig, ax = plt.subplots()
        ax.plot([0, 1], [0, 1])
        with tempfile.TemporaryDirectory() as out:
            written = tb_style.save_figure(fig, out, "demo_slug")
            plt.close(fig)
            png = os.path.join(out, "demo_slug.png")
            svg = os.path.join(out, "demo_slug.svg")
            self.assertTrue(os.path.exists(png))
            self.assertTrue(os.path.exists(svg))
            self.assertEqual(written, [png, svg])
            self.assertGreater(os.path.getsize(png), 5000)

    def test_creates_missing_output_dir(self):
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt

        fig, ax = plt.subplots()
        ax.plot([0, 1], [0, 1])
        with tempfile.TemporaryDirectory() as root:
            out = os.path.join(root, "nested", "dir")
            tb_style.save_figure(fig, out, "demo")
            plt.close(fig)
            self.assertTrue(os.path.exists(os.path.join(out, "demo.png")))
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: FAIL — `AttributeError: module 'tb_style' has no attribute 'save_figure'`

- [ ] **Step 3: Write minimal implementation**

Append to `experiments/analysis/tb_style.py`:

```python
# --- appearance -------------------------------------------------------------
# Okabe-Ito colour-blind-safe palette, matching the two figures plot_gamma.py
# already produced so old and new figures sit together in one chapter.
INK, MUTED, GRID = "#1a1a1a", "#5f5f5f", "#e6e6e3"
ORANGE, AMBER, GREEN, BLUE, PURPLE = "#D55E00", "#E69F00", "#009E73", "#0072B2", "#CC79A7"

SPARSE, SHAPED = BLUE, ORANGE          # reward arms
POCA_LS, PPO_LS = "-", "--"            # algorithms distinguished by line style

# A4 text column under FESB margins is ~16 cm.
COLUMN_IN = 6.3
DPI = 300


def style_ax(ax):
    ax.grid(True, color=GRID, linewidth=0.8, zorder=0)
    for side in ("top", "right"):
        ax.spines[side].set_visible(False)
    for side in ("left", "bottom"):
        ax.spines[side].set_color(MUTED)
    ax.tick_params(colors=MUTED, labelsize=9)


def save_figure(fig, out_dir, slug):
    """Write <slug>.png (300 DPI) and <slug>.svg into out_dir. -> [png, svg]"""
    os.makedirs(out_dir, exist_ok=True)
    png = os.path.join(out_dir, f"{slug}.png")
    svg = os.path.join(out_dir, f"{slug}.svg")
    fig.savefig(png, dpi=DPI, bbox_inches="tight", facecolor="white")
    fig.savefig(svg, bbox_inches="tight", facecolor="white")
    return [png, svg]
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: 11 tests, OK

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/tb_style.py experiments/analysis/tests/test_tb_style.py
git commit -m "feat(analysis): shared palette, axis styling and PNG+SVG save helper"
```

---

### Task 4: Figure manifest and INDEX.md

**Files:**
- Create: `experiments/analysis/figspec.py`
- Create: `experiments/analysis/tests/test_figspec.py`

- [ ] **Step 1: Write the failing test**

Create `experiments/analysis/tests/test_figspec.py`:

```python
import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

import figspec


class TestManifest(unittest.TestCase):
    def test_has_eight_figures(self):
        self.assertEqual(len(figspec.FIGURES), 8)

    def test_slugs_are_unique(self):
        slugs = [f.slug for f in figspec.FIGURES]
        self.assertEqual(len(slugs), len(set(slugs)))

    def test_every_figure_is_fully_specified(self):
        for fig in figspec.FIGURES:
            self.assertTrue(fig.slug, "slug missing")
            self.assertTrue(fig.section, f"{fig.slug}: section missing")
            self.assertTrue(fig.caption, f"{fig.slug}: caption missing")
            self.assertTrue(fig.runs, f"{fig.slug}: runs missing")
            self.assertIn(fig.producer, {"plot_5m", "plot_ppo", "plot_gamma"})

    def test_no_ppo_run_requests_group_reward(self):
        """PPO never logs Environment/Group Cumulative Reward."""
        for fig in figspec.FIGURES:
            if any(r.startswith("PPO_") for r in fig.runs):
                self.assertNotIn("Environment/Group Cumulative Reward", fig.tags,
                                 f"{fig.slug} asks PPO for a tag it never logs")


class TestRenderIndex(unittest.TestCase):
    def test_index_lists_every_figure(self):
        text = figspec.render_index()
        for fig in figspec.FIGURES:
            self.assertIn(fig.slug, text)
            self.assertIn(fig.section, text)

    def test_index_is_generated_warning_present(self):
        self.assertIn("generiran", figspec.render_index().lower())


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: FAIL — `ModuleNotFoundError: No module named 'figspec'`

- [ ] **Step 3: Write minimal implementation**

Create `experiments/analysis/figspec.py`:

```python
"""Manifest of thesis figures: slug, placement, Croatian caption, source runs.

Figure NUMBERS are not assigned here. The author numbers figures in Word; this
file records WHERE each figure belongs so the numbering can be done by hand.
"""
from dataclasses import dataclass, field

CATCH = "Environment/Catch"
ELO = "Self-play/ELO"
GROUP_R = "Environment/Group Cumulative Reward"
INDIV_R = "Environment/Cumulative Reward"


@dataclass(frozen=True)
class Figure:
    slug: str
    section: str        # where it belongs in the thesis
    illustrates: str    # which sentence/claim it supports
    caption: str        # ready-to-paste Croatian caption text
    runs: tuple
    tags: tuple
    producer: str


FIGURES = [
    Figure(
        slug="sparse_vs_shaped_5M",
        section="§6.3 Glavni rezultati (5M)",
        illustrates="neimenovana slika koja trenutno stoji ispod naslova 6.3",
        caption=(
            "Usporedba rijetke (*sparse*) i oblikovane (*shaped*) nagrade kroz 5 milijuna koraka, "
            "tri sjemena po ruci. Gornji panel: ELO iz samoigre; srednji: grupna kumulativna "
            "nagrada; donji: individualna kumulativna nagrada Chasera. Oblikovana ruka zadržava "
            "visoku individualnu nagradu dok joj je grupna nagrada na −1, što je potpis "
            "*shaping-farminga*. Pojas prikazuje raspon min–max po sjemenima."
        ),
        runs=("POCA_sparse_s1", "POCA_sparse_s2", "POCA_sparse_s3",
              "POCA_shaped_s1", "POCA_shaped_s2", "POCA_shaped_s3"),
        tags=(ELO, GROUP_R, INDIV_R),
        producer="plot_5m",
    ),
    Figure(
        slug="ppo_stopa_hvatanja",
        section="novo §6.5 (PPO 2×2)",
        illustrates="rezultati koji nedostaju za dizajn eksperimenta iz §5.4",
        caption=(
            "Stopa hvatanja (`Environment/Catch`) za sve četiri ćelije matrice algoritam × nagrada "
            "nakon 5 milijuna koraka. Tri ćelije dosežu 0,90–0,99; jedino MA-POCA s oblikovanom "
            "nagradom ostaje na ≈0,01."
        ),
        runs=("POCA_sparse_s1", "POCA_shaped_s1", "PPO_sparse_s1", "PPO_shaped_s1"),
        tags=(CATCH,),
        producer="plot_ppo",
    ),
    Figure(
        slug="ppo_elo",
        section="novo §6.5 (PPO 2×2)",
        illustrates="ista matrica, mjerena relativnom snagom umjesto stopom hvatanja",
        caption=(
            "ELO Chasera iz samoigre za sve četiri ćelije matrice algoritam × nagrada. Tri ćelije "
            "snažno divergiraju prema gore, dok MA-POCA s oblikovanom nagradom ostaje blizu "
            "početnih 1200."
        ),
        runs=("POCA_sparse_s1", "POCA_shaped_s1", "PPO_sparse_s1", "PPO_shaped_s1"),
        tags=(ELO,),
        producer="plot_ppo",
    ),
    Figure(
        slug="sonda_isporuke",
        section="novo §6.5 (sonda kanala isporuke)",
        illustrates="zaključak o kanalu isporuke terminalne nagrade iz §6.4.2",
        caption=(
            "Sonda kanala isporuke terminalne nagrade; sve tri konfiguracije koriste oblikovanje "
            "s coef 0,5 i razlikuju se samo po tome kamo se isporučuje terminalnih ±1. Samo grupni "
            "kanal ≈0,01, grupni + individualni ≈0,18 i dalje u porastu, samo individualni "
            "(PPO-stil) ≈0,99. Zamka oblikovanja ima dva uzroka, ne jedan."
        ),
        runs=("POCA_shaped_s1", "POCA_shaped_indivterm_s1", "PPO_shaped_s1"),
        tags=(CATCH,),
        producer="plot_ppo",
    ),
    Figure(
        slug="gama_sonde_zetva",
        section="novo §6.6 (γ-sonde)",
        illustrates="razrješava visjeće reference „§12 i §14\" i „[pogl. X.Y]\" (A10/A11)",
        caption=(
            "γ-sonde oblikovane ruke: stopa hvatanja ostaje ispod 1 % za sve tri vrijednosti γ "
            "(lijevo), dok individualna „žetva\" oblikovanja raste kako se horizont skraćuje "
            "(desno). Ponašanje prati stalni član koji je razmjeran s (1−γ), pa kraći horizont "
            "čini farmiranje isplativijim."
        ),
        runs=("POCA_shaped_g080_s1", "POCA_shaped_g090_s1", "POCA_shaped_s1"),
        tags=(CATCH, INDIV_R),
        producer="plot_gamma",
    ),
    Figure(
        slug="gama_krivulje_ucenja",
        section="novo §6.6 (γ-sweep, Faza A)",
        illustrates="γ-rasprava u §6.4.3 / §5.6, koja trenutno nema nijedan podatak",
        caption=(
            "Krivulje učenja stope hvatanja po γ (rijetka nagrada, arena s četiri fiksna stupa, "
            "5 milijuna koraka). Pojas prikazuje raspon min–max ondje gdje su dostupna tri "
            "sjemena. Bimodalnost pri γ = 0,995 vidljiva je isključivo grafički."
        ),
        runs=("POCA_sparse_obsF_g080_s1", "POCA_sparse_obsF_g080_s2", "POCA_sparse_obsF_g080_s3",
              "POCA_sparse_obsF_g090_s1", "POCA_sparse_obsF_g095_s1", "POCA_sparse_obsF_g099_s1",
              "POCA_sparse_obsF_g0995_s1", "POCA_sparse_obsF_g0995_s2",
              "POCA_sparse_obsF_g0995_s3"),
        tags=(CATCH,),
        producer="plot_gamma",
    ),
    Figure(
        slug="gama_osjetljivost",
        section="novo §6.6 (γ-sweep, Faza A)",
        illustrates="odgovor na pitanje „zašto γ = 0,99\"",
        caption=(
            "Osjetljivost na γ nakon 5 milijuna koraka: konačna stopa hvatanja (lijevo) i ELO "
            "razmak Chaser − Runner (desno). Točke su pojedinačna sjemena, siva linija spaja "
            "srednje vrijednosti."
        ),
        runs=("POCA_sparse_obsF_g080_s1", "POCA_sparse_obsF_g080_s2", "POCA_sparse_obsF_g080_s3",
              "POCA_sparse_obsF_g090_s1", "POCA_sparse_obsF_g095_s1", "POCA_sparse_obsF_g099_s1",
              "POCA_sparse_obsF_g0995_s1", "POCA_sparse_obsF_g0995_s2",
              "POCA_sparse_obsF_g0995_s3"),
        tags=(CATCH, ELO),
        producer="plot_gamma",
    ),
    Figure(
        slug="gama_elo_9_pokreta",
        section="novo §6.6 (γ-sweep, Faza A)",
        illustrates="bimodalnost γ = 0,995 na razini pojedinačnih pokretanja",
        caption=(
            "ELO iz samoigre za svih devet pokretanja Faze A. Chaseri se razilaze prema "
            "1650–1950, a Runneri padaju; pokretanje `g0995_s1` ostaje ravno do ≈4,3 milijuna "
            "koraka prije nego što krene učiti."
        ),
        runs=("POCA_sparse_obsF_g080_s1", "POCA_sparse_obsF_g080_s2", "POCA_sparse_obsF_g080_s3",
              "POCA_sparse_obsF_g090_s1", "POCA_sparse_obsF_g095_s1", "POCA_sparse_obsF_g099_s1",
              "POCA_sparse_obsF_g0995_s1", "POCA_sparse_obsF_g0995_s2",
              "POCA_sparse_obsF_g0995_s3"),
        tags=(ELO,),
        producer="plot_gamma",
    ),
]

APPENDIX = [
    ("tb_5M_sparse_vs_shaped", "POCA_(sparse|shaped)_s[123]",
     "Catch, Group Cumulative Reward, Cumulative Reward"),
    ("tb_ppo_2x2", "(POCA|PPO)_(sparse|shaped)_s1", "Catch, Self-play/ELO"),
    ("tb_gama_sweep", "POCA_sparse_obsF_g.*", "Catch, Self-play/ELO"),
    ("tb_baseline_vs_value_loss", "(POCA|PPO)_sparse_s1",
     "Losses/Baseline Loss, Losses/Value Loss"),
]

HEADER = """# Kazalo slika za poglavlje 6

> Ova je datoteka **generirana** skriptom `experiments/analysis/make_thesis_figures.py`.
> Ne uređuj je ručno — promijeni `experiments/analysis/figspec.py` i pokreni skriptu ponovno.

Brojeve slika dodjeljuje autor u Wordu. Ovdje je zabilježeno **gdje** svaka slika pripada.
Naslov slike ide **ispod** slike i centriran je, u skladu s Uputama FESB-a.

Datoteke: `poglavlje6/<naziv>.png` (300 dpi, za umetanje) i `<naziv>.svg` (za skaliranje).
"""


def render_index():
    lines = [HEADER, "", "## Slike poglavlja 6", ""]
    for fig in FIGURES:
        lines.append(f"### `{fig.slug}`")
        lines.append("")
        lines.append(f"- **Mjesto:** {fig.section}")
        lines.append(f"- **Ilustrira:** {fig.illustrates}")
        lines.append(f"- **Naslov slike:** {fig.caption}")
        lines.append(f"- **Pokretanja:** {', '.join(f'`{r}`' for r in fig.runs)}")
        lines.append(f"- **Mjerne veličine:** {', '.join(f'`{t}`' for t in fig.tags)}")
        lines.append(f"- **Izrađuje:** `{fig.producer}.py`")
        lines.append("")
    lines += ["## Prilog — snimke zaslona TensorBoarda", "",
              "| Datoteka | Filtar pokretanja | Paneli |", "|---|---|---|"]
    for slug, run_filter, panels in APPENDIX:
        lines.append(f"| `prilog_tensorboard/{slug}.png` | `{run_filter}` | {panels} |")
    lines.append("")
    return "\n".join(lines)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: 17 tests, OK

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/figspec.py experiments/analysis/tests/test_figspec.py
git commit -m "feat(analysis): figure manifest with Croatian captions and placement index"
```

---

### Task 5: Main 5M figure

**Files:**
- Create: `experiments/analysis/plot_5m.py`

This is the most important figure in the thesis. Three stacked panels sharing an x-axis; sparse in
blue, shaped in orange; min–max band over three seeds. The bottom panel is the point of the figure:
the shaped chaser's *individual* reward stays high while its *group* reward sits at −1.

- [ ] **Step 1: Write the producer**

Create `experiments/analysis/plot_5m.py`:

```python
"""Main results figure: sparse vs shaped over 5M steps, 3 seeds (thesis §6.3)."""
import os
import sys

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import tb_style as S

SPARSE_RUNS = ["POCA_sparse_s1", "POCA_sparse_s2", "POCA_sparse_s3"]
SHAPED_RUNS = ["POCA_shaped_s1", "POCA_shaped_s2", "POCA_shaped_s3"]

PANELS = [
    ("Self-play/ELO", "ELO (samoigra)", "Chaser"),
    ("Environment/Group Cumulative Reward", "grupna kumulativna nagrada", "Chaser"),
    ("Environment/Cumulative Reward", "individualna kumulativna nagrada", "Chaser"),
]


def build(out_dir):
    fig, axes = plt.subplots(3, 1, figsize=(S.COLUMN_IN, 8.4), dpi=S.DPI, sharex=True)

    for ax, (tag, ylabel, behavior) in zip(axes, PANELS):
        for runs, color, label in ((SPARSE_RUNS, S.SPARSE, "rijetka (sparse)"),
                                   (SHAPED_RUNS, S.SHAPED, "oblikovana (shaped)")):
            steps, mean, lo, hi = S.seed_band(runs, behavior, tag)
            x = steps / 1e6
            ax.fill_between(x, lo, hi, color=color, alpha=0.18, linewidth=0, zorder=2)
            ax.plot(x, mean, color=color, linewidth=1.8, zorder=3, label=label)
        S.style_ax(ax)
        ax.set_ylabel(ylabel, fontsize=9.5, color=S.INK)

    axes[0].legend(frameon=False, fontsize=9, loc="upper left")
    axes[1].axhline(0, color=S.MUTED, linewidth=0.8, linestyle=":", zorder=1)
    axes[2].set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    axes[0].set_title("Rijetka nasuprot oblikovanoj nagradi, 5M koraka, 3 sjemena (Chaser)",
                      fontsize=11, color=S.INK, loc="left")
    fig.text(0.01, 0.005,
             "kotrljajuća sredina, prozor 5; pojas = raspon min–max po sjemenima",
             fontsize=8, color=S.MUTED)
    fig.tight_layout(rect=[0, 0.02, 1, 1])
    written = S.save_figure(fig, out_dir, "sparse_vs_shaped_5M")
    plt.close(fig)
    return written


if __name__ == "__main__":
    target = sys.argv[1] if len(sys.argv) > 1 else "docs/slike/poglavlje6"
    for path in build(target):
        print("wrote", path)
```

- [ ] **Step 2: Run it**

Run from the repo root:
`python experiments/analysis/plot_5m.py docs/slike/poglavlje6`
Expected: two `wrote …` lines, for `sparse_vs_shaped_5M.png` and `.svg`

- [ ] **Step 3: Verify the figure says what it should**

Run:
```bash
python -c "
import sys; sys.path.insert(0,'experiments/analysis')
import tb_style as S
for run in ('POCA_sparse_s1','POCA_shaped_s1'):
    for tag in ('Environment/Group Cumulative Reward','Environment/Cumulative Reward'):
        _, v = S.load_series(run,'Chaser',tag)
        print(f'{run:18s} {tag:38s} {v[-5:].mean():8.3f}')
"
```
Expected: the shaped run shows a clearly negative group reward alongside a positive individual
reward; the sparse run shows a positive group reward. If the bottom two panels do not visibly
diverge for the shaped arm, the figure has failed its purpose — stop and investigate before
continuing.

- [ ] **Step 4: Open the PNG and confirm it is legible**

Read the file `docs/slike/poglavlje6/sparse_vs_shaped_5M.png` with the Read tool. Check: three
panels present, no clipped labels, legend readable, bands visible.

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/plot_5m.py docs/slike/poglavlje6/sparse_vs_shaped_5M.png docs/slike/poglavlje6/sparse_vs_shaped_5M.svg
git commit -m "feat(analysis): main 5M sparse-vs-shaped figure (3 panels, 3 seeds)"
```

---

### Task 6: PPO 2×2 and delivery probe

**Files:**
- Create: `experiments/analysis/plot_ppo.py`

Reward arm sets the colour, algorithm sets the line style. Remember: never ask a `PPO_*` run for
`Environment/Group Cumulative Reward`.

- [ ] **Step 1: Write the producer**

Create `experiments/analysis/plot_ppo.py`:

```python
"""PPO vs MA-POCA 2x2 and the delivery-channel probe (thesis §6.5)."""
import os
import sys

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import tb_style as S

# (run, label, colour, linestyle)
CELLS = [
    ("POCA_sparse_s1", "MA-POCA + rijetka", S.SPARSE, S.POCA_LS),
    ("POCA_shaped_s1", "MA-POCA + oblikovana", S.SHAPED, S.POCA_LS),
    ("PPO_sparse_s1", "PPO + rijetka", S.SPARSE, S.PPO_LS),
    ("PPO_shaped_s1", "PPO + oblikovana", S.SHAPED, S.PPO_LS),
]

PROBE = [
    ("POCA_shaped_s1", "samo grupni kanal", S.ORANGE),
    ("POCA_shaped_indivterm_s1", "grupni + individualni", S.GREEN),
    ("PPO_shaped_s1", "samo individualni (PPO)", S.BLUE),
]


def _curves(fig_ax, cells, tag, behavior="Chaser"):
    for run, label, color, linestyle in cells:
        steps, values = S.load_series(run, behavior, tag)
        fig_ax.plot(steps / 1e6, S.rolling(values), color=color, linestyle=linestyle,
                    linewidth=1.8, label=label, zorder=3)


def build_catch(out_dir):
    fig, ax = plt.subplots(figsize=(S.COLUMN_IN, 3.9), dpi=S.DPI)
    _curves(ax, CELLS, "Environment/Catch")
    S.style_ax(ax)
    ax.set_ylim(-0.03, 1.06)
    ax.set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    ax.set_ylabel("stopa hvatanja", fontsize=10, color=S.INK)
    ax.set_title("Stopa hvatanja: algoritam × nagrada (5M koraka)",
                 fontsize=11, color=S.INK, loc="left")
    ax.legend(frameon=False, fontsize=8.5, loc="center right")
    fig.tight_layout()
    written = S.save_figure(fig, out_dir, "ppo_stopa_hvatanja")
    plt.close(fig)
    return written


def build_elo(out_dir):
    fig, ax = plt.subplots(figsize=(S.COLUMN_IN, 3.9), dpi=S.DPI)
    _curves(ax, CELLS, "Self-play/ELO")
    ax.axhline(1200, color=S.MUTED, linewidth=0.8, linestyle=":", zorder=1)
    S.style_ax(ax)
    ax.set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    ax.set_ylabel("ELO (Chaser)", fontsize=10, color=S.INK)
    ax.set_title("ELO Chasera: algoritam × nagrada (5M koraka)",
                 fontsize=11, color=S.INK, loc="left")
    ax.legend(frameon=False, fontsize=8.5, loc="upper left")
    fig.tight_layout()
    written = S.save_figure(fig, out_dir, "ppo_elo")
    plt.close(fig)
    return written


def build_probe(out_dir):
    fig, ax = plt.subplots(figsize=(S.COLUMN_IN, 3.9), dpi=S.DPI)
    _curves(ax, [(r, l, c, "-") for r, l, c in PROBE], "Environment/Catch")
    S.style_ax(ax)
    ax.set_ylim(-0.03, 1.06)
    ax.set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    ax.set_ylabel("stopa hvatanja", fontsize=10, color=S.INK)
    ax.set_title("Kanal isporuke terminalne nagrade (sve ruke oblikovane, coef 0,5)",
                 fontsize=11, color=S.INK, loc="left")
    ax.legend(frameon=False, fontsize=8.5, loc="center right")
    fig.tight_layout()
    written = S.save_figure(fig, out_dir, "sonda_isporuke")
    plt.close(fig)
    return written


def build(out_dir):
    return build_catch(out_dir) + build_elo(out_dir) + build_probe(out_dir)


if __name__ == "__main__":
    target = sys.argv[1] if len(sys.argv) > 1 else "docs/slike/poglavlje6"
    for path in build(target):
        print("wrote", path)
```

- [ ] **Step 2: Run it**

Run: `python experiments/analysis/plot_ppo.py docs/slike/poglavlje6`
Expected: six `wrote …` lines (three figures × PNG + SVG)

- [ ] **Step 3: Check the figures against the known numbers**

Read `docs/slike/poglavlje6/ppo_stopa_hvatanja.png`. The four end-points must land near
POCA+sparse ≈ 1.00, PPO+sparse ≈ 0.91, PPO+shaped ≈ 0.99, POCA+shaped ≈ 0.01. If POCA+shaped is
anywhere but the floor, the run mapping is wrong.

- [ ] **Step 4: Commit**

```bash
git add experiments/analysis/plot_ppo.py docs/slike/poglavlje6/ppo_stopa_hvatanja.* docs/slike/poglavlje6/ppo_elo.* docs/slike/poglavlje6/sonda_isporuke.*
git commit -m "feat(analysis): PPO 2x2 catch/ELO figures and delivery-channel probe"
```

---

### Task 7: Gamma figures on the shared style

**Files:**
- Modify: `experiments/analysis/plot_gamma.py`

Two of this file's figures already exist and must keep their appearance; two are new. The refactor
replaces its private `series`/`rolling`/`style_ax` with `tb_style`, fixing the shortest-seed bug,
and adds `build(out_dir)` so the driver can call it.

- [ ] **Step 1: Replace the header and helpers**

In `experiments/analysis/plot_gamma.py`, replace lines 1–52 (everything from the docstring down to
and including the `style_ax` definition) with:

```python
"""Gamma-sweep figures, Phase A fixed pillars (thesis §6.6)."""
import os
import sys

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import tb_style as S

GAMMAS = {"g080": (0.8, ["s1", "s2", "s3"]), "g090": (0.9, ["s1"]), "g095": (0.95, ["s1"]),
          "g099": (0.99, ["s1"]), "g0995": (0.995, ["s1", "s2", "s3"])}
COLOR = {"g080": S.ORANGE, "g090": S.AMBER, "g095": S.GREEN,
         "g099": S.BLUE, "g0995": S.PURPLE}
LABEL = {k: f"γ={v[0]}".replace(".", ",") for k, v in GAMMAS.items()}

PROBES = [
    ("POCA_shaped_g080_s1", 0.8, S.ORANGE),
    ("POCA_shaped_g090_s1", 0.9, S.AMBER),
    ("POCA_shaped_s1", 0.99, S.BLUE),
]
```

Note the γ labels use a comma decimal separator, which is the Croatian convention and matches the
rest of the thesis. The old module-level `RES`, `OUT`, `os.makedirs`, `series`, `rolling` and
`style_ax` definitions all disappear — `tb_style` owns them now.

- [ ] **Step 2: Replace the two existing figure blocks with functions**

The rest of the file currently runs at import time, which would fire on every `import plot_gamma`.
Delete everything from `# ---------- Fig A` to the end of the file and replace it with:

```python
def build_catch_curves(out_dir):
    fig, ax = plt.subplots(figsize=(S.COLUMN_IN, 3.9), dpi=S.DPI)
    end_positions = []
    for tag_g, (gamma, seeds) in GAMMAS.items():
        runs = [f"POCA_sparse_obsF_{tag_g}_{s}" for s in seeds]
        steps, mean, lo, hi = S.seed_band(runs, "Chaser", "Environment/Catch")
        x = steps / 1e6
        if len(seeds) > 1:
            ax.fill_between(x, lo, hi, color=COLOR[tag_g], alpha=0.18,
                            linewidth=0, zorder=2)
        ax.plot(x, mean, color=COLOR[tag_g], linewidth=1.8, zorder=3)
        end_positions.append((float(mean[-1]), tag_g, len(seeds)))

    # direct end labels, nudged apart so they do not overlap
    end_positions.sort()
    last_y = -1
    for y, tag_g, nseeds in end_positions:
        ly = max(y, last_y + 0.055)
        last_y = ly
        suffix = " (3 sjemena)" if nseeds > 1 else ""
        ax.annotate(f"{LABEL[tag_g]}{suffix}  {y:.2f}".replace(".", ","),
                    xy=(5.02, y), xytext=(5.08, ly), fontsize=8, color=COLOR[tag_g],
                    va="center",
                    arrowprops=dict(arrowstyle="-", color=COLOR[tag_g], lw=0.6, alpha=0.5))
    S.style_ax(ax)
    ax.set_xlim(0, 6.4)
    ax.set_ylim(-0.02, 1.06)
    ax.set_xticks([0, 1, 2, 3, 4, 5])
    ax.set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    ax.set_ylabel("stopa hvatanja", fontsize=10, color=S.INK)
    ax.set_title("Stopa hvatanja po γ — rijetka nagrada, 4 fiksna stupa (Faza A, 5M)",
                 fontsize=10.5, color=S.INK, loc="left")
    fig.text(0.01, 0.01,
             "kotrljajuća sredina, prozor 5 (~250k koraka); pojas = min–max po sjemenima",
             fontsize=8, color=S.MUTED)
    fig.tight_layout(rect=[0, 0.03, 1, 1])
    written = S.save_figure(fig, out_dir, "gama_krivulje_ucenja")
    plt.close(fig)
    return written


def _finals(tag_g, behavior, scalar):
    _, seeds = GAMMAS[tag_g]
    values = []
    for seed in seeds:
        _, va = S.load_series(f"POCA_sparse_obsF_{tag_g}_{seed}", behavior, scalar)
        values.append(float(va[-5:].mean()))
    return values


def build_sensitivity(out_dir):
    fig, axes = plt.subplots(1, 2, figsize=(S.COLUMN_IN, 3.2), dpi=S.DPI)
    x_of = {k: 1 - v[0] for k, v in GAMMAS.items()}  # plot against (1-gamma), log scale

    panels = [
        ("konačna stopa hvatanja",
         lambda t: _finals(t, "Chaser", "Environment/Catch")),
        ("konačni ELO razmak (Chaser − Runner)",
         lambda t: [c - r for c, r in zip(_finals(t, "Chaser", "Self-play/ELO"),
                                          _finals(t, "Runner", "Self-play/ELO"))]),
    ]
    for ax, (title, getter) in zip(axes, panels):
        means = []
        for tag_g in GAMMAS:
            values = getter(tag_g)
            x = x_of[tag_g]
            ax.scatter([x] * len(values), values, s=34, color=COLOR[tag_g], zorder=4,
                       edgecolors="white", linewidths=1.1)
            means.append((x, float(np.mean(values))))
        means.sort(reverse=True)
        ax.plot([m[0] for m in means], [m[1] for m in means], color=S.MUTED,
                linewidth=1.4, zorder=3, alpha=0.8)
        ax.set_xscale("log")
        ax.invert_xaxis()  # left = short horizon (0.8), right = long (0.995)
        ax.set_xticks(list(x_of.values()))
        ax.set_xticklabels([f"{v[0]}".replace(".", ",") for v in GAMMAS.values()],
                           fontsize=8)
        ax.minorticks_off()
        S.style_ax(ax)
        ax.set_xlabel("γ (log-razmak po 1−γ)", fontsize=9.5, color=S.INK)
        ax.set_title(title, fontsize=9.5, color=S.INK, loc="left")
    axes[0].set_ylim(0.6, 1.05)
    fig.text(0.01, 0.01,
             "3 sjemena pri γ∈{0,8; 0,995}, 1 sjeme drugdje; točke su pojedinačna sjemena",
             fontsize=8, color=S.MUTED)
    fig.tight_layout(rect=[0, 0.05, 1, 1])
    written = S.save_figure(fig, out_dir, "gama_osjetljivost")
    plt.close(fig)
    return written
```

- [ ] **Step 3: Add the two new figures**

Append to `experiments/analysis/plot_gamma.py`:

```python
def build_probe_harvest(out_dir):
    """Catch stays on the floor for every gamma while the shaping harvest grows."""
    fig, axes = plt.subplots(1, 2, figsize=(S.COLUMN_IN, 3.4), dpi=S.DPI)

    for run, gamma, color in PROBES:
        steps, catch = S.load_series(run, "Chaser", "Environment/Catch")
        axes[0].plot(steps / 1e6, S.rolling(catch), color=color, linewidth=1.7,
                     label=f"γ={gamma}", zorder=3)
    S.style_ax(axes[0])
    axes[0].set_ylim(-0.005, 0.12)
    axes[0].set_xlabel("koraci (milijuni)", fontsize=9.5, color=S.INK)
    axes[0].set_ylabel("stopa hvatanja", fontsize=9.5, color=S.INK)
    axes[0].set_title("stopa hvatanja", fontsize=10, color=S.INK, loc="left")
    axes[0].legend(frameon=False, fontsize=8.5)

    harvest = []
    for run, gamma, color in PROBES:
        _, indiv = S.load_series(run, "Chaser", "Environment/Cumulative Reward")
        harvest.append((gamma, float(indiv[-5:].mean()), color))
    axes[1].bar([f"{g}" for g, _, _ in harvest], [v for _, v, _ in harvest],
                color=[c for _, _, c in harvest], zorder=3, width=0.6)
    for i, (_, value, _) in enumerate(harvest):
        axes[1].text(i, value, f"{value:+.1f}".replace(".", ","), ha="center",
                     va="bottom", fontsize=9, color=S.INK)
    S.style_ax(axes[1])
    axes[1].set_xlabel("γ", fontsize=9.5, color=S.INK)
    axes[1].set_ylabel("individualna nagrada", fontsize=9.5, color=S.INK)
    axes[1].set_title("žetva oblikovanja na 5M", fontsize=10, color=S.INK, loc="left")

    fig.tight_layout()
    written = S.save_figure(fig, out_dir, "gama_sonde_zetva")
    plt.close(fig)
    print("harvest ratio (γ=0.99 : 0.9 : 0.8) =",
          " : ".join(f"{v / harvest[-1][1]:.1f}" for _, v, _ in reversed(harvest)))
    return written


def build_elo_all_runs(out_dir):
    fig, ax = plt.subplots(figsize=(S.COLUMN_IN, 4.0), dpi=S.DPI)
    for tag_g, (gamma, seeds) in GAMMAS.items():
        for seed in seeds:
            run = f"POCA_sparse_obsF_{tag_g}_{seed}"
            for behavior, alpha in (("Chaser", 1.0), ("Runner", 0.45)):
                steps, elo = S.load_series(run, behavior, "Self-play/ELO")
                ax.plot(steps / 1e6, elo, color=COLOR[tag_g], alpha=alpha,
                        linewidth=1.2, zorder=3)
    ax.axhline(1200, color=S.MUTED, linewidth=0.8, linestyle=":", zorder=1)
    handles = [plt.Line2D([], [], color=COLOR[k], linewidth=1.6, label=LABEL[k])
               for k in GAMMAS]
    ax.legend(handles=handles, frameon=False, fontsize=8.5, ncol=5, loc="lower center")
    S.style_ax(ax)
    ax.set_xlabel("koraci treniranja (milijuni)", fontsize=10, color=S.INK)
    ax.set_ylabel("ELO", fontsize=10, color=S.INK)
    ax.set_title("ELO svih 9 pokretanja Faze A (puna linija = Chaser, blijeda = Runner)",
                 fontsize=10.5, color=S.INK, loc="left")
    fig.tight_layout()
    written = S.save_figure(fig, out_dir, "gama_elo_9_pokreta")
    plt.close(fig)
    return written


def build(out_dir):
    return (build_catch_curves(out_dir) + build_sensitivity(out_dir)
            + build_probe_harvest(out_dir) + build_elo_all_runs(out_dir))


if __name__ == "__main__":
    target = sys.argv[1] if len(sys.argv) > 1 else "docs/slike/poglavlje6"
    for path in build(target):
        print("wrote", path)
```

- [ ] **Step 4: Run it**

Run: `python experiments/analysis/plot_gamma.py docs/slike/poglavlje6`
Expected: eight `wrote …` lines plus one `harvest ratio …` line. Record the printed ratio — the
author may want it in the caption.

- [ ] **Step 5: Confirm the old figures still look right**

Compare `docs/slike/poglavlje6/gama_krivulje_ucenja.png` against the committed
`docs/figures/gamma/sweepA_catch_curves.png`. Same curves, same end labels; only size and DPI
should differ. If curves moved, the refactor changed behaviour — investigate before committing.

- [ ] **Step 6: Commit**

```bash
git add experiments/analysis/plot_gamma.py docs/slike/poglavlje6/gama_*.png docs/slike/poglavlje6/gama_*.svg
git commit -m "feat(analysis): gamma figures on shared style, add harvest probe and 9-run ELO"
```

---

### Task 8: Driver with --check

**Files:**
- Create: `experiments/analysis/make_thesis_figures.py`

- [ ] **Step 1: Write the driver**

Create `experiments/analysis/make_thesis_figures.py`:

```python
"""Build every thesis figure and regenerate docs/slike/INDEX.md.

    python experiments/analysis/make_thesis_figures.py --check   # resolve only
    python experiments/analysis/make_thesis_figures.py           # render everything
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import figspec
import tb_style as S

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_DIR = os.path.join(REPO, "docs", "slike", "poglavlje6")
INDEX = os.path.join(REPO, "docs", "slike", "INDEX.md")


def check():
    """Resolve every (run, behavior, tag) the manifest needs. -> exit code."""
    failures = []
    for fig in figspec.FIGURES:
        for run in fig.runs:
            for tag in fig.tags:
                behaviors = ("Chaser", "Runner") if tag == figspec.ELO else ("Chaser",)
                for behavior in behaviors:
                    try:
                        steps, values = S.load_series(run, behavior, tag)
                        print(f"  ok   {fig.slug:24s} {run:28s} {behavior:7s} "
                              f"{tag:38s} n={len(values)}")
                    except S.MissingSeries as exc:
                        failures.append(f"{fig.slug}: {exc}")
                        print(f"  FAIL {fig.slug:24s} {exc}")
    if failures:
        print(f"\n{len(failures)} unresolved series:")
        for line in failures:
            print("  -", line)
        return 1
    print(f"\nall series resolved for {len(figspec.FIGURES)} figures")
    return 0


def build_all():
    import plot_5m
    import plot_gamma
    import plot_ppo

    written = []
    for module in (plot_5m, plot_ppo, plot_gamma):
        written.extend(module.build(OUT_DIR))
    os.makedirs(os.path.dirname(INDEX), exist_ok=True)
    with open(INDEX, "w", encoding="utf-8") as handle:
        handle.write(figspec.render_index())
    written.append(INDEX)
    return written


if __name__ == "__main__":
    if "--check" in sys.argv:
        sys.exit(check())
    for path in build_all():
        print("wrote", os.path.relpath(path, REPO))
```

- [ ] **Step 2: Run the check**

Run: `python experiments/analysis/make_thesis_figures.py --check`
Expected: every line starts `ok`, final line `all series resolved for 8 figures`, exit code 0.

Verify the exit code: `python experiments/analysis/make_thesis_figures.py --check; echo "exit=$?"`
Expected: `exit=0`

- [ ] **Step 3: Prove the check actually fails on a typo**

Temporarily change one run id in `figspec.py` (e.g. `POCA_sparse_s1` → `POCA_sparse_sX`), then run
the check again.
Expected: a `FAIL` line naming the bad run, and `exit=1`. **Revert the edit afterwards** and re-run
to confirm `exit=0` again.

- [ ] **Step 4: Build everything**

Run: `python experiments/analysis/make_thesis_figures.py`
Expected: 17 `wrote …` lines — 8 figures × PNG + SVG, plus `docs/slike/INDEX.md`

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/make_thesis_figures.py docs/slike/INDEX.md
git commit -m "feat(analysis): thesis figure driver with --check gate and generated INDEX"
```

---

### Task 9: TensorBoard appendix plates

**Files:**
- Output: `docs/slike/prilog_tensorboard/*.png`

TensorBoard is already running at `http://localhost:6006` (verified HTTP 200). It is served from the
user's Anaconda Prompt; do not try to start or stop it.

- [ ] **Step 1: Open TensorBoard and fix the capture settings**

Using the Playwright MCP tools: resize the browser to 1600×1000, navigate to
`http://localhost:6006/#scalars`, and in the left sidebar set smoothing to `0.8`. Leave the theme
light — the plates are for print.

- [ ] **Step 2: Capture the four plates**

For each row below: type the regex into the sidebar "Filter runs" box, wait for the cards to
re-render, pin or filter to the listed panels using the tag-filter box, then screenshot into
`docs/slike/prilog_tensorboard/<slug>.png`.

| Slug | Run filter regex | Tag filter |
|---|---|---|
| `tb_5M_sparse_vs_shaped` | `POCA_(sparse\|shaped)_s[123]` | `Catch\|Cumulative Reward` |
| `tb_ppo_2x2` | `(POCA\|PPO)_(sparse\|shaped)_s1` | `Catch\|ELO` |
| `tb_gama_sweep` | `POCA_sparse_obsF_g.*` | `Catch\|ELO` |
| `tb_baseline_vs_value_loss` | `(POCA\|PPO)_sparse_s1` | `Baseline Loss\|Value Loss` |

The last plate is the interesting one: the POCA run shows both losses, the PPO run shows no
baseline loss at all. That absence is the evidence that the trainer really was MA-POCA.

- [ ] **Step 3: Verify each plate**

Read each PNG back with the Read tool. Confirm: run names legible in the legend, axes labelled,
no half-rendered cards, no browser dialogs in frame.

- [ ] **Step 4: Commit**

```bash
git add docs/slike/prilog_tensorboard/
git commit -m "docs: TensorBoard appendix plates at uniform capture settings"
```

---

### Task 10: Final verification

**Files:** none modified

- [ ] **Step 1: Run the whole test suite**

Run: `python -m unittest discover -s experiments/analysis/tests -t . -v`
Expected: 17 tests, OK, no failures or errors.

- [ ] **Step 2: Rebuild from scratch and confirm reproducibility**

```bash
rm -rf docs/slike/poglavlje6
python experiments/analysis/make_thesis_figures.py
ls docs/slike/poglavlje6
```
Expected: all 16 files regenerate (8 PNG + 8 SVG). This proves criterion 5 of the spec — every
figure is reproducible from the event files alone.

- [ ] **Step 3: Confirm nothing outside our paths was touched**

Run: `git status --short`
Expected: only `ProjectSettings/ProjectSettings.asset` remains modified (pre-existing, belongs to
Unity, and is not ours to commit). If `docs/*.md` files appear modified, the other session is
mid-edit — leave them alone.

- [ ] **Step 4: Confirm the deliverable is complete**

Read `docs/slike/INDEX.md`. Every one of the 8 figures must have a section, an "Ilustrira" line, and
a caption. Confirm the appendix table lists 4 plates.

- [ ] **Step 5: Report**

Summarise for the user: which figures were produced, where they live, what each one is for, and the
harvest ratio printed in Task 7. Note that figure numbers are theirs to assign in Word.

---

## Spec coverage

| Spec requirement | Task |
|---|---|
| Strict loader, loud failure on missing run/tag | 1, 8 |
| Unequal series lengths handled | 2 |
| PNG 300 DPI + SVG, 16 cm column | 3 |
| Placement-first `INDEX.md`, Croatian captions | 4 |
| Main 5M figure, 3 panels, 3 seeds, min–max band | 5 |
| PPO 2×2 + delivery probe | 6 |
| γ probes, learning curves, sensitivity, 9-run ELO | 7 |
| `--check` gate | 8 |
| Four appendix plates at uniform settings | 9 |
| Reproducible from event files alone | 10 |
| `docs/figures/` and `Theory.md` untouched | all — no task modifies them |
| Explicit-path commits only | all commit steps |
