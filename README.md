# game-control
Windows 8 (and up) app for controlling video game playing

I wanted some kind of program to give me warnings and even stop the game that I've been playing if it's been a while, but I 
couldn't find one anywhere. This app is very specific to my stuff, but it can easily be updated. Also, I know I could have 
added some of the configuration in a better place, but I purposefully hard coded the values in so that I wouldn't just change 
them to get around it. There's also a hidden project in here (UpdateRegistry) that I needed to be a separate process to run
with permissions.

Also worthy of note is that I set up a scheduled task in Windows to run every 10 minutes to check if anything is running.
