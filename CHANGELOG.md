# 1.0.0

 - First release

## 1.2.0

#### General

 - Server-based variables and operations now have their own class

 - The old `ExecuteRuntime` method is now fired by an event in the `Server` class

 - Stream objects now have a custom base class, `Stream` in `Convex.Resources`.
   - a note: logging to file doesn't work currently, will fix in a later patch

### 1.3.2 — .NET Standard 1.6

#### General

 - Library target type from `.NET Framework 4.6.2` to `.NET Standard 1.6`

 - Implemented full asynchronosity

 - Logging is now handled by the [Serilog](https://serilog.net/) dependency.
 
 - Server's listener is now changed to be recursively triggered (see: `Server.cs, QueueAsync(Client caller)`).
 
 - Config now houses the default file paths as static strings.
    - Along with this, the file paths for Database and Log in the config file are left blank. Iif left empty the default file paths will be used.

 - `ObservableCollection<User> Users` has been moved to the `Database` class.
 
 - Small update to README.

#### Plugins

 - Plugins are no longer unloadable. The assembly instances are now loaded directly into the base assembly at runtime.

 - Methods are now subscribed to an `AsyncEvent` event, rather than instanced separately in a list.
    - Accordingly, subscribed methods must now be asynchronous
 
#### Bug Fixes

 - Logging is now fixed.

 - Config file is now saved correctly.

### 1.3.3

#### General

 - Version is now assigned at runtime from assembly

 - Plugins directory is now absolute from runtime root directory

### 1.3.5

#### General

 - `Config` has changed to `ClientConfiguration`
    - `Server` object no longer in the object, or the JSON config file.

 - There is now a base type `Message`, which `ChannelMessage` and `SimpleMessage` (previously `SimpleReturnMessage`) inherit from.

 - Fixed some possible encapsulation issues with public/private methods and properties

 - Assembly version in `Client.cs` and `Core.cs` are now accurately assigned

 - Moved the `Convex.Plugins.Core` to namespace contents to `Convex.Plugin.Core`, in accordance with Convex namespace naming conventions.

#### Plugins
 
 - The code in `PluginHost.cs` should garner a clearer structure

#### Logging

 - `logger` in `Client.cs` is now a backing field for the property `Logger`.
 
 - The property automatically disposes of the previous logger when set, and assigns the new static logger in `Log.Logger`

 - If writeToConsole in `Client` constructor is *TRUE*, console is assured to be activated.

#### Bug Fixes

 - Plugins are now properly loaded into the assembly.

### 1.3.7 — Simplified async!

#### General 

 - The namespace structure has been changed to more accurate reflect the naming conventions set by MSDN.

 - Events now use `AsyncEventHandler`, offering vastly simplified implementation.

##### SimpleMessage

 - renamed `CommandEventArgs`

##### ChannelMessagedEventArgs

 - renamed `ServerMessagedEventArgs`

### 1.3.8

#### General

 - Logging is temporarily no longer handled by the Convex library.
  - this is likely to change. I need to work out how to handle it internally so it has the smallest impact on the overall package.

### 1.3.9.3

#### General

 - Downgraded to .NETStandard 1.5 for compatability with current .NETFramework versions

 - *CHANGELOG.md*
  - Updated format to be more readable

 - *Client.cs*
  - New constructor: `public Client(string address, int port, string friendlyname = "")`
  - Added `FriendlyName` property

 - *Server.cs*
  - New constructor: `public Server(Connection connection)`
