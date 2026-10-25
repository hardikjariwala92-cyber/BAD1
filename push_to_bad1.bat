@echo off
setlocal EnableDelayedExpansion
title BAD1 - GitHub Automated Push and Sync Tool
color 0B

cd /d "%~dp0"

echo ======================================================================
echo           BAD1 - Sales, Billing and Business Analytics Suite
echo              GitHub Automated Push and Repository Sync
echo ======================================================================
echo.
echo Target Repository : https://github.com/hardikjariwala92-cyber/BAD1.git
echo Local Workspace   : %CD%
echo.

:: 1. Check if git is installed
where git >nul 2>nul
if errorlevel 1 (
    color 0C
    echo [ERROR] Git is not installed or not in PATH!
    echo Please install Git for Windows from https://git-scm.com/
    echo.
    pause
    exit /b 1
)

:: 2. Clear any stale Git index lock if present
if exist ".git\index.lock" (
    echo [*] Removing stale Git index lock...
    del /f /q ".git\index.lock" >nul 2>nul
)

:: 3. Ensure remote 'bad1' is configured correctly
git remote get-url bad1 >nul 2>nul
if errorlevel 1 (
    echo [*] Adding remote 'bad1' - https://github.com/hardikjariwala92-cyber/BAD1.git
    git remote add bad1 https://github.com/hardikjariwala92-cyber/BAD1.git
) else (
    git remote set-url bad1 https://github.com/hardikjariwala92-cyber/BAD1.git
    echo [OK] Remote 'bad1' verified: https://github.com/hardikjariwala92-cyber/BAD1.git
)

:: 4. Detect current active branch
for /f "tokens=*" %%i in ('git branch --show-current') do set CURRENT_BRANCH=%%i
if "!CURRENT_BRANCH!"=="" set CURRENT_BRANCH=feature/sales-billing
echo [*] Current Active Branch: !CURRENT_BRANCH!
echo.

:: 5. Auto-detect uncommitted or untracked changes and commit them cleanly
git status --porcelain | findstr /r "." >nul 2>nul
if errorlevel 1 goto REPO_CLEAN

echo [*] Uncommitted changes detected in workspace.
echo [*] Staging all files with 'git add -A' ...
git add -A
echo [*] Creating commit with latest changes...
git commit -m "feat(sync): sync latest features, UI upgrades and tests to BAD1 repository"
echo [OK] Local changes successfully committed!
echo.
goto SYNC_BRANCHES

:REPO_CLEAN
echo [OK] Workspace is clean (all files committed).
echo.

:SYNC_BRANCHES
:: 6. Fast-forward local 'main' branch to current HEAD (no checkout needed)
if not "!CURRENT_BRANCH!"=="main" (
    echo [*] Fast-forwarding local 'main' branch to latest commit...
    git branch -f main HEAD >nul 2>nul
    echo [OK] Local 'main' synchronized with '!CURRENT_BRANCH!'.
    echo.
)

echo ----------------------------------------------------------------------
echo Select a push option (or wait 3 seconds for Auto-Push):
echo ----------------------------------------------------------------------
echo  [1] Auto-Push (Recommended - Syncs main + !CURRENT_BRANCH! + all branches to BAD1)
echo  [2] Push only 'main' branch
echo  [3] Push only active branch [!CURRENT_BRANCH!]
echo  [4] Push ALL branches to BAD1
echo  [5] Check Git status and remotes
echo  [6] Exit
echo ----------------------------------------------------------------------
echo.

if "%~1"=="" (
    choice /c 123456 /t 3 /d 1 /m "Enter choice [Auto-proceeds with 1 in 3s]"
    set CHOICE_ERR=!ERRORLEVEL!
) else (
    set CHOICE_ERR=%~1
)

if "!CHOICE_ERR!"=="1" goto PUSH_RECOMMENDED
if "!CHOICE_ERR!"=="2" goto PUSH_MAIN
if "!CHOICE_ERR!"=="3" goto PUSH_CURRENT
if "!CHOICE_ERR!"=="4" goto PUSH_ALL
if "!CHOICE_ERR!"=="5" goto SHOW_STATUS
if "!CHOICE_ERR!"=="6" goto EXIT_SCRIPT

:PUSH_RECOMMENDED
echo.
echo ======================================================================
echo Pushing latest code to BAD1 GitHub...
echo Target: https://github.com/hardikjariwala92-cyber/BAD1
echo ======================================================================
echo.

echo [*] Pushing 'main' branch...
git push -u bad1 main:main --force
set PUSH_ERR=!ERRORLEVEL!

echo.
echo [*] Pushing '!CURRENT_BRANCH!' branch...
git push -u bad1 !CURRENT_BRANCH!:!CURRENT_BRANCH! --force
if !ERRORLEVEL! neq 0 set PUSH_ERR=!ERRORLEVEL!

echo.
echo [*] Syncing all other branches to BAD1...
git push bad1 --all --force >nul 2>nul

goto CHECK_RESULT

:PUSH_MAIN
echo.
echo ======================================================================
echo Pushing latest code to 'main' on GitHub (BAD1)...
echo ======================================================================
echo.
git push -u bad1 HEAD:main --force
set PUSH_ERR=%ERRORLEVEL%
goto CHECK_RESULT

:PUSH_ALL
echo.
echo ======================================================================
echo Pushing ALL branches to GitHub bad1 repo...
echo ======================================================================
echo.
git push --all bad1 --force
set PUSH_ERR=%ERRORLEVEL%
goto CHECK_RESULT

:PUSH_CURRENT
echo.
echo ======================================================================
echo Pushing branch '!CURRENT_BRANCH!' to bad1 repo...
echo ======================================================================
echo.
git push -u bad1 !CURRENT_BRANCH! --force
set PUSH_ERR=%ERRORLEVEL%
goto CHECK_RESULT

:SHOW_STATUS
echo.
echo ======================================================================
echo Git Remotes:
echo ======================================================================
git remote -v
echo.
echo ======================================================================
echo Git Status:
echo ======================================================================
git status
echo.
pause
goto :eof

:CHECK_RESULT
if %PUSH_ERR% equ 0 (
    color 0A
    echo.
    echo ======================================================================
    echo [SUCCESS] Push completed successfully!
    echo Your latest code, live PDF designer, reports and tests are now on GitHub!
    echo.
    echo Live Repository : https://github.com/hardikjariwala92-cyber/BAD1
    echo Active Branches : main, !CURRENT_BRANCH!
    echo ======================================================================
) else (
    color 0C
    echo.
    echo ======================================================================
    echo [NOTICE] Push exited with code %PUSH_ERR%.
    echo If GitHub Credential Manager opened a browser popup, please sign in.
    echo.
    echo Troubleshooting:
    echo 1. If a browser window opened, click 'Sign in with your browser'.
    echo 2. If prompted for password in terminal, enter a GitHub Personal
    echo    Access Token with 'repo' scope from:
    echo    https://github.com/settings/tokens
    echo 3. Verify that your GitHub account has write access to:
    echo    https://github.com/hardikjariwala92-cyber/BAD1
    echo ======================================================================
)
echo.
pause
goto :eof

:EXIT_SCRIPT
echo Exiting.
goto :eof
