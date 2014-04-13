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

Compiling
-------

**Windows**
Either compile with visual studio or the included compile.bat script
pass it %1 as the project name, either "nvm2" or "ncc"

**Linux**
Compile the sln with monodevelop.

Why make this?
-------
I made this to understand how computers work.
I have allways wanted to make my own programming language, because I have allways been interested in parsing and how code is translated into stuf happening.
After multiple failed atempts at trying to make a programming language without any kind of direction or guidance really, I finally looked up how to make my own programming language. This made me familliar with parsing and I still use a modified lexer/scanner of the first I made to this day. Then, since I thought translating to IL or real assembly was to hard, I made my own IL, for my own machine.
The first was a pretty basic one, [NCVM](https://github.com/Northcode/NCVM), its still on github to this day. Warning, its memory allocation is BAD, REAL BAD.
My second, third and fourth one can be found in the [nvm repo](https://github.com/Northcode/nvm) (though some of these may be unfinished).

This is the first one that has paging, virtual address translation and simulates parts like hard disks, ram chips, cpus, and cd roms.
