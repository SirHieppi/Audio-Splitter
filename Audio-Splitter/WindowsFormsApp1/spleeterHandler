#!/bin/bash

#example = ./spleeterHandler -file ./myAudioFile.mp3 ~/documents 4

# $1 == yt or local file
# $2 == path or youtube link
# $3 == output for spleeter files
# $4 == amount of stems

# Progress indicator
# while [ $done -eq 0 ]; do
# 	for s in / - \\ \|; do
# 		printf "\r$s";sleep .1;
# 	done;
# done

# For youtube output
if [ ! -d "./temp" ]; then
	mkdir temp
fi

cd ./spleeterEnvP362

cmd=""

#OUTPUT="$(./python ./youtube-dl)"
#echo "${OUTPUT}"

# Set output path
if [ -z "$3" ]; then
	# Default output path
	outputPath="../output"
else
	outputPath=$3
fi

# Set output path
if [ -z "$4" ]; then
	# Default
	stems="2"
else
	if [ $4 -eq 0 ]; then
		stems="2"
	else
		stems=$4
	fi
fi
stems=$stems"stems"

# Example spleeter command
# spleeter separate -i spleeter/audio_example.mp3 -p spleeter:2stems -o output
# Use spleeter
#------------------------------currently in WpfApp1/spleeterEnvP362------------------------------

audioFormat="mp3"

# Fix output path for windows path
# outputPath="$(echo $outputPath | tr '\\' '/' | cat)"

if [ $1 = "-file" ]; then
	echo "Processing local audio file with spleeter..."
	cmd="./python.exe ./Scripts/spleeter.exe separate -i \"$2\" -p spleeter:$stems -o $outputPath"
	echo "Processed local audio file at $outputPath"
	echo $cmd | sh

elif [ $1 = "-link" ]; then
	# Download mp3

	fileName="$(./python.exe ./youtube-dl --get-filename -o '%(title)s.mp3' $2 --restrict-filenames)"

	echo "Grabbing audio from youtube link..."
	# cmd="./python.exe ./youtube-dl $2 -o ../temp/spleeterOutput.$audioFormat -x --audio-format mp3"
	cmd="./python.exe ./youtube-dl $2 -o '../temp/$fileName' -x --audio-format mp3"

	echo $cmd | sh

	echo "Processing audio from youtube link with spleeter..."
	# cmd="./python.exe ./Scripts/spleeter.exe separate -i ../temp/spleeterOutput.$audioFormat -p spleeter:$stems -o $outputPath"
	cmd="./python.exe ./Scripts/spleeter.exe separate -i '../temp/$fileName' -p spleeter:$stems -o $outputPath"

	echo $cmd | sh
else
	cmd="no command created"
fi

# Remove temp dir
cd ..
rm -r ./temp

# if [ "$(./condaEnv/python.exe ./condaEnv/youtube-dl | grep 'error')" ]; then
# 	echo "an error has occured"
# else
# 	echo "no errors occured"
# fi

# Execute command
# echo "$cmd" | sh

# sleep 10

echo "Done"

exit 0
