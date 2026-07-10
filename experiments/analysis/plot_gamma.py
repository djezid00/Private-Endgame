"""Phase A gamma-sweep figures (fixed pillars) -> docs/figures/gamma/."""
import os, glob, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np

sys.path.insert(0, os.path.dirname(__file__))
from parse_tb import extract_scalars  # dependency-free tfevents parser

RES = r"C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results"
OUT = r"c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1/docs/figures/gamma"
os.makedirs(OUT, exist_ok=True)

GAMMAS = {"g080": (0.8, ["s1", "s2", "s3"]), "g090": (0.9, ["s1"]), "g095": (0.95, ["s1"]),
          "g099": (0.99, ["s1"]), "g0995": (0.995, ["s1", "s2", "s3"])}
COLOR = {"g080": "#D55E00", "g090": "#E69F00", "g095": "#009E73",
         "g099": "#0072B2", "g0995": "#CC79A7"}
LABEL = {k: f"γ={v[0]}" for k, v in GAMMAS.items()}

INK, MUTED, GRID = "#1a1a1a", "#5f5f5f", "#e6e6e3"


def series(run, beh, tag):
    files = sorted(glob.glob(os.path.join(RES, run, beh, "events.out.tfevents.*")))
    pts = []
    for f in files:
        pts.extend(extract_scalars(f).get(tag, []))
    pts.sort()
    return np.array([p[0] for p in pts]), np.array([p[1] for p in pts])


def rolling(y, w=5):
    if len(y) < w:
        return y
    out = np.convolve(y, np.ones(w) / w, mode="valid")
    return np.concatenate([y[: w - 1], out])  # keep length


def style_ax(ax):
    ax.grid(True, color=GRID, linewidth=0.8, zorder=0)
    for s in ("top", "right"):
        ax.spines[s].set_visible(False)
    for s in ("left", "bottom"):
        ax.spines[s].set_color(MUTED)
    ax.tick_params(colors=MUTED, labelsize=9)


# ---------- Fig A: catch-rate training curves ----------
fig, ax = plt.subplots(figsize=(8.6, 4.6), dpi=150)
end_positions = []
for tag_g, (g, seeds) in GAMMAS.items():
    runs = [f"POCA_sparse_obsF_{tag_g}_{s}" for s in seeds]
    curves = []
    for r in runs:
        st, va = series(r, "Chaser", "Environment/Catch")
        curves.append(rolling(va))
    n = min(len(c) for c in curves)
    st = st[:n] / 1e6
    arr = np.vstack([c[:n] for c in curves])
    mean = arr.mean(axis=0)
    if len(curves) > 1:
        ax.fill_between(st, arr.min(axis=0), arr.max(axis=0), color=COLOR[tag_g],
                        alpha=0.18, linewidth=0, zorder=2)
    ax.plot(st, mean, color=COLOR[tag_g], linewidth=1.8, zorder=3)
    end_positions.append((mean[-1], tag_g, len(curves)))

# direct end labels, nudged apart
end_positions.sort()
last_y = -1
for y, tag_g, nseeds in end_positions:
    ly = max(y, last_y + 0.055)
    last_y = ly
    suffix = " (3 seeds)" if nseeds > 1 else ""
    ax.annotate(f"{LABEL[tag_g]}{suffix}  {y:.2f}", xy=(5.02, y), xytext=(5.08, ly),
                fontsize=9, color=COLOR[tag_g], va="center",
                arrowprops=dict(arrowstyle="-", color=COLOR[tag_g], lw=0.6, alpha=0.5))
style_ax(ax)
ax.set_xlim(0, 6.4)
ax.set_ylim(-0.02, 1.06)
ax.set_xticks([0, 1, 2, 3, 4, 5])
ax.set_xlabel("training steps (millions)", fontsize=10, color=INK)
ax.set_ylabel("catch rate (Environment/Catch)", fontsize=10, color=INK)
ax.set_title("Chaser catch rate vs γ — sparse reward, 4 fixed pillars (Phase A, 5M)",
             fontsize=11, color=INK, loc="left")
fig.text(0.01, 0.01, "rolling mean, window 5 (~250k steps); bands = min–max over 3 seeds",
         fontsize=8, color=MUTED)
fig.tight_layout(rect=[0, 0.03, 1, 1])
fig.savefig(os.path.join(OUT, "sweepA_catch_curves.png"))
plt.close(fig)

# ---------- Fig B: final-value sensitivity ----------
def finals(tag, beh, scalar):
    g, seeds = GAMMAS[tag]
    vals = []
    for s in seeds:
        _, va = series(f"POCA_sparse_obsF_{tag}_{s}", beh, scalar)
        vals.append(va[-5:].mean())
    return vals


fig, axes = plt.subplots(1, 2, figsize=(9.4, 4.0), dpi=150)
x_of = {k: 1 - v[0] for k, v in GAMMAS.items()}  # plot vs (1-gamma), log

for ax, (title, getter) in zip(axes, [
    ("final catch rate", lambda t: finals(t, "Chaser", "Environment/Catch")),
    ("final ELO gap (Chaser − Runner)",
     lambda t: [c - r for c, r in zip(finals(t, "Chaser", "Self-play/ELO"),
                                      finals(t, "Runner", "Self-play/ELO"))]),
]):
    means = []
    for tag_g in GAMMAS:
        vals = getter(tag_g)
        x = x_of[tag_g]
        ax.scatter([x] * len(vals), vals, s=42, color=COLOR[tag_g], zorder=4,
                   edgecolors="white", linewidths=1.2)
        means.append((x, float(np.mean(vals))))
    means.sort(reverse=True)
    ax.plot([m[0] for m in means], [m[1] for m in means], color=MUTED,
            linewidth=1.4, zorder=3, alpha=0.8)
    ax.set_xscale("log")
    ax.invert_xaxis()  # left = short horizon (gamma 0.8), right = long (0.995)
    ax.set_xticks(list(x_of.values()))
    ax.set_xticklabels([f"{v[0]}" for v in GAMMAS.values()])
    ax.minorticks_off()
    style_ax(ax)
    ax.set_xlabel("γ (log-spaced by 1−γ)", fontsize=10, color=INK)
    ax.set_title(title, fontsize=10.5, color=INK, loc="left")
axes[0].set_ylim(0.6, 1.05)
fig.suptitle("Gamma sensitivity at 5M — per-seed points, gray line = mean (Phase A, fixed pillars)",
             fontsize=11, color=INK, x=0.01, ha="left")
fig.text(0.01, 0.01, "3 seeds at γ∈{0.8, 0.995}, 1 seed elsewhere; endpoints show seed spread",
         fontsize=8, color=MUTED)
fig.tight_layout(rect=[0, 0.04, 1, 0.93])
fig.savefig(os.path.join(OUT, "sweepA_sensitivity.png"))
plt.close(fig)

print("wrote figures to", OUT)
