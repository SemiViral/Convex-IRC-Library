# 1.0.0

 - First release

## 1.2.0

#### General

 - Server-based variables and operations now have their own class

 - The old `ExecuteRuntime` method is now fired by an event in the `Server` class

 - Stream objects now have a custom base class, `Stream` in `Convex.Resources`.
   - a note: logging to file doesn't work currently, will fix in a later patch

##- 1.3.2 — .NET Standard 1.6

#### General

 - Library target type from `.NET Framework 4.6.2` to `.NET Standard 1.6`

 - Implemented full asynchronosity

 - Logging is now handled by the [Serilog](https://serilog.net/) dependency.
 
 - **Server.cs**
    - listener is now changed to be recursively triggered (see: `QueueAsync(Client caller)`).
 
 - **Config**
    - added static fields of default filepaths.
    - `Log/DatabaseFilePath` defaults to `DefaultLog/DatabaseFilePath` if left unset.

 - **Database.cs**
    - `ObservableCollection<User> Users` has been moved to the `Database` class.
 
 - **README.md**
    - Added some info

#### Plugins

 - Plugins are no longer unloadable. The assembly instances are now loaded into the base AppDomain at runtime.

 - Methods are now subscribed to an `AsyncEvent` event, rather than instanced separately in a list.
    - Accordingly, subscribed methods must now be asynchronous
 
#### Bug Fixes

 - Logging is now fixed.

 - Config file is now saved correctly.

## 1.3.3

#### General

 - Version is now assigned at runtime from assembly

 - Plugins directory is now absolute from runtime root directory

## 1.3.5

#### General

 - Fixed some possible encapsulation issues with public/private methods and properties.
 - Assembly version in `Client.cs` and `Core.cs` are now accurately assigned.

 - **Config.cs**
    - Renamed `ClientConfiguration.cs`.
    - Moved `Server` property to `Client.cs`.

 - **Message.cs**
    - Added this as base class for `ChannelMessage` and `SimpleMessage` to inherit from.


 - **Convex.Plugins.Core**
    - Renamed `Convex.Plugin.Core`.

#### Plugins
 
 - The code in `PluginHost.cs` should garner a clearer structure

#### Logging

 - `logger` in `Client.cs` is now a backing field for the property `Logger`.
 
 - The property automatically disposes of the previous logger when set, and assigns the new static logger in `Log.Logger`

 - If writeToConsole in `Client` constructor is *TRUE*, console is assured to be activated.

#### Bug Fixes

 - Plugins are now properly loaded into the assembly.

## 1.3.7 — Simplified async!

#### General 

 - The namespace structure has been changed to more accurate reflect the naming conventions set by MSDN.

 - Events now use `AsyncEventHandler`, offering vastly simplified implementation.

##### SimpleMessage

 - renamed `CommandEventArgs`

##### ChannelMessagedEventArgs

 - renamed `ServerMessagedEventArgs`

## 1.3.8

#### General

 - Logging is temporarily no longer handled by the Convex library.
  - this is likely to change. I need to work out how to handle it internally so it has the smallest impact on the overall package.

## 1.3.9.3

#### General

 - Downgraded to .NETStandard 1.5 for compatability with current .NETFramework versions

 - **CHANGELOG.md**
   - Updated format to be more readable

 - **Client.cs**
   - New constructor: `public Client(string address, int port, string friendlyname = "")`
   - Added `FriendlyName` property
 
 - **Server.cs**
   - New constructor: `public Server(Connection connection)`

## 1.3.9.7

#### General

 - **ADDED: MethodDomain.cs**
  - Overlay for keeping and adding plugin methods

 - **IAsyncRegistrar.cs**
  - `string Command` to `object Identifier`