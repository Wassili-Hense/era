#region license
//Copyright (c) 2011-2014 <comparator@gmx.de>; Wassili Hense

//This file is part of the X13.Home project.
//https://github.com/X13home

//BSD License
//See LICENSE.txt file for license details.
#endregion license

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace X13 {
  public class Log {

    static Log() {
      if(!Directory.Exists("../log")) {
        Directory.CreateDirectory("../log");
      } else {
        DateTime now=DateTime.Now;
        try {
          foreach(string f in Directory.GetFiles("./log/", "*.log", SearchOption.TopDirectoryOnly)) {
            if(File.GetLastWriteTime(f).AddDays(40)<now) {
              File.Delete(f);
            }
          }
        }
        catch(System.IO.IOException) {
        }
      }
    }

    public static void Debug(string format, params object[] arg) {
      onWrite(LogLevel.Debug, format, arg);
    }
    public static void Info(string format, params object[] arg) {
      onWrite(LogLevel.Info, format, arg);
    }
    public static void Warning(string format, params object[] arg) {
      onWrite(LogLevel.Warning, format, arg);
    }
    public static void Error(string format, params object[] arg) {
      onWrite(LogLevel.Error, format, arg);
    }
    public static void onWrite(LogLevel ll, string format, params object[] arg) {
      string msg=string.Empty;
      DateTime now=DateTime.Now;
      switch(ll) {
      case LogLevel.Error:
        msg=string.Format("{0:HH:mm:ss.ff}[E] {1}", now, string.Format(format, arg));
        Console.ForegroundColor=ConsoleColor.Red;
        Console.WriteLine(msg);
        break;
      case LogLevel.Warning:
        msg=string.Format("{0:HH:mm:ss.ff}[W] {1}", now, string.Format(format, arg));
        Console.ForegroundColor=ConsoleColor.Yellow;
        Console.WriteLine(msg);
        break;
      case LogLevel.Info:
        msg=string.Format("{0:HH:mm:ss.ff}[I] {1}", now, string.Format(format, arg));
        Console.ForegroundColor=ConsoleColor.White;
        Console.WriteLine(msg);
        break;
      case LogLevel.Debug:
        msg=string.Format("{0:HH:mm:ss.ff}[D] {1}", now, string.Format(format, arg));
        Console.ForegroundColor=ConsoleColor.Gray;
        Console.WriteLine(msg);
        break;
      }
      try {
        File.AppendAllText(string.Format("../log/{0:yyMMdd}.log", now), msg+"\n");
      }
      catch(Exception) {
      }
    }
  }
  public enum LogLevel {
    Debug,
    Info,
    Warning,
    Error
  }
}
