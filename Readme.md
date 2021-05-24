MusicHax for Lethal League Blaze, a mod to swap music easily.
Glomzubuk rewrote pretty much all of the code to make this Mod Manager compatible.

Program.cs is the file for compiling the ASMRewriter.exe, the other two files are for the mod .dll itself.
Refer to the guide for creating mods if you would like to compile it for yourself: https://docs.google.com/document/d/18CHOzfFKfhW9Ch-zbERdJ5kGqhePSTJ3D3EEeA6R1-Q/edit

To do list:
- [ ] Some day I would like to improve this to have a mod settings menu that can toggle vanilla music. ModMenuIntegration.cs was meant to do this but I was too lazy to finish it so it goes functionally unused. If you want to set this up, feel free to contribute to this repo. ~~Also, now that this has preload, add a toggle to enable/disable that for big music collections.~~
- [X] ~~Also, because this mod uses a weird type of code injection, it has an issue where uninstalling it won't actually remove it and reinstalling it will "stack" the injected code, which can cause long load times. A fix for this would be cool, but I couldn't figure out a clean one.~~ Done =)
