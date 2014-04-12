Northcode Virtual Machine 2
===

This is a new version of my virtual machine, where it emulates a pc.
It runs a custom assembly language made for it and will have supporr for a c like language.
Has currently no graphics, only text io.
Uses a hard disk interface to mount a folder as a hard drive.

Contains a very basic bios and os written in the assembly language.
bios is loaded by adding the parameter -b to the arguments followed by the filename of the compiled bios.

nvm.exe contains the vm and an assembler for the language, using -h as an argument will give cmd line information of how to pass arguments.

ncc.exe is the c like compiler.