@echo off
echo Compiling GLSL shaders to SPIR-V...

set SHADER_DIR=Infrastructure\Vulkan\Shaders

if not exist "%VULKAN_SDK%\Bin\glslc.exe" (
    if exist "C:\VulkanSDK\1.4.335.0\Bin\glslc.exe" (
        set GLSLC="C:\VulkanSDK\1.4.335.0\Bin\glslc.exe"
    ) else (
        echo ERROR: glslc.exe not found. Please install Vulkan SDK.
        pause
        exit /b 1
    )
) else (
    set GLSLC="%VULKAN_SDK%\Bin\glslc.exe"
)

%GLSLC% %SHADER_DIR%\raytracing.comp -o %SHADER_DIR%\raytracing.comp.spv

if %ERRORLEVEL% EQU 0 (
    echo Shader compilation successful!
) else (
    echo ERROR: Shader compilation failed!
    pause
    exit /b 1
)

echo Done.
