@echo off
setlocal
REM ============================================================================
REM  PHASE C (RQ-D) - MA-POCA vs PPO at 2v2. Sparse, gamma 0.99, random pillars.
REM  6 runs x 5M steps. ~4.9 h/run on GPU  =>  ~29 h total.
REM  (On CPU the same matrix is ~68 h - see the CUDA guard below.)
REM
REM  PREREQ 1: run from the Anaconda Prompt with the GPU env active:
REM              conda activate mlagents_gpu
REM  PREREQ 2: Phase C binary rebuilt + TagMApoca_team_smoke gate PASSED.
REM            Gate criterion 3 (Baseline/Value > 1.05) measured 1.30 / 1.28.
REM
REM  POCA arm runs first: if interrupted, the primary arm is complete.
REM ============================================================================

set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\poca"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  pause
  exit /b 1
)

REM --- CUDA GUARD -------------------------------------------------------------
REM Phase C is 2.3x faster on GPU (update step 5.7x). A conda-cloned env ships
REM console-script launchers that embed the ORIGINAL env's interpreter, so
REM mlagents-learn can silently run on CPU while `python` reports CUDA fine.
REM That mistake costs ~39 extra hours and only shows up in timers.json.
python -c "import torch,sys; sys.exit(0 if torch.cuda.is_available() else 1)"
if errorlevel 1 (
  echo [ERROR] torch.cuda.is_available^(^) is False - this would run on CPU.
  echo         Activate the GPU env:  conda activate mlagents_gpu
  echo         Verify: python -c "import torch;print^(torch.__version__,torch.cuda.is_available^(^)^)"
  echo         Expect: 2.11.0+cu126 True
  pause
  exit /b 1
)
echo [OK] CUDA available.

if not exist batch_logs mkdir batch_logs
echo Starting Phase C at %DATE% %TIME%

REM --- MA-POCA arm (primary) --------------------------------------------------
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s1 --seed 1 > batch_logs\POCA_team_2v2_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s2 --seed 2 > batch_logs\POCA_team_2v2_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s3 --seed 3 > batch_logs\POCA_team_2v2_s3.log 2>&1

REM --- PPO baseline arm -------------------------------------------------------
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s1  --seed 1 > batch_logs\PPO_team_2v2_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s2  --seed 2 > batch_logs\PPO_team_2v2_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s3  --seed 3 > batch_logs\PPO_team_2v2_s3.log 2>&1

echo Phase C complete at %DATE% %TIME%
endlocal
pause
