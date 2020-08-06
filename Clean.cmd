@echo off
REM https://stackoverflow.com/questions/10178983/batch-file-to-clean-unity-directory-of-junk-files
echo %cd%
echo "this will delete some files within the directory above!  Make sure unity is not running!"
pause
echo "are you sure you would like to do this?"
pause
rd /s /q Library
rd /s /q Temp
del /s /q /f *.csproj
del /s /q /f *.pidb
del /s /q /f *.unityproj
del /s /q /f *.DS_Store
del /s /q /f *.sln
del /s /q /f *.userprefs
echo "done."
pause
