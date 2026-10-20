@echo off
setlocal EnableDelayedExpansion
title BAD1 - GitHub Repository Push and Sync Tool
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
if %ERRORLEVEL% neq 0 (
    color 0C
    echo [ERROR] Git is not installed or not in PATH!
    echo Please install Git for Windows from https://git-scm.com/
    echo.
    pause
    exit /b 1
)

:: 2. Ensure remote 'bad1' is configured
git remote | findstr /i "bad1" >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [*] Adding remote 'bad1' - https://github.com/hardikjariwala92-cyber/BAD1.git
    git remote add bad1 https://github.com/hardikjariwala92-cyber/BAD1.git
) else (
    echo [OK] Remote 'bad1' is configured.
)

:: 3. Show current active branch
for /f "tokens=*" %%i in ('git branch --show-current') do set CURRENT_BRANCH=%%i
if "!CURRENT_BRANCH!"=="" set CURRENT_BRANCH=main
echo [*] Current Active Branch: !CURRENT_BRANCH!
echo.

:: 4. Auto-detect uncommitted changes and commit them
set HAS_UNCOMMITTED=
for /f "tokens=*" %%s in ('git status --porcelain') do set HAS_UNCOMMITTED=1

if defined HAS_UNCOMMITTED (
    echo [*] Uncommitted changes detected in workspace.
    echo [*] Staging all files with 'git add .' ...
    git add .
    echo [*] Creating commit with latest changes...
    git commit -m "feat(sync): sync latest features, UI upgrades and tests to BAD1 repository"
    echo [OK] Local changes successfully committed!
    echo.
) else (
    echo [OK] Workspace is clean (all files already committed).
    echo.
)

:: 5. Sync active branch to local main so both are up to date
if not "!CURRENT_BRANCH!"=="main" (
    echo [*] Synchronizing '!CURRENT_BRANCH!' into local 'main'...
    git checkout main >nul 2>nul
    git merge !CURRENT_BRANCH! --no-edit >nul 2>nul
    git checkout !CURRENT_BRANCH! >nul 2>nul
    echo [OK] Local 'main' synchronized with '!CURRENT_BRANCH!'.
    echo.
)

echo ----------------------------------------------------------------------
echo Select a push option:
echo ----------------------------------------------------------------------
echo  [1] Push to GitHub 'main' branch (Recommended - Updates full app on GitHub)
echo  [2] Push ALL branches - main and all feature branches
echo  [3] Push current active branch [!CURRENT_BRANCH!] to bad1
echo  [4] Make 'bad1' the primary 'origin' remote and push
echo  [5] Check Git status and remotes
echo  [6] Exit
echo ----------------------------------------------------------------------

if "%~1"=="" (
    set /p ACTION="Enter choice 1-6 [default: 1]: "
) else (
    set ACTION=%~1
)

if "%ACTION%"=="" set ACTION=1

if "%ACTION%"=="1" goto PUSH_MAIN
if "%ACTION%"=="2" goto PUSH_ALL
if "%ACTION%"=="3" goto PUSH_CURRENT
if "%ACTION%"=="4" goto SET_ORIGIN
if "%ACTION%"=="5" goto SHOW_STATUS
if "%ACTION%"=="6" goto EXIT_SCRIPT

echo [!] Invalid choice. Defaulting to Option 1.
goto PUSH_MAIN

:PUSH_MAIN
echo.
echo ======================================================================
echo Pushing latest code to 'main' on GitHub (https://github.com/hardikjariwala92-cyber/BAD1)...
echo ======================================================================
echo.
git push -u bad1 HEAD:main --force
if %ERRORLEVEL% equ 0 (
    git push -u bad1 main --force >nul 2>nul
)
goto CHECK_RESULT

:PUSH_ALL
echo.
echo ======================================================================
echo Pushing ALL branches to GitHub bad1 repo...
echo ======================================================================
echo.
git push --all bad1 --force
goto CHECK_RESULT

:PUSH_CURRENT
echo.
echo ======================================================================
echo Pushing branch '!CURRENT_BRANCH!' to bad1 repo...
echo ======================================================================
echo.
git push -u bad1 !CURRENT_BRANCH! --force
goto CHECK_RESULT

:SET_ORIGIN
echo.
echo ======================================================================
echo Setting 'bad1' as primary 'origin'...
echo ======================================================================
git remote | findstr /i "^team-origin$" >nul 2>nul
if %ERRORLEVEL% neq 0 (
    git remote rename origin team-origin 2>nul
)
git remote rename bad1 origin 2>nul
git remote set-url origin https://github.com/hardikjariwala92-cyber/BAD1.git
echo [OK] Primary origin set to https://github.com/hardikjariwala92-cyber/BAD1.git
echo.
echo Pushing 'main' to origin...
git push -u origin HEAD:main --force
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
if %ERRORLEVEL% equ 0 (
    color 0A
    echo.
    echo ======================================================================
    echo [SUCCESS] Push completed successfully!
    echo Your code is live at: https://github.com/hardikjariwala92-cyber/BAD1
    echo ======================================================================
) else (
    color 0C
    echo.
    echo ======================================================================
    echo [NOTICE] Push exited with code %ERRORLEVEL%.
    echo If GitHub Credential Manager opened a browser popup, please sign in.
    echo.
    echo Troubleshooting:
    echo 1. If a browser window opened, click 'Sign in with your browser'.
    echo 2. If prompted for password in terminal, enter a GitHub Personal
    echo    Access Token with 'repo' scope from:
    echo    https://github.com/settings/tokens
    echo 3. Refer to docs\PUSH_TO_BAD1_GUIDE.md for step-by-step instructions.
    echo ======================================================================
)
echo.
pause
goto :eof

:EXIT_SCRIPT
echo Exiting.
goto :eof
