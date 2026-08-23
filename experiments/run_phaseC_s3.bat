@echo off
setlocal
REM ============================================================================
REM  PHASE C - SEED 3 pair. Completes the 3-seed convention used by every other
REM  phase in this thesis. Sparse, gamma 0.99, random pillars, 2v2.
REM  2 runs x 5M steps, ~5.5 h each on GPU  =>  ~11 h total.
REM
REM  PREREQ: conda activate mlagents_gpu    (GPU env - 2.3x faster overall)
REM  Both guards below are the same ones in run_phaseC.bat and must pass.
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
python -c "import torch,sys; sys.exit(0 if torch.cuda.is_available() else 1)"
if errorlevel 1 (
  echo [ERROR] torch.cuda.is_available^(^) is False - this would run on CPU ^(~11 h/run^).
  echo         Activate the GPU env:  conda activate mlagents_gpu
  pause
  exit /b 1
)
echo [OK] CUDA available.

REM --- ONNX EXPORT GUARD ------------------------------------------------------
REM Without the dynamo=False patch every run dies at the first checkpoint (250k).
findstr /C:"dynamo=False" "C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\ml-agents\mlagents\trainers\torch_entities\model_serialization.py" >nul 2>&1
if errorlevel 1 (
  echo [ERROR] ONNX export is not patched - every run would crash at the first checkpoint.
  pause
  exit /b 1
)
echo [OK] ONNX export patched (dynamo=False).

if not exist batch_logs mkdir batch_logs
echo Starting Phase C seed 3 at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s3 --seed 3 > batch_logs\POCA_team_2v2_s3.log 2>&1
if errorlevel 1 ( echo [ERROR] POCA_team_2v2_s3 FAILED - aborting. See batch_logs\POCA_team_2v2_s3.log & pause & exit /b 1 )

mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s3  --seed 3 > batch_logs\PPO_team_2v2_s3.log 2>&1
if errorlevel 1 ( echo [ERROR] PPO_team_2v2_s3 FAILED - aborting. See batch_logs\PPO_team_2v2_s3.log & pause & exit /b 1 )

echo Phase C seed 3 complete at %DATE% %TIME%
endlocal
pause
