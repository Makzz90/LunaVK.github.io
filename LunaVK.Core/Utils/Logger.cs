using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace LunaVK.Core.Utils
{
    public class Logger
    {
        private object lockObj = new object();

        /// <summary>
        /// VKLog.txt
        /// </summary>
        private const string LOGNAME = "VKLog.txt";

        public Action<string> LogAdded;

        private List<string> _logItems = new List<string>();

        private bool _logInCobsole=false;

        private static Logger _instance;
        public static Logger Instance
        {
            get { return Logger._instance ?? (Logger._instance = new Logger()); }
        }
        /*
        public void LogMemoryUsage()
        {
            this.Info("Memory usage: {0}", (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage"));
        }
        */
        /// <summary>
        /// Добавляем лог в память
        /// </summary>
        /// <param name="message"></param>
        private void AddLogItem(string message)
        {
            this.LogAdded?.Invoke(message);
            this._logItems.Add(message);

#if DEBUG
            if (Debugger.IsAttached && this._logInCobsole)
                Debug.WriteLine(message);
#endif

            if (this._logItems.Count > 40)
                this._logItems.RemoveAt(0);
        }

        /// <summary>
        /// Возвращает записи текущей сессии
        /// </summary>
        /// <returns></returns>
        public string GetLog
        {
            get { return string.Join("\n", this._logItems); }
        }

        /// <summary>
        /// Записываем текущую сессию в файл
        /// </summary>
        public void Save()
        {
            //
            if (!Settings.DEV_IsLogsEnabled)
                return;
            //
            string str = string.Join("\n", this._logItems);
            this.WriteLogToStorage(str);
        }

        public void Assert(bool assertion, string commentOnFailure)
        {
            if (assertion)
                return;
            this.Info("ASSERTION FAILED, {0}", commentOnFailure);
        }

        /// <summary>
        /// Добавляем информационную запись к текущей сессии
        /// </summary>
        /// <param name="info"></param>
        /// <param name="formatParameters"></param>
        public void Info(string info, params object[] formatParameters)
        {
            string logMsg = info;
            if (formatParameters != null && formatParameters.Length != 0)
                logMsg = string.Format(info, formatParameters);
            
            string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ": " + logMsg;
            this.AddLogItem(str.Substring(0, Math.Min(500, str.Length)));
            if (!Settings.DEV_IsLogsEnabled)
                return;
            this.WriteLogToStorage(logMsg);
        }

        /// <summary>
        /// Добавляем ошибочную запись к текущей сессии и записывает в хранилище
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        /// <param name="code">Код ошибки</param>
        public void Error(string text, VKError error)
        {
            string str = "ERROR: " + text + Environment.NewLine + error.error_msg + " ErrorCode: " + error.error_code + Environment.NewLine;
            this.AddLogItem(str);

            //if (!Settings.DEV_IsLogsEnabled)
            //    return;

            this.WriteLogToStorage(str);
        }

        public void Error(string error)
        {
            string str = "ERROR: " + error;
#if DEBUG
            if (Debugger.IsAttached)
                Debug.WriteLine(str);
#endif
            this.AddLogItem(str);

            //if (!Settings.DEV_IsLogsEnabled)
            //    return;

            this.WriteLogToStorage(str);
        }

        /// <summary>
        /// Добавляем ошибочную запись к текущей сессии
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        /// <param name="code">Код ошибки</param>
        public void Error(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            string str = "ERROR: " + error + Environment.NewLine + exceptionData;
#if DEBUG
            if (Debugger.IsAttached)
                Debug.WriteLine(str);
#endif
            this.AddLogItem(str);

            //if (!Settings.DEV_IsLogsEnabled)
            //    return;
            
            this.WriteLogToStorage(str);
        }
        /*
        public void ErrorAndSaveToIso(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            string str = "ERROR: " + error + Environment.NewLine + exceptionData;
            //
#if DEBUG
            if(Debugger.IsAttached)
                Debug.WriteLine(str);
#endif
            //
            this.AddLogItem(str);
            this.WriteLogToStorage(str);
        }
        */
        private string GetExceptionData(Exception e)
        {
            string str = "e.Message = " + e.Message + Environment.NewLine + "e.Stack = " + e.StackTrace;
            if (e.InnerException != null)
                return str + Environment.NewLine + this.GetExceptionData(e.InnerException);
            return str;
        }

        private void WriteLogToStorage(string logMsg)
        {
            try
            {
                logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logMsg;
                lock (this.lockObj)
                {
                    using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        Stream stream;
                        //string[] rows = null;
                        //int numToSkip = 0;

                        if (!storeForApplication.FileExists(Logger.LOGNAME))
                        {
                            stream = storeForApplication.CreateFile(Logger.LOGNAME);
                        }
                        else
                        {
                            stream = storeForApplication.OpenFile(Logger.LOGNAME, FileMode.OpenOrCreate);
                            stream.Seek(0, SeekOrigin.End);

                            /*
                            //
                            //
                            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII,true, 1024,true))
                            {
                                rows = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                numToSkip = rows.Length - 30;
                                if (numToSkip < 0)
                                    numToSkip = 0;
                            }*/
                        }

                        stream.Seek(0, SeekOrigin.End);
                        //stream.Seek(0, SeekOrigin.Begin);
                        //stream.Flush();



                        using (StreamWriter streamWriter = new StreamWriter(stream))
                        {
                            /*
                            if (rows != null)
                            {
                                foreach (var row in rows)
                                {
                                    if(numToSkip>0)
                                    {
                                        numToSkip--;
                                        continue;
                                    }
                                    streamWriter.WriteLine(row);
                                }
                            }

    */
                            streamWriter.WriteLine(logMsg);
                            streamWriter.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public string ReadLogFromStorage()
        {
            try
            {
                lock (this.lockObj)
                {
                    using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        Stream stream;
                        if (!storeForApplication.FileExists(Logger.LOGNAME))
                        {
                            return "!FileExists";
                        }
                        else
                        {
                            stream = storeForApplication.OpenFile(Logger.LOGNAME, FileMode.Open);
                            stream.Seek(0, SeekOrigin.Begin);
                        }

                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            string str = streamReader.ReadToEnd();
                            return str;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                int i = 0;
            }

            return "";
        }

        /// <summary>
        /// Удалить журнал
        /// </summary>
        public void DeleteLogFromIsolatedStorage()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storeForApplication.FileExists(Logger.LOGNAME))
                    return;
                storeForApplication.DeleteFile(Logger.LOGNAME);
            }
        }

        public bool IsLogFileExists
        {
            get
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    return storeForApplication.FileExists(Logger.LOGNAME);
                }
            }
        }
    }
}
