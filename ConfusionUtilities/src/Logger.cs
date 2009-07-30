/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
*/
using System;
using System.Threading;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace ConfusionUtilities
{
	///	<summary>
	///	
	///	
	///	You should have a .config file called the same thing as the assembly.
	///	For instance, if your assembly is Logger.exe then it is
	///	Logger.exe.config
	///	
	///	Contents should be (at least) <code>
	///	<?xml version="1.0" encoding="utf-8" ?><configuration><appSettings>
	/// <add key="LogFile" value="blah.log" />
    /// <add key="LoggerDebugOn" value="true" />
	/// </appSettings></configuration>
	/// </code>
	/// 
	///	</summary>
	public abstract class Logger
	{

		#region Static factory methods
		private static Logger instance = null;
        /// <summary>
        ///  put the processId in here or whatever
        /// 
        /// </summary>

        static string CLIENT_ID = null;

		public static void GetInstance(string classname)
		{
			if (instance == null) 
			{
				Type tType = Type.GetType(classname);
				System.Reflection.ConstructorInfo constructor = tType.GetConstructor(System.Type.EmptyTypes);
				instance = (Logger)constructor.Invoke(null);			
			}
		}
		
		/// <summary>
		/// Loads the default logger, the FileLogger
		/// </summary>
		/// <returns></returns>
		public static Logger GetInstance()
		{
			if (instance == null)
				instance = new FileLogger();
			///  here we avoid dynamic invocation, as it hides any errors that might happen on 
			///  the constructor's invocation
			return instance;
		}

        public static void Log(string text, params object[] args)
        {
            String formatted = string.Format(text, args);
            GetInstance().log(formatted);
        }

        public static void Log(string text, object arg0)
        {
            String formatted = string.Format(text, arg0);
            GetInstance().log(formatted);
        }
        
        /// <summary>
        /// The VARIOUS log methods are convenience methods, client
        /// could use GetInstance().log too
        /// </summary>
        /// <param name="text"></param>
		public static void Log(string text) 
		{
			GetInstance().log(text);
		}

        public static void Debugg(string text) {
            Log(text, EventType.Debug);        
        }

		public static void Log(string text,EventType eType) 
		{
			GetInstance().log(text, eType);
		}

        public static void Log(Exception e) {
            GetInstance().log(e);
        }


		public static void ShutDown() 
		{
			if (instance == null)
				return;
			instance.shutDown();
			instance = null;
		}

        public void shutdown(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance.shutDown();
        }

        public static void ShutDown(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GetInstance().shutDown();
        }


		public static string FileLoggerDefaultFilename 
		{
			set 
			{
				if (value != null && value.Length > 0)
					FileLogger.BACKUP_FILENAME = value;
			}
		}
		
		#endregion 

		protected StringBuilder logBuffer = new StringBuilder();
		protected Thread t = null;
		static int LOGGING_EVERY = 5000;
        public static string KEY_FOR_DEBUG = "LoggerDebugOn";
        static protected double VERSION = 1.05;

		protected Logger() 
		{
		}

        protected virtual void init(bool log) {
            this.logging = log;

            try
            {
                Debugging = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings[KEY_FOR_DEBUG]);
            }
            catch (Exception) {
                Debugging = false;
            }
            
            if (t != null)
                throw new Exception("Log has already been initialized. Call Shutdown first.");
            t = new Thread(new ThreadStart(SleepWrite));
            t.Name = "Logger Runaway Thread Started " + Util.GetTimestamp();
            t.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="clientId">a special client identifier, like the process id
        /// using  System.Diagnostics.Process.GetCurrentProcess().Id</param>
        public static void Init(bool log, string clientId)
        {
            CLIENT_ID = clientId;
            GetInstance().init(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log">To Log or Not To Log</param>
        public static void Init(bool log) {
            GetInstance().init(log);
        }



        public static bool IsInitialized { 
            get {
                return (GetInstance().t != null);
            }
            
        }

		public enum	EventType
		{
			Information,
			Error,
			Warning,
            Debug
		}


		protected void SleepWrite()
		{
			Debug.Assert(t != null);
			bool terminate = false;
			while (terminate == false)
			{
                try
                {
                    // sleep for 1 second
                    Thread.Sleep(LOGGING_EVERY);
                    Write();
                }
                catch (ThreadInterruptedException)
                {
                    terminate = true;
                    try
                    {
                        t = null; // clear out the reference to the thread
                        Write(); // flush all the end stuff out
                    }
                    catch (Exception e) {
                        Debug.WriteLine(e.Message);
                        Trace.WriteLine(e.Message);
                    }
                }
                catch (Exception e) {
                    log(e); // we have no choice, really
                    Debug.WriteLine(e.Message);
                    Trace.WriteLine(e.Message);
                }
			}
            Debug.WriteLine("letting thread finish " + Thread.CurrentThread.Name);
		}

		protected abstract void Write();

		public void log(string text)
		{
            if (!Logging)
                return;
            log(text, EventType.Information);
		}

        public void log(Exception ex)
        {
            if (!Logging || ex == null)
                return;
            log("EXCEPTION: " + ex.Message, EventType.Error);
            log(ex.Message + "(" + ex.GetType().Name + ")", EventType.Error);
            log(ex.StackTrace, EventType.Error);
            log(ex.InnerException);
        }


		/// <summary>
		/// Logs info, errors, etc.
		/// Access to this method is serialized (synchronized) so as
		/// 1) to coordinate with the Write method and
		/// 2) because it does its writing in multiple lines of code, though
		/// this could be avoided
		/// </summary>
		/// <param name="text"></param>
		/// <param name="eType"></param>
		protected void log(string text,EventType eType)
		{
            if (!Logging)
                return;
            if (eType == EventType.Debug && !Debugging)
                return;
            if (t == null)
                throw new Exception("Log it not initialized. Either Shutdown was already called, Init was never called, OR INIT DID NOT SUCEED.");
            lock (logBuffer)
            {
                if (CLIENT_ID != null)
                    logBuffer.Append("[" + Util.GetTimestamp() + " " + eType + " Pid " + CLIENT_ID + "] - ");
                else
                    logBuffer.Append("[" + Util.GetTimestamp() + " " + eType + "] - ");
                
                logBuffer.Append(text + "\n");
            }
		}


		void shutDown() 
		{
            if (!IsInitialized)
                return;
			Debug.WriteLine("DEBUG: Shutting down, last info " + GetInfo());
			instance.t.Interrupt(); // pretty sure this should be this, but no time to change and test
			
		}

		public static string GetInfo() {
			if (instance != null)
				return "[LoggerInfo] " + instance.getInfo();
			else
				return "[LoggerInfo] " + "Logger has not been started.";
		}

		public virtual string getInfo() 
		{
			if (t != null && t.IsAlive) 
			{
				return "Logging every " + LOGGING_EVERY + " milliseconds with " + t.Name;
			} else
				return "Not logging, not intialized, or something is wrong.";
		}

        bool logging = true;
        public bool Logging {
            get {
                return logging;
            }
            internal set {
                this.logging = value;
            }
        }

        bool debugging = true;
        public bool Debugging
        {
            get
            {
                return debugging;
            }
            set
            {
                this.debugging = value;
            }
        }


        public static string Tail(int maxLines) {
            return GetInstance().tail(maxLines);
        }
        internal abstract string tail(int maxLines);
        public static void Wipe()
        {
            GetInstance().wipe();
        }
        internal abstract void wipe();

        /// <summary>
        /// returns a string like 20h15m35s or 20h15m35s35ms using 
        /// the current system clock as T2
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Since(DateTime timeOne, bool milliseconds)
        {
            TimeSpan since = DateTime.Now - timeOne;
            return Since(since, milliseconds);
        }

        /// <summary>
        /// returns a string like 20h15m35s or 20h15m35s35ms
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Since(TimeSpan s, bool milliseconds)
        {
            string retVal = (s.Days * 60 + s.Hours) + "h" + s.Minutes + "m" + s.Seconds + "s";
            if (milliseconds)
                retVal += s.Milliseconds + "ms";
            return retVal;
        }

        public static void LogTimer(DateTime one, string description) {
            LogTimer(one, description, 0);
        }


        public static void LogTimer(DateTime one, string description, int greaterThanMilliseconds)
        {
            DateTime now = DateTime.Now; // do this first for correctness
            TimeSpan compareMe = now - one;
            
            if (compareMe.CompareTo(TimeSpan.FromMilliseconds(greaterThanMilliseconds)) > -1)
            {
                string since = Since(now - one, true);
                Log(description + ": " + since);
            }
        }


	}


}
