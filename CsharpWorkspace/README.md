# MusicServerClientDnD

A C# .NET 8.0 Windows Forms application for streaming audio from a host to clients over encrypted TCP.

## Features
- Modern, minimalistic GUI
- Host or Client mode selection at startup
- Host: select audio sources, set password, stream encrypted audio to clients
- Client: connect to host with IP and password, receive and decrypt audio, adjust playback volume
- All communication is encrypted and password protected
- Easy double-clickable .exe for Windows users

## Getting Started
1. Ensure you have .NET 8.0 Runtime installed
2. Build the solution
3. Run the generated executable

## Project Structure
- `MusicServerClientDnD/` - Main Windows Forms application
- `.github/copilot-instructions.md` - Copilot custom instructions

## License
MIT
