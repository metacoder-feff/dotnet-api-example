
# Async Locks comparation

|  | Binary<br>Lock | Counting<br>Lock | Do not throw at Release<br>after Dispose (*1) | Interrupt Lock awaiting<br>on Dispose (*2) | Graceful<br>releasing (*3) | Reenterant |
|---|---|---|---|---|---|---|
| DotNext.AsyncLock.Semaphore | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| DotNext.AsyncLock.Exclusive | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| FEFF.Extentions.SemaphoreLock | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

* (*1) The aim of 'FEFF.Extentions.SemaphoreLock' is to avoid concurrent access to a resource. There is no matter to throw 'ObjectDisposedExeception' at Release() because Lock would never be entered again when it is disposed.
* (*2) ❌ Awaiting 'DotNext.AsyncLock.Semaphore' (SemaphoreSlim) would be infinite if it is disposed after awaiting has began. Even with defined Timeout.
  * ✅ In other cases awating is interruped with 'ObjectDisposedExeception'.
* (*3) Use 'DotNext.AsyncLock.Exclusive.**DisposeAsync**' to dispose the lock and wait for all acquisitions to be released. 
  * 'DotNext.AsyncLock.Exclusive.**Dispose**' just disposes the lock.
  * 'DotNext.AsyncLock.Semaphore' has method 'DisposeAsync' but it does NOT wait for anything.