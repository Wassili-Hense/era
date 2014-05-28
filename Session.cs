using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X13 {
  internal class Session {
    private static ConcurrentBag<WeakReference> _sessions;

    static Session() {
      _sessions=new ConcurrentBag<WeakReference>();
    }

    public static Session Get(string sid, System.Net.IPEndPoint re, bool create) {
      Session s;
      if(string.IsNullOrEmpty(sid) || (s=_sessions.Where(z => z.IsAlive).Select(z => z.Target as Session).FirstOrDefault(z => z!=null && z.id==sid && z.ip.Equals(re.Address)))==null) {
        if(create) {
          s=new Session(re);
          _sessions.Add(new WeakReference(s));
        } else {
          s=null;
        }
      }
      return s;
    }
    private string _host;
    public readonly string id;
    public readonly System.Net.IPAddress ip;
    public string userName;

    public Session(System.Net.IPEndPoint re) {
      this.id = Guid.NewGuid().ToString();
      this.ip = re.Address;
    }

  }
}
