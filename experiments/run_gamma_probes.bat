@echo off
setlocal
REM ============================================================================
REM  GAMMA PROBES (RQ-B) - 2 runs: shaped arm, NO obstacles, gamma 0.8 / 0.9.
REM  Tests the farming-trap mechanism (standing term scales with 1-gamma).
REM  Baseline for comparison: POCA_shaped_s{1,2,3} (gamma 0.99, already run).
REM
REM  PREREQUISITES
REM   1. Headless build REBUILT after the shaping_gamma commit (binary must
REM      contain the new env-param code) - check the [TagAgent] log line.
REM   2. Smoke gate TagMApoca_gprobe_smoke.yaml PASSED against this binary.
REM   3. Run from the Anaconda Prompt (conda env "mlagents").
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

echo Starting gamma probes at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_shaped_g080.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_g080_s1 --seed 1 > batch_logs\POCA_shaped_g080_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_shaped_g090.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_g090_s1 --seed 1 > batch_logs\POCA_shaped_g090_s1.log 2>&1

echo Probes complete at %DATE% %TIME%
endlocal
pause
