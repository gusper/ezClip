This is a .NET console application that I wrote back in 2008. 

---

This is a tiny C# application I wrote a while back that allows you to copy/paste files via the clipboard. The main scenario I use it in is when I have multiple command prompt windows open and I want to copy files from one to the other without having to type in paths, etc. It also now supports executing an application or a command (e.g., dir) and copying the output of that process to the clipboard (basically skip the common step of piping the output of something to a file and then opening it in an editor like notepad to copy/paste it somewhere else). An even easier way is to just pipe data to ezclip directly. For example, executing ‘dir /s /b | ezclip' (assuming ezclip.exe is in your %path% environment variable) will directly copy the output of the directory command to the clipboard.

Usage information:
ezClip [ copy | paste | add | list | output ] <list of files (wildcards allowed)>

For example:
'ezClip copy somefile.txt myapp.exe' or 'ezclip c somefile.txt myapp.exe'
will copy somefile.txt and myapp.exe to the clipboard.

'ezClip list' or 'ezclip l'
will display the files currently in the clipboard.

'ezClip paste' or 'ezclip p'
will paste any files that are in the clipboard into the current directory.

'ezClip add *.mp3' or 'ezclip a *.mp3'
will add all MP3 files to the files already in the clipboard.

'ezClip output dir /b /s' or 'ezclip o dir s/b /s'
will copy the output of the dir command (or any other command/executable) to the clipboard.

'<command> | ezClip' for example,  'dir /b /s | ezclip'
will copy the output being piped in directly to the clipboard.

'ezClip' or 'ezclip ?'
will display help on usage, version, and support information.

Installation Instructions: 

Unzip the ezclip.zip file. Copy the ezclip.exe file to a directory that is also in your system's %PATH% environment variable so it's easy to access from any other location. 

Other contributors

Thanks to Joel McIntyre (http://www.joelmcintyre.com) for contributing a few very useful features.

Latest Version:
- 1.20 - Released on July 20, 2008
