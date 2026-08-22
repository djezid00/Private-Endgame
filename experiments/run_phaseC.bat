@echo off
setlocal
REM ============================================================================
REM  PHASE C (RQ-D) - MA-POCA vs PPO at 2v2. Sparse, gamma 0.99, random pillars.
REM  4 runs x 5M steps, 2 seeds per arm.  ~4.7 h/run on GPU  =>  ~19 h total.
REM  (Same matrix on CPU is ~45 h - see the CUDA guard below.)
REM
REM  ARMS INTERLEAVED BY SEED, NOT GROUPED BY ALGORITHM. After the first two
REM  runs you already hold a complete 1-seed POCA-vs-PPO comparison; after all
REM  four, the 2-seed version. Grouping by algorithm would mean an interruption
REM  at the halfway point left two POCA runs and no comparison at all.
REM
REM  PREREQ 1: conda activate mlagents_gpu   (GPU env - 2.3x faster overall)
REM  PREREQ 2: Phase C binary rebuilt + TagMApoca_team_smoke gate PASSED.
REM            Gate criterion 3 (Baseline/Value > 1.05) measured 1.30 / 1.28.
REM
REM  CONTINGENCY: if the two arms land close together, add a third seed each
REM  (~9.4 h) before concluding "no difference":
REM    mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s3 --seed 3
REM    mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s3  --seed 3
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
REM A conda-cloned env ships console-script launchers embedding the ORIGINAL
REM env's interpreter, so mlagents-learn can silently run on CPU while `python`
REM reports CUDA fine. That mistake turns 19 h into 45 h and only shows up in
REM timers.json afterwards.
python -c "import torch,sys; sys.exit(0 if torch.cuda.is_available() else 1)"
if errorlevel 1 (
  echo [ERROR] torch.cuda.is_available^(^) is False - this would run on CPU.
  echo         Activate the GPU env:  conda activate mlagents_gpu
  echo         Expect: 2.11.0+cu126 True
  pause
  exit /b 1
)
echo [OK] CUDA available.

if not exist batch_logs mkdir batch_logs
echo Starting Phase C at %DATE% %TIME%

REM --- seed 1 pair ------------------------------------------------------------
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s1 --seed 1 > batch_logs\POCA_team_2v2_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s1  --seed 1 > batch_logs\PPO_team_2v2_s1.log 2>&1

REM --- seed 2 pair ------------------------------------------------------------
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s2 --seed 2 > batch_logs\POCA_team_2v2_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s2  --seed 2 > batch_logs\PPO_team_2v2_s2.log 2>&1

echo Phase C complete at %DATE% %TIME%
endlocal
pause
