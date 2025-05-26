@echo off
REM ==============================================
REM magick_alpha_off.bat
REM Recursively strip alpha channel from PNGs
REM matching *hir_col*.png
REM Usage: magick_process.bat [root_folder]
REM ==============================================

:: 1. Determine root directory (param or current dir)
set "rootDir=%~1"
if "%rootDir%"=="" set "rootDir=%CD%"

echo -------------------------------------------------
echo Stripping alpha from PNGs in: "%rootDir%"
echo -------------------------------------------------

:: 2. Loop through all PNGs containing "hir_col"
for /R "%rootDir%" %%F in (*hir_col*.png) do (
    echo Processing "%%~fF"
    magick "%%~fF" -alpha off "%%~fF"
)

:: 3. Loop through all PNGs containing "ehl"
for /R "%rootDir%" %%F in (*ehl*.png) do (
    echo Processing "%%~fF"
    magick "%%~fF" -fuzz 20%% -transparent black "%%~fF"
    magick "%%~fF" -channel A -blur 0x2 -level 20%%,80%% "%%~fF"
)

setlocal enabledelayedexpansion

:: Process all _hir_hhl.png files in subdirectories
for /R "%rootDir%" %%F in (*_hir_hhl.png) do (
    echo [Processing] Found highlight texture: "%%~fF"
    
    :: Generate corresponding _def filename
    set "hlPath=%%~fF"
    set "defPath=!hlPath:_hhl.png=_def.png!"
    
    :: Generate output filename
    set "outputPath=!hlPath:_hhl.png=_sph.png!"
    
    if exist "!defPath!" (
        echo [Matched] Found mask file: "!defPath!"
        
        :: Temporary file in source directory
        set "tempAlpha=%%~dpFtemp_alpha.png"
        
        :: Step 1: Extract red channel from _def as Alpha
        magick "!defPath!" -alpha extract -channel R -separate +channel "!tempAlpha!"
        
        :: Step 2: Combine _hl RGB with Alpha channel
        magick "%%~fF" "!tempAlpha!" -compose CopyOpacity -composite "!outputPath!"
        
        :: Cleanup temp file
        del "!tempAlpha!"
        echo [Success] Generated SPH texture: "!outputPath!"
    ) else (
        echo [Error] Missing mask file: "!defPath!"
    )
)

:: Process all _prp_col_alp.png files in subdirectories
for /R "%rootDir%" %%F in (*_prp_col_alp.png) do (
    echo [Processing] Found highlight texture: "%%~fF"
    
    :: Generate corresponding _def filename
    set "colPath=%%~fF"
    set "defPath=!colPath:_col_alp.png=_def.png!"
    
    :: Generate output filename
    set "outputPath=!colPath:_col_alp.png=_sph.png!"
    
    if exist "!defPath!" (
        echo [Matched] Found mask file: "!defPath!"
        
        :: Temporary file in source directory
        set "tempAlpha=%%~dpFtemp_alpha.png"
        
        :: Step 1: Extract red channel from _def as Alpha
        magick "!defPath!" -alpha extract -channel R -separate +channel "!tempAlpha!"
        
        :: Step 2: Combine _col RGB with Alpha channel
        magick "%%~fF" "!tempAlpha!" -compose CopyOpacity -composite "!outputPath!"
        
        :: Cleanup temp file
        del "!tempAlpha!"
        echo [Success] Generated SPH texture: "!outputPath!"
    ) else (
        echo [Error] Missing mask file: "!defPath!"
    )
)

echo -------------------------------------------------
echo Done.
echo -------------------------------------------------

pause