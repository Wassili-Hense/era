using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace X13 {
  public partial class era_svc : ServiceBase {
    static void Main(string[] args) {
      if(args.Length>0 && args[0]=="/c") {
        var svc=new era_svc();
        svc.OnStart(null);
        Console.WriteLine("\nPress Enter key to stop the server...");
        Console.ReadLine();
        svc.OnStop();
      } else {
        ServiceBase[] ServicesToRun;
        ServicesToRun = new ServiceBase[] 
            { 
                new era_svc() 
            };
        ServiceBase.Run(ServicesToRun);
      }
    }

    private HttpServer _sv;
    public era_svc() {
      InitializeComponent();
    }

    protected override void OnStart(string[] args) {
      _sv = new HttpServer(3128);
      _sv.Log.Output=WsLog;
      _sv.RootPath=Path.GetFullPath(Path.GetFullPath("../htdocs"));
      _sv.OnGet+=OnGet;
      _sv.AddWebSocketService<ApiV03>("/api/v03");
      _sv.Start();
      if(_sv.IsListening) {
        Log.Info("HttpServer started on {0}:{1} ", Environment.MachineName, _sv.Port.ToString());
      } else {
        Log.Error("HttpServer start failed");
      }

    }
    protected override void OnStop() {
      _sv.Stop();
    }

    private void WsLog(LogData d, string f) {
      Log.Debug("WS({0}) - {1}", d.Level, d.Message);
    }
    private void OnGet(object sender, HttpRequestEventArgs e) {
      var req = e.Request;
      var res = e.Response;
      if(req.RemoteEndPoint==null) {
        res.StatusCode=(int)HttpStatusCode.NotAcceptable;
        return;
      }
      if(req.HttpMethod!="GET") {
        res.StatusCode=(int)HttpStatusCode.MethodNotAllowed;
        return;
      }
      System.Net.IPEndPoint remoteEndPoint = req.RemoteEndPoint;
      {
        System.Net.IPAddress remIP;
        if(req.Headers.Contains("X-Real-IP") && System.Net.IPAddress.TryParse(req.Headers["X-Real-IP"], out remIP)) {
          remoteEndPoint=new System.Net.IPEndPoint(remIP, remoteEndPoint.Port);
        }
      }
      string path=req.RawUrl=="/"?"/index.html":req.RawUrl;
      string client;
      Session ses;
      if(req.Cookies["sessionId"]!=null) {
        ses=Session.Get(req.Cookies["sessionId"].Value, remoteEndPoint, false);
      } else {
        ses=null;
      }

      if(ses!=null && ses.owner!=null) {
        client=ses.owner.name;
      } else {
        client=remoteEndPoint.Address.ToString();
      }

      try {
        FileInfo f = new FileInfo(Path.Combine(_sv.RootPath, path.Substring(1)));
        if(f.Exists) {
          string eTag=f.LastWriteTimeUtc.Ticks.ToString("X8")+"-"+f.Length.ToString("X4");
          string et;
          if(req.Headers.Contains("If-None-Match") && (et=req.Headers["If-None-Match"])==eTag) {
            res.Headers.Add("ETag", eTag);
            res.StatusCode=(int)HttpStatusCode.NotModified;
            res.WriteContent(Encoding.UTF8.GetBytes("Not Modified"));
          } else {
            byte[] content;
            if((content =_sv.GetFile(path))!=null) {
              res.Headers.Add("ETag", eTag);
              res.ContentType=Ext2ContentType(f.Extension);
              res.WriteContent(content);
            } else {
              res.StatusCode=(int)HttpStatusCode.InternalServerError;
              res.WriteContent(Encoding.UTF8.GetBytes("Content is broken"));
            }
          }
        } else {
          res.StatusCode = (int)HttpStatusCode.NotFound;
          res.WriteContent(Encoding.UTF8.GetBytes("404 Not found"));
        }
        //if(_verbose.value) {
          Log.Debug("{0} [{1}]{2} - {3}", client, req.HttpMethod, req.RawUrl, ((HttpStatusCode)res.StatusCode).ToString());
        //}
      }
      catch(Exception ex) {
        //if(_verbose.value) {
          Log.Debug("{0} [{1}]{2} - {3}", client, req.HttpMethod, req.RawUrl, ex.Message);
        //}
      }
    }
    private string Ext2ContentType(string ext) {
      switch(ext) {
      case ".jpg":
      case ".jpeg":
        return "image/jpeg";
      case ".png":
        return "image/png";
      case ".css":
        return "text/css";
      case ".csv":
        return "text/csv";
      case ".htm":
      case ".html":
        return "text/html";
      case ".js":
        return "application/javascript";
      }
      return "application/octet-stream";
    }

  }
}
