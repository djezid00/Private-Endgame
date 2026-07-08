@echo off
setlocal
REM ============================================================================
REM  GAMMA SWEEP PHASE A - RESUME batch (after the 2026-07-07 Ctrl+C pause).
REM  Already COMPLETE (do NOT re-run): g080 s1/s2/s3.
REM  This batch does the remaining 6 runs in order:
REM    1) g090_s1  -- RESUMES from ~1.9M (has --resume; log appended)
REM    2-6) g095_s1, g099_s1, g0995 s1/s2/s3 -- fresh (NO --resume)
REM  Run from the Anaconda Prompt (conda env "mlagents"). ~26-30h total.
REM  If interrupted again: the g090 line already has --resume; for any OTHER
REM  run that gets interrupted, append --resume to ITS line before re-running.
REM ============================================================================

set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\poca"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  pause
  exit /b 1
)
if not exist batch_logs mkdir batch_logs

echo Resuming Phase A at %DATE% %TIME%

REM --- interrupted run: resume to 5M (append to its existing log) ---
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g090.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g090_s1  --seed 1 --resume >> batch_logs\POCA_sparse_obsF_g090_s1.log 2>&1

REM --- not-yet-started runs: fresh (no --resume) ---
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g095.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g095_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g095_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s1 --seed 1 > batch_logs\POCA_sparse_obsF_g0995_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s2 --seed 2 > batch_logs\POCA_sparse_obsF_g0995_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s3 --seed 3 > batch_logs\POCA_sparse_obsF_g0995_s3.log 2>&1

echo Phase A (resume) complete at %DATE% %TIME%
endlocal
pause
