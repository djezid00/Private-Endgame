@echo off
setlocal
REM ============================================================================
REM  1v1 RIGOR PHASE - overnight MA-POCA batch: 6 runs (sparse x3, shaped x3).
REM  PPO is deferred (run separately after POCA finishes, per the plan).
REM
REM  PREREQUISITES
REM   1. Build the HEADLESS player in Unity:
REM        File > Build Settings > Windows (Standalone) > Build
REM      Note the produced .exe path and set ENV below.
REM   2. Open the Anaconda Prompt (conda env "mlagents" must be active).
REM   3. Run this file from that Anaconda Prompt:  experiments\run_overnight_poca.bat
REM
REM  Results -> <ml-agents>\results\<run-id> ; per-run console logs -> .\batch_logs\
REM  Each run goes up to 5M steps (early-stop is a manual/interactive choice; an
REM  unattended batch just runs each to completion).
REM  Re-running after a partial batch: either clear results\ for the affected
REM  run-ids, or append "--resume" to those lines (do NOT use --force unless you
REM  intend to overwrite a finished run).
REM
REM  Optional throughput: append "--num-envs 2" to launch 2 game copies per run
REM  (more CPU/RAM; measure first - see the arena bake-off notes).
REM ============================================================================

REM === EDIT THIS: full path to your built headless executable ===
set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"

REM ML-Agents repo root (configs are read relative to here)
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\poca"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  echo Build it first ^(File ^> Build Settings ^> Build^) and update ENV in this script.
  pause
  exit /b 1
)
if not exist batch_logs mkdir batch_logs

echo Starting overnight batch at %DATE% %TIME%

REM ---- MA-POCA, SPARSE arm (distance_shaping_coef = 0.0), 3 seeds ----
mlagents-learn %CFG%\TagMApoca_sparse_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_s1 --seed 1 > batch_logs\POCA_sparse_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_s2 --seed 2 > batch_logs\POCA_sparse_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_s3 --seed 3 > batch_logs\POCA_sparse_s3.log 2>&1

REM ---- MA-POCA, SHAPED arm (distance_shaping_coef = 0.5), 3 seeds ----
mlagents-learn %CFG%\TagMApoca_shaped_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_s1 --seed 1 > batch_logs\POCA_shaped_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_shaped_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_s2 --seed 2 > batch_logs\POCA_shaped_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_shaped_5M.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_s3 --seed 3 > batch_logs\POCA_shaped_s3.log 2>&1

echo Batch complete at %DATE% %TIME%
endlocal
pause
