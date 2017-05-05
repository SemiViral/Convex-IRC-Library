# 1.0.0

 - First release

## 1.2.0

#### General

 - Server-based variables and operations now have their own class

 - The old `ExecuteRuntime` method is now fired by an event in the `Server` class

 - Stream objects now have a custom base class, `Stream` in `Convex.Resources`.

   - a note: logging to file doesn't work currently, will fix in a later patch

## 1.3.2 — .NET Standard 1.6

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

## 1.3.3

#### General

 - Version is now assigned at runtime from assembly
 - Plugins directory is now absolute from runtime root directory