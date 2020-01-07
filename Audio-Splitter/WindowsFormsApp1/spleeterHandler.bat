@echo off

set type=%1
set source=%2
set outputPath=%3
set stems=%4
set audioFormat="mp3"

REM %type% %source% %outputPath% %stems%

REM for youtube output
ECHO Creating temp folder
MD temp

cd ./spleeterEnvP362

REM Paths should be encased in quotes!

IF "%type%"  == "-file" (
    ECHO Processing local audio file with spleeter...
    python.exe Scripts/spleeter.exe separate -i %source% -p spleeter:%stems%stems -o %outputPath%
) ELSE (
    ECHO using link
    SET fileName=""
    SET cmd=python.exe youtube-dl --get-filename -o "%%(title)s.mp3" %source% --restrict-filenames
    FOR /F "tokens=*" %%i IN ('python.exe youtube-dl --get-filename -o "%%(title)s.mp3" %source% --restrict-filenames --ffmpeg-location ../ffmpeg/bin') DO SET fileName=%%i
	:wait
	IF "%fileName%" == "" goto wait
    ECHO file name is %fileName%
    ECHO Grabbing audio from youtube link...

    python.exe youtube-dl %source% -o "../temp/%fileName%" -x --audio-format %audioFormat% --ffmpeg-location ../ffmpeg/bin
    ECHO Processing audio from youtube link with spleeter...
	ECHO python.exe Scripts/spleeter.exe separate -i "../temp/%fileName%" -p spleeter:%stems%stems -o %outputPath%
    python.exe Scripts/spleeter.exe separate -i "../temp/%fileName%" -p spleeter:%stems%stems -o %outputPath%
)

IF "%stems%"  == "" (
    ECHO no stem amount selected
) ELSE (
    ECHO using %stems% stems
)

cd ../

ECHO Done.

ECHO Removing temp folder

CD temp
DEL *.*
CD ..
RD temp

PAUSE

REM spleeterHandler.bat -file "C:\Users\SirHieppi\Music\Lauv Feelings.mp3" C:\Users\SirHieppi\Music 2
REM spleeterHandler.bat -file https://www.youtube.com/watch?v=jcuIJ9waAVw C:\Users\SirHieppi\Music 2
