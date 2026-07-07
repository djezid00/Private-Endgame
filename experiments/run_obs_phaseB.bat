@echo off
setlocal
REM ============================================================================
REM  GAMMA SWEEP PHASE B (RQ-A/RQ-C) - sparse, 4 RANDOM-PER-EPISODE pillars, 9 runs:
REM  gamma 0.8 x3 seeds, 0.9 / 0.95 / 0.99 x1, 0.995 x3 seeds.
REM  GATE: launch only after the Phase A review (decision gate in the plan).
REM  PREREQ: same binary as Phase A (no code change between phases).
REM  Run from the Anaconda Prompt (conda env "mlagents").
REM
REM  Re-running after a partial batch: clear results\ for the affected run-ids,
REM  or append "--resume" to those lines (do NOT use --force unless you intend
REM  to overwrite a finished run).
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

echo Starting Phase B at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g080_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s2  --seed 2 > batch_logs\POCA_sparse_obsR_g080_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s3  --seed 3 > batch_logs\POCA_sparse_obsR_g080_s3.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g090.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g090_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g090_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g095.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g095_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g095_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s1 --seed 1 > batch_logs\POCA_sparse_obsR_g0995_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s2 --seed 2 > batch_logs\POCA_sparse_obsR_g0995_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s3 --seed 3 > batch_logs\POCA_sparse_obsR_g0995_s3.log 2>&1

echo Phase B complete at %DATE% %TIME%
endlocal
pause
