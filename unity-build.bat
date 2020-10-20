REM The Unity Editor must be closed for this script to work

set UNITY_EDITOR_LOCATION=C:\Program Files\Unity\Hub\Editor\2018.4.12f1\Editor\Unity.exe
set UNITY_PROJECT_PATH=.\

set WEB_TARGET_PATH=.\build\target\webgl

REM Build webGL
del /s /q "%WEB_TARGET_PATH%\*.*"
"%UNITY_EDITOR_LOCATION%" -quit -batchmode -projectPath "%UNITY_PROJECT_PATH%" -executeMethod BuildWebGL.Build -buildPath "%WEB_TARGET_PATH%"

