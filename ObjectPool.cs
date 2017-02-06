using System;
using System.Collections.Generic;

namespace SimpleObjecPool
{
    public abstract class ObjectPool<T>
    {
        private readonly object _syncLock = new object();
        private readonly long _expirationTime;

        private readonly Dictionary<T, long> _locked;
        private readonly Dictionary<T, long> _unlocked;

        protected ObjectPool() {
            _expirationTime = 30000; // 30 seconds
            _locked = new Dictionary<T, long>();
            _unlocked = new Dictionary<T, long>();
        }

        protected abstract T Create();
        public abstract bool Validate(T o);
        public abstract void Expire(T o);

        public  T CheckOut() {
            lock (_syncLock)
            {
                long now = GetSystemMillis();
                T t;
                if (_unlocked.Count > 0)
                {
                    Dictionary<T, long>.KeyCollection.Enumerator e = _unlocked.Keys.GetEnumerator();
                    while (e.MoveNext()) {
                        t = e.Current;

                        long unlockTime;
                        _unlocked.TryGetValue(t, out unlockTime);

                        if ( unlockTime!= default(long) && (now - unlockTime > _expirationTime)) {
                            // object has expired
                            _unlocked.Remove(t);
                            Expire(t);
                            t = default(T);
                        } else {
                            if (Validate(t)) {
                                _unlocked.Remove(t);
                                _locked.Add(t, now);
                                return t;
                            } else {
                                // object failed validation
                                _unlocked.Remove(t);
                                Expire(t);
                                t = default(T);
                            }
                        }
                    }
                }
                // no objects available, create a new one
                t = Create();
                _locked.Add(t, now);
                return t;
            }
        }

        public void CheckIn(T t) {
            lock (_syncLock){
                _locked.Remove(t);
                _unlocked.Add(t, GetSystemMillis());
            }
        }

        private long GetSystemMillis()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long millis = (long)ts.TotalMilliseconds;
            return millis;
        }
    }
}
