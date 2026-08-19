@echo off
setlocal
REM ============================================================================
REM  PHASE B (RQ-C) - sparse, gamma 0.99 ONLY, matched seeds. 5 runs (~22 h).
REM
REM  Supersedes run_obs_phaseB.bat (the 9-run gamma sweep at randomized layouts).
REM  Phase A already settled gamma=0.99 as the operating point, so this tests the
REM  LAYOUT effect at that point instead of re-sweeping a closed question.
REM
REM  Arms (3 vs 3 at gamma=0.99):
REM    obsR s1/s2/s3  - 4 pillars RANDOMIZED per episode  (new)
REM    obsF s2/s3     - 4 pillars FIXED                   (backfill; s1 ran in Phase A)
REM  The obsF backfill exists because Phase A ran gamma=0.99 with ONE seed; without
REM  it the fixed-vs-random contrast would be 3-vs-1 and seed noise could swallow
REM  the effect (cf. Phase A gamma=0.995, where 1 of 3 seeds collapsed).
REM
REM  Random arm runs FIRST: if the batch is interrupted, the new arm is complete.
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

echo Starting Phase B (gamma 0.99, matched seeds) at %DATE% %TIME%

REM --- RANDOM layout arm (the new science) ------------------------------------
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s1 --seed 1 > batch_logs\POCA_sparse_obsR_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s2 --seed 2 > batch_logs\POCA_sparse_obsR_g099_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s3 --seed 3 > batch_logs\POCA_sparse_obsR_g099_s3.log 2>&1

REM --- FIXED layout backfill (s1 already exists from Phase A) ------------------
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s2 --seed 2 > batch_logs\POCA_sparse_obsF_g099_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s3 --seed 3 > batch_logs\POCA_sparse_obsF_g099_s3.log 2>&1

echo Phase B complete at %DATE% %TIME%
endlocal
pause
