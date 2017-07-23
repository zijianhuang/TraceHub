using System;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Threading;
using Fonlow.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fonlow.Logging
{
    class LoggingConnection : IDisposable
    {
        HubConnection hubConnection;

        /// <summary>
        /// For reproducing trace messages from the server
        /// </summary>
        TraceSource loggingSource;

        public string Url { get; private set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        bool isAnonymous = false;

        const string sourceName = "loggingHub";
        const string hubPasswordField = sourceName + "_Password";
        const string hubUsernameField = sourceName + "_Username";

        public async Task<bool> Execute()
        {
            loggingSource = new TraceSource(sourceName); //However, the source value won't be "loggingHub" but the source value from the original traces emitted by the source applications.
            Url = System.Configuration.ConfigurationManager.AppSettings[sourceName];
            Password = System.Configuration.ConfigurationManager.AppSettings[hubPasswordField];
            UserName = System.Configuration.ConfigurationManager.AppSettings[hubUsernameField];

            if (String.IsNullOrEmpty(Url))
            {
                Console.WriteLine("The config file does not define app setting loggingHub which should be the URL that this program should listen to. Please input one and press Enter, or just press Enter to exit.");

                Console.Write("Please enter URL: ");
                Url = Console.ReadLine();
                if (String.IsNullOrEmpty(Url))
                {
                    throw new AbortException("Not to define URL, so exit the program.");
                }
            }

            isAnonymous = String.Equals(UserName, "anonymous", StringComparison.CurrentCultureIgnoreCase);

            if (!isAnonymous)
            {
                if (String.IsNullOrEmpty(UserName))
                {
                    Console.Write("Username: ");
                    UserName = Console.ReadLine();
                    if (String.IsNullOrEmpty(UserName))
                    {
                        throw new AbortException("Not to define username, so exit the program.");
                    }
                }

                if (String.IsNullOrEmpty(Password))
                {
                    Console.Write("Password: ");
                    Password = Console.ReadLine();
                    if (String.IsNullOrEmpty(Password))
                    {
                        throw new AbortException("Not to define password, so exit the program.");
                    }
                }
            }

            var ok = await DoFunctionRepeatedly(20, ConnectHub);
            return ok;
        }
        IHubProxy loggingHubProxy;

        IDictionary<string, string> queryString = new Dictionary<string, string>();

        IDisposable writeMessageHandler;
        IDisposable writeMessagesHandler;
        IDisposable writeTraceHandler;
        IDisposable writeTracesHandler;

        void CreateHubConnection()
        {
            hubConnection = new HubConnection(Url, queryString);
            //#if DEBUG
            //            hubConnection.TraceLevel = TraceLevels.All;
            //            hubConnection.TraceWriter = Console.Out;
            //#endif
            Debug.WriteLine($"HubConnection created for {Url}.");

            HubConnectionSubscribeEvents();

            HubConnectionProxySubscribeServerEvents();

            Debug.WriteLine("HubProxy created.");
        }

        void HubConnectionProxySubscribeServerEvents()
        {
            loggingHubProxy = hubConnection.CreateHubProxy(sourceName);

            writeMessageHandler = loggingHubProxy.On<string>("WriteMessage", s => Console.WriteLine(s));

            writeMessagesHandler = loggingHubProxy.On<string[]>("WriteMessages", ss =>
            {
                foreach (var s in ss)
                {
                    Console.WriteLine(s);
                }
            });


            writeTraceHandler = loggingHubProxy.On<TraceMessage>("WriteTrace", tm => ReproduceTrace(tm));

            writeTracesHandler = loggingHubProxy.On<TraceMessage[]>("WriteTraces", tms =>
            {
                foreach (var tm in tms)
                {
                    ReproduceTrace(tm);
                }
            });
        }

        string FormatTraceMessage(TraceMessage tm)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append(tm.TimeUtc.ToString("yy-MM-ddTHH:mm:ss.fffZ") + "  ");//To use the timestamp sent from the source.

            if (!String.IsNullOrEmpty(tm.Origin))
            {
                builder.Append("[" + tm.Origin + "]: ");
            }

            builder.Append(tm.Message);
            return builder.ToString();
        }

        void ReproduceTrace(TraceMessage tm)
        {
            if (loggingSource.Listeners.Count == 0)
                return;

            foreach (var listener in loggingSource.Listeners.OfType<TraceListener>())
            {
                listener.TraceEvent(new TraceEventCache(), tm.Source, tm.EventType, tm.Id, FormatTraceMessage(tm));
            }
        }

        void DisposeConnection()
        {
            Debug.Assert(hubConnection != null);
            HubConnectionUnubscribeEvents();

            writeMessageHandler.Dispose();
            writeMessagesHandler.Dispose();
            writeTraceHandler.Dispose();
            writeTracesHandler.Dispose();

            Trace.TraceInformation("Stop Hubconnection... If you see Trace Error about WebSocketException, NOT to worry, since this is written by the SignalR client lib.");
            hubConnection.Stop();
            Trace.TraceInformation("Hubconnection stopped. If you see Trace Error about WebSocketException above, NOT to worry, since this is written by the SignalR client lib.");

            Debug.WriteLine("Now hubConnection.Dispose");
            hubConnection.Dispose();
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

        async Task<bool> ConnectHub()
        {
            try
            {
                Debug.WriteLine("HubConnection starting ...");

                if (!isAnonymous)
                {
                    var tokenModel = GetBearerToken();
                    if (tokenModel == null)
                    {
                        Trace.TraceWarning("Auth failed");
                        return false;
                    }
//                    hubConnection.Headers.Add("Authorization", $"{tokenModel.TokenType} {tokenModel.AccessToken}");
                    queryString["token"] = tokenModel.AccessToken;
                }

                CreateHubConnection();

                await hubConnection.Start();
                Debug.WriteLine("HubConnection state: " + hubConnection.State);
                await Invoke("ReportClientType", ClientType.Console);
                return hubConnection.State == ConnectionState.Connected;
            }
            catch (Exception ex) when ((ex is System.Net.Http.HttpRequestException)//Likely the server is unavailable
                    || (ex is System.Net.Sockets.SocketException)//Likely something wrong with the server
                    || (ex is System.Net.WebSockets.WebSocketException)
                    || (ex is Microsoft.AspNet.SignalR.Client.HttpClientException)//likely auth error
                    || (ex is Newtonsoft.Json.JsonReaderException))
            {
                bool withCriticalEndpointProblem = false;

                Debug.Assert(ex != null);
                var exceptionName = ex.GetType().Name;
                Trace.TraceWarning(exceptionName + ": " + ex.Message);
                if (ex.InnerException != null)
                {
                    exceptionName = ex.InnerException.GetType().Name;
                    Trace.TraceWarning(ex.InnerException.Message);
                }

                var httpClientException = ex as HttpClientException;
                if (httpClientException != null)
                {
                    if (httpClientException.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Trace.TraceError("Even though the host exists however, the url for auth or signalR does not exist. The program will abort.");
                        withCriticalEndpointProblem = true;
                        return true;
                    }
                }

                if (withCriticalEndpointProblem)
                {
                    throw new AbortException("Abort.");
                }

                return false;
            }
            catch (FormatException ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }
            catch (HttpClientException ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Some new exception: " + ex);
                throw;

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
            if ((hubConnection == null) || (hubConnection.State != ConnectionState.Connected))
                return null;

            return loggingHubProxy.Invoke(method, args);
        }


        private void HubConnection_Error(Exception obj)
        {
            Trace.TraceWarning("Some error: " + obj);
        }

        private void HubConnection_Reconnecting()
        {
            //the SignalR client lib may try to reconnect up to 7 times, then disconnected, no more retry.
            Trace.TraceInformation($"Reconnecting {Url}...");
        }

        private void HubConnection_Reconnected()
        {
            Trace.TraceInformation($"{Url} reconnected.");
            Invoke("ReportClientType", ClientType.Console);
        }

        private void HubConnection_StateChanged(StateChange obj)
        {
            Trace.TraceInformation($"HubConnection state changed from {obj.OldState} to {obj.NewState} .");

            if  (obj.NewState == ConnectionState.Disconnected)
            {
                DisposeAndReconnectAsync();
            }
        }

        void DisposeAndReconnectAsync()
        {
            DisposeConnection();
            Debug.WriteLine("hubConnection disposed.");

            Reconnect().ContinueWith(t =>
            {
                Trace.TraceInformation("Reconnect() done.");
            });

            Trace.TraceInformation("Reconnect() called.");
        }

        async Task Reconnect()
        {
            Debug.WriteLine("Now need to create a new HubConnection object");
            var ok = await DoFunctionRepeatedly(20, ConnectHub);
            if (!ok)
            {
                throw new AbortException();
            }

            Debug.WriteLine("HubConnection is created and successfully started.");
        }

        private void HubConnection_ConnectionSlow()
        {
            Trace.TraceWarning("HubConnection_ConnectionSlow: Connection is about to timeout.");
        }

        private void HubConnection_Closed()
        {
            //SignalR client lib won't retry after closed.
            Trace.TraceWarning("HubConnection_Closed: Hub could not connect or get disconnected.");
        }


        static async Task<bool> DoFunctionRepeatedly(int seconds, Func<Task<bool>> func)
        {
            while (true)
            {
                var r = await func();
                if (r)
                    return true;

                try
                {
                    Console.WriteLine("........................................");
                    Console.WriteLine(String.Format("Input Q and Enter to quit, or Enter twice to execute again before {0} {1} elapse...", seconds, seconds > 1 ? "seconds" : "second"));
                    var input = WaitReader.ReadLine(seconds);
                    if (String.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
                        return false;

                    Trace.TraceWarning("Silly input with {0}. You should had inputted Q or Enter. Now continue to execute.", input);
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("TimeoutException");
                }

            }

        }


        TokenResponseModel GetBearerToken()
        {
            var tokenText = GetToken(new Uri(Url), UserName, Password);
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
                    Console.WriteLine($"Connecting {baseUri}...");
                    var response = client.PostAsync(new Uri(baseUri, "Token"), content).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Cannot get token for {0}:{1} with Uri {2}, with status code {3} and message {4}", userName, password, baseUri, response.StatusCode, response.ReasonPhrase);
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
                    if (hubConnection != null)
                    {
                        DisposeConnection();
                    }
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
