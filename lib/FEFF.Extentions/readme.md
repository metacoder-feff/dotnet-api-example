|  | Binary<br>Lock | Counting<br>Lock | Release after Dispose<br>Not Throws (*1) | Interrupt Lock awaiting<br>on Dispose (*2) | Graceful Dispose (*3) |
|---|---|---|---|---|---|
| DotNext.AsyncLock.Semaphore | ✅ | ✅ | ❌ | ❌ | ✅ |
| DotNext.AsyncLock.Exclusive | ✅ | ❌ | ❌ | ✅ | ❌ |
| FEFF.Extentions.SemaphoreLock | ✅ | ✅ | ✅ | ✅ | ❌ |

* (*1) The aim of 'FEFF.Extentions.SemaphoreLock' is to avoid concurrent access to a resource. There is no matter to throw at Release() because Lock would never be entered again when it is disposed.
* (*2) Awaiting DotNext.AsyncLock.Semaphore (SemaphoreSlim) would be infinite if it is disposed after awaiting began. Even with defined Timeout.
  * When awating is interruped (✅) it continues with ObjectDisposedExeception.
* (*3) Use DotNext.AsyncLock.Exclusive.DisposeAsync to dispose and wait for all locks to be released. 
  * DotNext.AsyncLock.Exclusive.Dispose just disposes the lock.
  * DotNext.AsyncLock.Semaphore has method DisposeAsync but it does NOT wait for anything.