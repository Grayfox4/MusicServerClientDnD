# MusicServerClientDnD
a  way to stream music from the pc of the DM to the players

This repo contains everything needed for a group of players to share sound over ip4 tcp

There is a simple, minimalistic GUI that controls the program. It should still look modern and cool.

The user should be given a choice in the beginning - Client or Host. Depending on the user selection, the user will be in Client or Host mode.

A client gets a means to connect their client with a stream from the Host. This is a password protected one-way data stream. Once connected, the client can disconnect, change hosts, or alter volume with a slider or a percentage entry.

A host gets shown their TCP ip or whatever else is applicable, for the clients to connect to. They get to set a password that will encrypt the stream. A password on the client side decrypts it. Be smart about this. They will be able to use the Windows API to choose an audio source, much like the volume sliders when you click the symbol. Each audio source will have a checkbox and a volume slider. When the box is checked, the audio will be streamed. The volume slider sets the outgoing volume level. 

All streams are encrypted and password protected.