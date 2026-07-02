@echo off
setlocal
REM ============================================================================
REM  PPO COMPARISON — 2 runs (PPO sparse x1, PPO shaped x1), seed 1, 5M each.
REM  Completes the 2x2 (algorithm x reward) vs the 3-seed MA-POCA bands.
REM  PREREQUISITE: run the PPO SMOKE gate first (see docs plan Task 4) and confirm
REM  PPO trains + tolerates grouped agents BEFORE launching these 5M runs.
REM  Open the Anaconda Prompt (conda env "mlagents" active) and run this file.
REM ============================================================================

REM === full path to the built headless executable (same build as the POCA batch) ===
set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"

REM ML-Agents repo root (configs are read relative to here)
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\ppo"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  pause
  exit /b 1
)
if not exist batch_logs mkdir batch_logs

echo Starting PPO batch at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_ppo_sparse.yaml --env="%ENV%" --no-graphics --run-id=PPO_sparse_s1 --seed 1 > batch_logs\PPO_sparse_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_ppo_shaped.yaml --env="%ENV%" --no-graphics --run-id=PPO_shaped_s1 --seed 1 > batch_logs\PPO_shaped_s1.log 2>&1

echo PPO batch complete at %DATE% %TIME%
endlocal
pause
