# MusicServerClientDnD

A C# .NET 8.0 Windows Forms application for streaming audio from a host to clients over encrypted TCP.

## Features
- Modern, minimalistic GUI
- Host or Client mode selection at startup
- Host: select audio output device, set password, stream encrypted audio to clients
- Client: connect to host with IP and password, receive and decrypt audio, adjust playback volume
- All communication is encrypted and password protected
- Easy double-clickable .exe for Windows users

## Per-Application Audio Capture
To stream audio from a specific application (e.g., Spotify, VLC, Firefox), use a virtual audio device:

### VB-Audio Virtual Cable (Recommended, Free for personal use)
- Download: https://vb-audio.com/Cable/
- Install and set as the output device for your chosen app in Windows Sound settings.
- In MusicServerClientDnD, select "CABLE Output" as the source to stream only that app's audio.

### VoiceMeeter (Advanced, Free)
- Download: https://vb-audio.com/Voicemeeter/
- Lets you route audio from specific apps to virtual devices for advanced mixing and capture.

## Getting Started
1. Ensure you have .NET 8.0 Runtime installed
2. Build the solution or use the provided .exe
3. Run the generated executable

## Project Structure
- `MusicServerClientDnD/` - Main Windows Forms application
- `.github/copilot-instructions.md` - Copilot custom instructions

## License
MIT
