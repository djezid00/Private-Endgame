@echo off
setlocal
REM ============================================================================
REM  PHASE B (RQ-C) - sparse, gamma 0.99, 4 RANDOMIZED pillars. 3 runs (~13 h).
REM
REM  Supersedes run_obs_phaseB.bat (the 9-run gamma sweep at randomized layouts).
REM  Phase A already settled gamma=0.99 as the operating point, so this tests the
REM  LAYOUT effect at that point instead of re-sweeping a closed question.
REM
REM  Compare against the Phase A FIXED-layout plateau, already established by four
REM  runs at gamma >= 0.95 (catch 0.99-1.00, ELO gap 1211-1257):
REM    obsF_g095_s1 1.00/1211   obsF_g099_s1 0.99/1249
REM    obsF_g0995_s2 1.00/1253  obsF_g0995_s3 1.00/1257
REM  g099_s1 sits mid-cluster, so it is not an outlier and needs no backfill.
REM
REM  CONTINGENCY (do NOT run upfront): if the random arm lands marginal, i.e.
REM  overlapping 0.99-1.00 rather than clearly below, only then add fixed seeds
REM  s2/s3 at gamma=0.99 to firm up that single-seed cell (~9 h):
REM    mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s2 --seed 2
REM    mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s3 --seed 3
REM
REM  PREREQ: binary rebuilt after ca64ed0 + TagMApoca_obs_smoke gate PASSED
REM          (ObsSmoke_02 == ObsSmoke_03 confirmed --seed reproducibility).
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

echo Starting Phase B (gamma 0.99, randomized pillars) at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s1 --seed 1 > batch_logs\POCA_sparse_obsR_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s2 --seed 2 > batch_logs\POCA_sparse_obsR_g099_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s3 --seed 3 > batch_logs\POCA_sparse_obsR_g099_s3.log 2>&1

echo Phase B complete at %DATE% %TIME%
endlocal
pause
