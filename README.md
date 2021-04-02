# DragonLegionBot
A .NET bot written for our Discord server.

### Running the bot
- In order for Lavalink to start, you need to set the JAVA_HOME environment variable to correspond to your Java install path. The current version of Lavalink provided in the source prefers the use of Java 13, though from 11 to 14 is compatible with minor issues.

- Replace `"TOKEN GOES HERE"` in appsettings.json with your bot token.

##### Windows
Just run the .exe after building and you should be done

##### Linux
Build the project using `dotnet build -c Release --runtime [RUNTIME]`, replacing `[RUNTIME]` with the appropriate runtime string for your distro.