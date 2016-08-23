using System;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fonlow.Diagnostics
{
    class LoggingConnection : IDisposable
    {
        HubConnection hubConnection;

        public HubConnection Connection
        {
            get
            {
                lockConnection.EnterReadLock();
                try
                {
                    return hubConnection;
                }
                finally
                {
                    lockConnection.ExitReadLock();
                }
            }

            set
            {
                lockConnection.EnterWriteLock();
                try
                {
                    hubConnection = value;
                }
                finally
                {
                    lockConnection.ExitWriteLock();
                }
            }
        }

        HubInfo hubInfo;

        const string sourceName = "loggingHub";

        bool Execute(HubInfo hubInfo)
        {
            this.hubInfo = hubInfo;
            if (String.IsNullOrEmpty(hubInfo.Url))
            {
                throw new AbortException("The config file does not define app setting loggingHub which should be the URL that this program should listen to. Please input one and press Enter, or just press Enter to exit.");
            }

            isAnonymous = String.Equals(hubInfo.User, "anonymous", StringComparison.CurrentCultureIgnoreCase);

            CreateHubConnection();

            var ok = DoFunctionRepeatedly(20, ConnectHub);
            return ok;
        }

        bool executed = false;

        bool IsExecuted()
        {
            lock (lockObj)
            {
                if (!executed)
                {
                    executed = true;
                    return false;
                }

                return executed;//True always now
            }
        }

        object lockObj = new object();

        ReaderWriterLockSlim lockConnection = new ReaderWriterLockSlim();

        void ExecuteOnce(HubInfo hubInfo)
        {
            if (!IsExecuted())
            {
                Execute(hubInfo);
            }
        }

        public void ExecuteOnceAsync(HubInfo hubInfo)
        {
            Action<HubInfo> d = ExecuteOnce;
            d.BeginInvoke(hubInfo, null, null);
        }

        IHubProxy loggingHubProxy;

        void CreateHubConnection()
        {
            Connection = new HubConnection(hubInfo.Url);
//#if DEBUG
//            hubConnection.TraceLevel = TraceLevels.All;
//            hubConnection.TraceWriter = Console.Out;
//#endif
            HubConnectionSubscribeEvents();

            loggingHubProxy = Connection.CreateHubProxy(sourceName);
        }

        bool ConnectHub()
        {
            try
            {
#if DEBUG
                Console.WriteLine($"HubConnection to {hubInfo.Url} starting ...");
#endif
                if (!isAnonymous)
                {
                    var tokenModel = GetBearerToken();
                    if (tokenModel == null)
                    {
#if DEBUG
                        Console.WriteLine("Auth failed");
#endif
                        return false;
                    }
                    hubConnection.Headers.Add("Authorization", $"{tokenModel.TokenType} {tokenModel.AccessToken}");
                }

                hubConnection.Start().Wait();
#if DEBUG
                Console.WriteLine("HubConnection state: " + hubConnection.State);
#endif
                return hubConnection.State == ConnectionState.Connected;
            }
            catch (AggregateException ex)
            {
                ex.Handle((innerException) =>
                {
                    Debug.Assert(innerException != null);
                    var exceptionName = innerException.GetType().Name;
#if DEBUG
                    Console.WriteLine(exceptionName + ": " + innerException.Message);
#endif
                    if (innerException.InnerException != null)
                    {
                        exceptionName = innerException.InnerException.GetType().Name;
#if DEBUG
                        Console.WriteLine(innerException.InnerException.Message);
#endif
                    }

                    return (innerException is System.Net.Http.HttpRequestException)//Likely the server is unavailable
                    || (innerException is System.Net.Sockets.SocketException)//Likely something wrong with the server
                    || (innerException is Microsoft.AspNet.SignalR.Client.HttpClientException)//likely auth error
                    || (innerException is Newtonsoft.Json.JsonReaderException)
                    ;

                });

                return false;
            }
            catch (FormatException ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                //DisposeAndReconnect(); For some reasons, disposing or stopping the connection will result in NullReferenceException inside Microsoft.AspNet.SignalR.Client.Connection.Stop(TimeSpan timeout)
                return false;
            }
#if DEBUG
            catch (Exception ex)
            {
                Console.WriteLine("Some new exception: " + ex);
                throw;

            }
#endif

        }

        void DisposeConnection()
        {
            Debug.Assert(hubConnection != null);
            hubConnection.Dispose();

            HubConnectionUnubscribeEvents();

            hubConnection = null;
        }

        void HubConnectionSubscribeEvents()
        {
            hubConnection.Closed += HubConnection_Closed;
            hubConnection.ConnectionSlow += HubConnection_ConnectionSlow;
            hubConnection.StateChanged += HubConnection_StateChanged;
            hubConnection.Reconnected += HubConnection_Reconnected;
            hubConnection.Reconnecting += HubConnection_Reconnecting;
            hubConnection.Error += HubConnection_Error;
        }

        void HubConnectionUnubscribeEvents()
        {
            hubConnection.Closed -= HubConnection_Closed;
            hubConnection.ConnectionSlow -= HubConnection_ConnectionSlow;
            hubConnection.StateChanged -= HubConnection_StateChanged;
            hubConnection.Reconnected -= HubConnection_Reconnected;
            hubConnection.Reconnecting -= HubConnection_Reconnecting;
            hubConnection.Error -= HubConnection_Error;
        }


        private void HubConnection_Error(Exception obj)
        {
#if DEBUG
            Console.WriteLine("HubConnection_Error: " + obj.ToString());
#endif
        }

        private void HubConnection_Reconnecting()
        {
#if DEBUG
            Console.WriteLine("HubConnection_Reconnecting: Reconnecting");
#endif
        }

        private void HubConnection_Reconnected()
        {
#if DEBUG
            Console.WriteLine("HubConnection_Reconnected");
#endif
        }

        private void HubConnection_StateChanged(StateChange obj)
        {
#if DEBUG
            Console.WriteLine($"HubConnection state changed from {obj.OldState} to {obj.NewState} .");
#endif
            if ((obj.OldState == ConnectionState.Reconnecting) && (obj.NewState == ConnectionState.Disconnected))
            {
                DisposeAndReconnectAsync();
            }
        }

        void DisposeAndReconnectAsync()
        {
            DisposeConnection();
#if DEBUG
            Console.WriteLine("hubConnection disposed.");
#endif

            Action d = () => Reconnect();//Need to fire it asynchronously in another thread in order not to hold up this event handling function StateChanged, so hubConnection could be really disposed.
            d.BeginInvoke(null, null);//And Console seems to be thread safe with such asynchronous call.
        }

        void Reconnect()
        {
#if DEBUG
            Console.WriteLine("Now need to create a new HubConnection object");
#endif
            CreateHubConnection();
            var ok = DoFunctionRepeatedly(20, ConnectHub);
            if (!ok)
            {
                throw new AbortException();
            }
#if DEBUG
            Console.WriteLine("HubConnection is created and successfully started.");
#endif

        }

        private void HubConnection_ConnectionSlow()
        {
#if DEBUG
            Console.WriteLine("HubConnection_ConnectionSlow: Connection is about to timeout.");
#endif
        }

        private void HubConnection_Closed()
        {
#if DEBUG
            Console.WriteLine("HubConnection_Closed: Hub could not connect or get disconnected.");
#endif
        }


        static bool DoFunctionRepeatedly(int seconds, Func<bool> func)
        {
            while (true)
            {
                var r = func();
                if (r)
                    return true;

                try
                {
#if DEBUG
                    Console.WriteLine("........................................");
                    Console.WriteLine(String.Format("Wait before {0} {1} elapse...", seconds, seconds > 1 ? "seconds" : "second"));
#endif
                    WaitReader.ReadLine(seconds);
                }
                catch (TimeoutException)
                {
#if DEBUG
                    Console.WriteLine("TimeoutException");
#endif
                }

            }

        }

        /// <summary>
        /// Invoke. Null if not successful.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns>Null if no connection.</returns>
        public Task Invoke(string method, params object[] args)
        {
            if ((Connection == null) || (Connection.State != ConnectionState.Connected))
                return null;

            return loggingHubProxy.Invoke(method, args);
        }

        bool isAnonymous = false;

        TokenResponseModel GetBearerToken()
        {
            var tokenText = GetToken(new Uri(hubInfo.Url), hubInfo.User, hubInfo.Password);
            if (String.IsNullOrEmpty(tokenText))
                return null;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponseModel>(tokenText);
        }

        static string GetToken(Uri baseUri, string userName, string password)
        {
            //inspired by http://www.codeproject.com/Articles/823263/ASP-NET-Identity-Introduction-to-Working-with-Iden
            var pairs = new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "username", userName ),
                            new KeyValuePair<string, string> ( "password", password )
                        };
            var content = new System.Net.Http.FormUrlEncodedContent(pairs);
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = client.PostAsync(new Uri(baseUri, "Token"), content).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var text = response.Content.ReadAsStringAsync().Result;
                    return text;
                }

            }
            catch (AggregateException e)
            {
                e.Handle((innerException) =>
                {
#if DEBUG
                    Console.WriteLine(innerException.Message);
#endif
                    return true;
                });
                return null;
            }
        }

        #region IDisposable pattern
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Connection != null)
                    {
                        Connection.Dispose();
                    }

                    lockConnection.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }


    static class WaitReader
    {
        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        static WaitReader()
        {
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }

        /// <summary>
        /// Read a line and timeout in minutes.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string ReadLine(int seconds)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(new TimeSpan(0, 0, seconds));
            if (success)
                return input;
            else
                throw new TimeoutException("User did not provide input within the timelimit.");
        }
    }
}
