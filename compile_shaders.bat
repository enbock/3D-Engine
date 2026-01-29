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
%GLSLC% %SHADER_DIR%\pass1_primary.comp -o %SHADER_DIR%\pass1_primary.comp.spv
%GLSLC% %SHADER_DIR%\pass2_lighting.comp -o %SHADER_DIR%\pass2_lighting.comp.spv
%GLSLC% %SHADER_DIR%\pass3_reflections.comp -o %SHADER_DIR%\pass3_reflections.comp.spv
%GLSLC% %SHADER_DIR%\pass4_composite.comp -o %SHADER_DIR%\pass4_composite.comp.spv

echo.
echo All shaders compiled successfully!
echo Done.
exit /b 0

:error
echo.
echo ERROR: Shader compilation failed!
exit /b 1
