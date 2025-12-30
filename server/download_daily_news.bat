@echo off
set BASE_DIR=C:\ACTUS_LIVEU\DAILY-NEWS

REM === COMMON OPTIONS ===
set YTDLP_OPTS=^
-f "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]" ^
--merge-output-format mp4 ^
--download-archive downloaded.txt ^
REM --playlist-end 18 ^
--max-downloads 18 ^
--match-filter "duration <= 900" ^
--dateafter now-3days ^
--no-playlist ^
--progress ^
--write-info-json ^
--newline

REM === BBC NEWS ===
yt-dlp %YTDLP_OPTS% ^
-o "%BASE_DIR%\videos\BBCNews\%%(upload_date)s_%%(title)s.%%(ext)s" ^
https://www.youtube.com/@BBCNews/videos

REM === SKY NEWS ===
yt-dlp %YTDLP_OPTS% ^
-o "%BASE_DIR%\videos\SkyNews\%%(upload_date)s_%%(title)s.%%(ext)s" ^
https://www.youtube.com/@SkyNews/videos

REM === DW NEWS ===
yt-dlp %YTDLP_OPTS% ^
-o "%BASE_DIR%\videos\DWNews\%%(upload_date)s_%%(title)s.%%(ext)s" ^
https://www.youtube.com/@DWNews/videos

REM === AL JAZEERA ===
yt-dlp %YTDLP_OPTS% ^
-o "%BASE_DIR%\videos\AlJazeera\%%(upload_date)s_%%(title)s.%%(ext)s" ^
https://www.youtube.com/@AlJazeeraEnglish/videos

echo Daily news ingestion completed.
pause
