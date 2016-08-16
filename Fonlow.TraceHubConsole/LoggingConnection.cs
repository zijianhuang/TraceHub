using System;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Threading;
using Fonlow.Diagnostics;
using System.Collections.Generic;

namespace Fonlow.Logging
{
    class LoggingConnection : IDisposable
    {
        HubConnection hubConnection;

        public HubConnection Connection
        {
            get { return hubConnection; }
        }

        public string Url { get; private set; }

        /// <summary>
        /// For reproducing trace messages from the server
        /// </summary>
        TraceSource loggingSource;

        const string sourceName = "loggingHub";

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool Execute()
        {
            loggingSource = new TraceSource(sourceName);
            Url = System.Configuration.ConfigurationManager.AppSettings[sourceName];

            if (String.IsNullOrEmpty(Url))
            {
                Console.WriteLine("The config file does not define app setting loggingHub which should be the URL that this program should listen to. Please input one and press Enter, or just press Enter to exit.");

                Console.Write("Please enter URL: ");
                Url = Console.ReadLine();
                if (String.IsNullOrEmpty(Url))
                {
                    throw new AbortException();
                }
            }


            CreateHubConnection();

            var ok = DoFunctionRepeatedly(20, ConnectHub);
            return ok;
        }

        IDisposable writeMessageHandler;
        IDisposable writeMessagesHandler;
        IDisposable writeTraceHandler;
        IDisposable writeTracesHandler;

        void CreateHubConnection()
        {
            hubConnection = new HubConnection(Url);
#if DEBUG
            hubConnection.TraceLevel = TraceLevels.All;
            hubConnection.TraceWriter = Console.Out;
#endif
            Debug.WriteLine($"HubConnection created for {Url}.");
            
            HubConnectionSubscribeEvents();

            IHubProxy loggingHubProxy = hubConnection.CreateHubProxy(sourceName);
            
            writeMessageHandler=   loggingHubProxy.On<string>("WriteMessage", s => Console.WriteLine(s));

            writeMessagesHandler= loggingHubProxy.On<string[]>("WriteMessages", ss =>
            {
                foreach (var s in ss)
                {
                    Console.WriteLine(s);
                }
            });

            Func<TraceMessage, string> formatTraceMessage = (tm) =>
            {
                StringBuilder builder = new StringBuilder();
                if (!String.IsNullOrEmpty(tm.Origin))
                {
                    builder.Append("ORIGIN: " + tm.Origin + "  ");
                }

                if (tm.TimeUtc != null)
                {
                    builder.Append(tm.TimeUtc.Value.ToString("yy-MM-ddTHH:mm:ss:fffZ") + "  ");//To use the timestamp sent from the source.
                }

                builder.Append(tm.Message);
                return builder.ToString();
            };

            writeTraceHandler=  loggingHubProxy.On<TraceMessage>("WriteTrace", tm => loggingSource.TraceEvent(tm.EventType, 0, formatTraceMessage(tm)));

            writeTracesHandler= loggingHubProxy.On<TraceMessage[]>("WriteTraces", tms =>
            {
                foreach (var tm in tms)
                {
                    loggingSource.TraceEvent(tm.EventType, 0, formatTraceMessage(tm));
                }
            });

            Debug.WriteLine("HubProxy created.");
        }

        void DisposeConnection()
        {
            Debug.Assert(hubConnection != null);
            hubConnection.Stop(TimeSpan.FromSeconds(2));
            hubConnection.Dispose();
            HubConnectionUnubscribeEvents();
            writeMessageHandler.Dispose();
            writeMessagesHandler.Dispose();
            writeTraceHandler.Dispose();
            writeTracesHandler.Dispose();
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

        bool ConnectHub()
        {
            try
            {
                Debug.WriteLine("HubConnection starting ...");
                var tokenModel = GetBearerToken();
                if (tokenModel == null)
                {
                    Console.WriteLine("Auth failed");
                    return false;
                }
                hubConnection.Headers.Add("Authorization", $"{tokenModel.TokenType} {tokenModel.AccessToken}");

                hubConnection.Start().Wait();
                Debug.WriteLine("HubConnection state: "+hubConnection.State);
                return hubConnection.State == ConnectionState.Connected;
            }
            catch (AggregateException ex)
            {
                ex.Handle((innerException) =>
                {
                    Debug.Assert(innerException != null);
                    var exceptionName = innerException.GetType().Name;
                    Trace.TraceWarning(exceptionName + ": "+innerException.Message);
                    if (innerException.InnerException != null)
                    {
                        exceptionName = innerException.InnerException.GetType().Name;
                        Trace.TraceWarning(innerException.InnerException.Message);
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
                Trace.TraceError(ex.ToString());
                //DisposeAndReconnect(); For some reasons, disposing or stopping the connection will result in NullReferenceException inside Microsoft.AspNet.SignalR.Client.Connection.Stop(TimeSpan timeout)
                return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Some new exception: " + ex);
                throw;

            }

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
        }

        private void HubConnection_StateChanged(StateChange obj)
        {
            Trace.TraceInformation($"HubConnection state changed from {obj.OldState} to {obj.NewState} .");

            if ((obj.OldState == ConnectionState.Reconnecting) && (obj.NewState == ConnectionState.Disconnected))
            {
                DisposeAndReconnect();
            }
        }

        void DisposeAndReconnect()
        {
            DisposeConnection();
            Debug.WriteLine("hubConnection disposed.");

            Action d = () => Reconnect();//Need to fire it asynchronously in another thread in order not to hold up this event handling function StateChanged, so hubConnection could be really disposed.
            d.BeginInvoke(null, null);//And Console seems to be thread safe with such asynchronous call.
        }

        void Reconnect()
        {
            Debug.Write("Now need to create a new HubConnection object");
            CreateHubConnection();
            var ok = DoFunctionRepeatedly(20, ConnectHub);
            if (!ok)
            {
                throw new AbortException();
            }

            Debug.WriteLine("HubConnection is created and successfully started.");
        }

        private void HubConnection_ConnectionSlow()
        {
            Trace.TraceWarning("Connection is about to timeout.");
        }

        private void HubConnection_Closed()
        {
            //SignalR client lib won't retry after closed.
            Trace.TraceWarning("Hub could not connect or get disconnected.");
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
                    Console.WriteLine("........................................");
                    Console.WriteLine(String.Format("Input Q and Enter to quit, or Enter twice to execute again before {0} {1} elapse...", seconds, seconds > 1 ? "seconds" : "second"));
                    var input = WaitReader.ReadLine(seconds);
                    if (String.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
                        return false;

                    if (String.IsNullOrEmpty(input))
                        continue;

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
            Password = System.Configuration.ConfigurationManager.AppSettings["loggingHub_Password"];
            UserName = System.Configuration.ConfigurationManager.AppSettings["loggingHub_Username"];
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
                        hubConnection.Dispose();
                        Debug.WriteLine("hubConnection disposed at the end.");
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
