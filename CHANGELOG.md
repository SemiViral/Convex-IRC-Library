# 1.0.0
 - First release

## 1.2.0
 - Server-based variables and operations now have their own class
 - The old `ExecuteRuntime` method is now fired by an event in the `Server` class
 - Stream objects now have a custom base class, `Stream` in `Convex.Resources`.
   - a note: logging to file doesn't work currently, will fix in a later patch