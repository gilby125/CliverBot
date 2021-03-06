//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey_stoyan@yahoo.com
//        http://www.cliversoft.com
//        26 September 2006
//Copyright: (C) 2006-2013, Sergey Stoyan
//********************************************************************************************
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Cliver
{
    public partial class Log
    {
        public abstract class Writer
        {
            internal Writer(string name, string file_name, Session session)
            {
                Name = name;
                this.file_name = file_name;
                this.session = session;
            }

            public readonly string Name;
            public string FileName
            {
                get
                {
                    return file_name;
                }
            }
            string file_name = null;

            readonly Session session;

            public static int MaxFileSize = -1;

            internal const string MAIN_THREAD_LOG_NAME = null;

            /// <summary>
            /// Log path
            /// </summary>
            public string Path
            {
                get
                {
                    string directory;
                    switch (Log.mode)
                    {
                        case Cliver.Log.Mode.ONLY_LOG:
                            directory = Cliver.Log.WorkDir;
                            break;
                        //case Cliver.Log.Mode.SINGLE_SESSION:
                        case Cliver.Log.Mode.SESSIONS:
                            directory = session.Path;
                            break;
                        default:
                            throw new Exception("Unknown LOGGING_MODE:" + Cliver.Log.mode);
                    }
                    return directory + @"\" + FileName;
                }
            }

            /// <summary>
            /// Close the log
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (log_writer != null)
                        log_writer.Close();
                    log_writer = null;
                    _ErrorCount = 0;
                }
            }

            /// <summary>
            /// Write the error to the current thread's log
            /// </summary>
            /// <param name="e"></param>
            public void Error(Exception e)
            {
                string m;
                string d;
                Log.GetExceptionMessage(e, out m, out d);
                Write(Log.MessageType.ERROR, m, d);
            }

            /// <summary>
            /// Write the error to the current thread's log
            /// </summary>
            /// <param name="e"></param>
            public void Error(string message)
            {
                Write(Log.MessageType.ERROR, message, Log.GetStackString());
            }

            public void Error2(string message)
            {
                Write(Log.MessageType.ERROR, message);
            }

            /// <summary>
            /// Write the stack informtion for the caller to the current thread's log
            /// </summary>
            /// <param name="e"></param>
            public void Trace(object message = null)
            {
                if (message != null)
                    Write(Log.MessageType.TRACE, message.ToString(), Log.GetStackString());
                else
                    Write(Log.MessageType.TRACE, null, Log.GetStackString());
            }

            /// <summary>
            /// Write the error to the current thread's log and terminate the process.
            /// </summary>
            /// <param name="e"></param>
            public void Exit(string message)
            {
                lock (this)
                {
                    if (Name != MAIN_THREAD_LOG_NAME)
                        Log.Main.Write("EXITING: due to thread #" + Name + ". See the respective Log");
                    Write(Log.MessageType.EXIT, message, Log.GetStackString());
                }
            }

            public void Exit2(string message)
            {
                lock (this)
                {
                    if (Name != MAIN_THREAD_LOG_NAME)
                        Log.Main.Write("EXITING: due to thread #" + Name + ". See the respective Log");
                    Write(Log.MessageType.EXIT, message);
                }
            }

            /// <summary>
            /// Write the error to the current thread's log and terminate the process.
            /// </summary>
            /// <param name="e"></param>
            public void Exit(Exception e)
            {
                lock (this)
                {
                    if (Name != MAIN_THREAD_LOG_NAME)
                        Log.Main.Write("EXITING: due to thread #" + Name + ". See the respective Log");
                    string m;
                    string d;
                    Log.GetExceptionMessage(e, out m, out d);
                    Write(Log.MessageType.EXIT, m, d);
                }
            }

            public delegate void OnExitig(string message);
            static public event OnExitig Exitig = null;

            /// <summary>
            /// Write the warning to the current thread's log.
            /// </summary>
            /// <param name="e"></param>
            public void Warning(string message)
            {
                Write(Log.MessageType.WARNING, message);
            }

            /// <summary>
            /// Write the exception as warning to the current thread's log.
            /// </summary>
            /// <param name="e"></param>
            public void Warning(Exception e)
            {
                string m;
                string d;
                Log.GetExceptionMessage(e, out m, out d);
                Write(Log.MessageType.WARNING, m, d);
            }

            /// <summary>
            /// Write the notification to the current thread's log.
            /// </summary>
            /// <param name="e"></param>
            public void Inform(string message)
            {
                Write(Log.MessageType.INFORM, message);
            }

            public void Write(string line)
            {
                Write(Log.MessageType.LOG, line);
            }

            /// <summary>
            /// Write the message to the current thread's log.
            /// </summary>
            /// <param name="e"></param>
            public void Write(Log.MessageType type, string message, string details = null)
            {
                lock (this)
                {
                    if (type == Log.MessageType.EXIT)
                    {
                        if (exiting_thread != null)
                            return;
                        exiting_thread = new System.Threading.Thread(() =>
                        {
                            if (Exitig != null)
                                Exitig.Invoke(message);
                            write(type, message, details);
                            Environment.Exit(0);
                        });
                        exiting_thread.IsBackground = true;
                        exiting_thread.Start();
                    }
                    else
                        write(type, message, details);
                }
            }
            void write(Log.MessageType type, string message, string details)
            {
                lock (this)
                {
                    if (Writing != null)
                        Writing.Invoke(type, message, details);

                    if (Log.write_log)
                    {
                        if (log_writer == null)
                            log_writer = new StreamWriter(Path, true);

                        details = string.IsNullOrWhiteSpace(details) ? "" : "\r\n\r\n" + details;

                        switch (type)
                        {
                            case Log.MessageType.INFORM: message = "INFORM: " + message; break;
                            case Log.MessageType.WARNING: message = "WARNING: " + message; break;
                            case Log.MessageType.ERROR:
                                message = "ERROR: " + message + details;
                                _ErrorCount++;
                                break;
                            case Log.MessageType.EXIT:
                                message = "EXIT: " + message + details;
                                _ErrorCount++;
                                break;
                            case Log.MessageType.TRACE: message = "TRACE: " + message; break;
                            case Log.MessageType.LOG: break;
                            default: throw new Exception("No case for " + type.ToString());
                        }

                        if (MaxFileSize > 0)
                        {
                            FileInfo fi = new FileInfo(Path);
                            if (fi.Length > MaxFileSize)
                            {
                                log_writer.Close();

                                int counter = 0;
                                file_name = Regex.Replace(file_name, @"(\d+_)(\d+)(\.[^\.]+)$",
                                    (Match m) =>
                                    {
                                        counter = int.Parse(m.Groups[2].Value) + 1;
                                        return m.Groups[1].Value + counter + m.Groups[3].Value;
                                    }
                                    );
                                if (counter < 1)
                                    file_name = Regex.Replace(file_name, @"\.[^\.]+$", @"_1$0");

                                log_writer = new StreamWriter(Path, true);
                            }
                        }
                        //if (!string.IsNullOrWhiteSpace(details))
                        //    message += "\r\n\r\n" + details;
                        log_writer.WriteLine(DateTime.Now.ToString("[dd-MM-yy HH:mm:ss] ") + message);
                        log_writer.Flush();
                    }
                }
            }
            TextWriter log_writer = null;
            static protected System.Threading.Thread exiting_thread = null;

            public int ErrorCount
            {
                get
                {
                    return _ErrorCount;
                }
            }
            int _ErrorCount = 0;

            public delegate void OnWrite(Log.MessageType type, string message, string details);
            static public event OnWrite Writing = null;
        }
    }
}