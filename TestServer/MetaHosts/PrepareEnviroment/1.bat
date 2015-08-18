echo Registering FxAdvisorCore.dll...
$(ProjectDir)gacutil.exe /i $(TargetDir)FxAdvisorCore.dll /f /nologo

echo Registering AForge.dll...
$(ProjectDir)gacutil.exe /i $(TargetDir)AForge.dll /f /nologo

echo Registering Common.dll...
$(ProjectDir)gacutil.exe /i $(TargetDir)Common.dll /f /nologo

if defined META_FOLDER (
echo Updating Meta Trader...
copy /y $(TargetDir)MetaClientWrapper.dll  "%META_FOLDER%experts\libraries\"

echo Updating Meta Trader...
rem copy /y "$(SolutionDir)metaScripts\libraries\DotNetBridgeLib.mq4" "%META_FOLDER%experts\libraries\"
)

xcopy /y "$(SolutionDir)ArbitrationBrain\bin\Debug\ArbitrationBrain.dll"  "$(SolutionDir)FxAdvisorHost\bin\Debug\Advisors\"

