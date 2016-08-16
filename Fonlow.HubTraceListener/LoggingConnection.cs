﻿using System;
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
#if DEBUG
            hubConnection.TraceLevel = TraceLevels.All;
            hubConnection.TraceWriter = Console.Out;
#endif
            HubConnectionSubscribeEvents();

            loggingHubProxy = Connection.CreateHubProxy(sourceName);
        }

        bool ConnectHub()
        {
            try
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

                Connection.Start().Wait();
                return true;
            }
            catch (AggregateException ex)
            {
                ex.Handle((innerException) =>
                {
                    if (innerException.InnerException != null)
                    {
                    }
                    return true;
                });

                if (ex.InnerException is System.Net.Http.HttpRequestException)//Likely the server is unavailable
                {
                    return false;
                }
                if (ex.InnerException is System.Net.Sockets.SocketException)//Likely something wrong with the server
                {
                    return false;
                }

                throw;
            }

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
        }

        private void HubConnection_Reconnecting()
        {
        }

        private void HubConnection_Reconnected()
        {
        }

        private void HubConnection_StateChanged(StateChange obj)
        {
            if ((obj.OldState == ConnectionState.Reconnecting) && (obj.NewState == ConnectionState.Disconnected))
            {
                DisposeConnection();

                Action d = () => Reconnect();//Need to fire it asynchronously in another thread in order not to hold up this event handling function StateChanged, so hubConnection could be really disposed.
                d.BeginInvoke(null, null);//And Console seems to be thread safe with such asynchronous call.
            }
        }

        void Reconnect()
        {
            CreateHubConnection();
            var ok = DoFunctionRepeatedly(20, ConnectHub);
            if (!ok)
            {
                throw new AbortException();
            }

        }

        private void HubConnection_ConnectionSlow()
        {
        }

        private void HubConnection_Closed()
        {
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
                    var input = WaitReader.ReadLine(seconds);
                    if (String.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
                        return false;

                    if (String.IsNullOrEmpty(input))
                        continue;

                }
                catch (TimeoutException)
                {
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


        TokenResponseModel GetBearerToken()
        {
            var tokenText = GetToken(new Uri(hubInfo.Url), hubInfo.User, hubInfo.Password);
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
                    Trace.TraceWarning(innerException.Message);
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
