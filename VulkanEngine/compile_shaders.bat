@echo off
echo Compiling GLSL shaders to SPIR-V...

set SHADER_DIR=Infrastructure\Vulkan\Shaders

if not exist "%VULKAN_SDK%\Bin\glslc.exe" (
    echo ERROR: glslc.exe not found. Please install Vulkan SDK.
    pause
    exit /b 1
)

"%VULKAN_SDK%\Bin\glslc.exe" %SHADER_DIR%\raytracing.comp -o %SHADER_DIR%\raytracing.comp.spv

if %ERRORLEVEL% EQU 0 (
    echo Shader compilation successful!
) else (
    echo ERROR: Shader compilation failed!
    pause
    exit /b 1
)

echo Done.
