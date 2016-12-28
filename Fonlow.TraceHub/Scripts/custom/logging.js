///<reference path="../typings/jquery/jquery.d.ts" />
///<reference path="../typings/signalr/signalr.d.ts" />
var Fonlow_Logging;
(function (Fonlow_Logging) {
    (function (ClientType) {
        ClientType[ClientType["Undefined"] = 0] = "Undefined";
        ClientType[ClientType["TraceListener"] = 1] = "TraceListener";
        ClientType[ClientType["Browser"] = 2] = "Browser";
        ClientType[ClientType["Console"] = 4] = "Console";
    })(Fonlow_Logging.ClientType || (Fonlow_Logging.ClientType = {}));
    var ClientType = Fonlow_Logging.ClientType;
    /**
     * Manage SignalR connection
     */
    var LoggingHubStarter = (function () {
        function LoggingHubStarter() {
            this.listeningStoped = true;
            console.debug('LoggingHubStarter created.');
        }
        LoggingHubStarter.prototype.reconnect = function () {
            this.init();
            this.start();
        };
        /**
         * If the connection is not stopped intentionally, will reconnect later.
         * @param ms milliseconds to wait.
         */
        LoggingHubStarter.prototype.reconnectWithDelay = function (ms) {
            var _this = this;
            if (this.listeningStoped)
                return;
            setTimeout(function () {
                _this.reconnect();
            }, ms);
        };
        /**
         * This should be placed before logout.
         */
        LoggingHubStarter.prototype.stopListening = function () {
            console.debug('ready to stopListening');
            this.listeningStoped = true;
            try {
                this.connection.stop(false, true);
            }
            catch (ex) {
                console.error(ex);
            }
            console.debug('Stopped listening signalR.');
        };
        LoggingHubStarter.prototype.init = function () {
            this.connection = $.hubConnection(); //get the hub connection object from SignalR jQuery lib.
            if (!this.connection) {
                console.error('Cannot obtain $.hubconnection.');
                return false;
            }
            this.proxy = this.connection.createHubProxy('loggingHub'); //connection.hub class is a derived class of connection
            this.wrapServerFunctions();
            this.subscribeServerPusheEvents();
            this.hubConnectionSubscribeEvents();
            return true;
        };
        /**
         * Just provide strongly typed client calls to signalR server.
         */
        LoggingHubStarter.prototype.wrapServerFunctions = function () {
            var _this = this;
            this.server = {
                uploadTrace: function (traceMessage) { return _this.invoke('uploadTrace', traceMessage); },
                uploadTraces: function (traceMessages) { return _this.invoke('uploadTraces', traceMessages); },
                getAllClients: function () { return _this.invoke('getAllClients'); },
                reportClientType: function (clientType) { return _this.invoke('reportClienttype', clientType); },
                reportClientTypeAndTraceTemplate: function (clientType, template, origin) { return _this.invoke('reportClientTypeAndTraceTemplate', clientType, template, origin); },
                retrieveClientSettings: function () { return _this.invoke('retrieveClientSettings'); },
            };
        };
        /**
         * Subscribe some server push events.
         */
        LoggingHubStarter.prototype.subscribeServerPusheEvents = function () {
            this.proxy.on('writeTrace', clientFunctions.writeTrace);
            this.proxy.on('writeTraces', clientFunctions.writeTraces);
            this.proxy.on('writeMessage', clientFunctions.writeMessage);
            this.proxy.on('writeMessages', clientFunctions.writeMessages);
        };
        /**
         * Basic house keeping of signalR connection
         * @param connection
         */
        LoggingHubStarter.prototype.hubConnectionSubscribeEvents = function () {
            var _this = this;
            this.connection
                .stateChanged(function (change) {
                console.info("HubConnection state changed from " + change.oldState + " to " + change.newState + " .");
                if (change.newState == 4 /* Disconnected */) {
                    console.warn('Now try to re-establish signalR connection by the component.');
                }
            })
                .disconnected(function () {
                console.warn('HubConnection_Closed: Hub could not connect or get disconnected.');
            })
                .reconnected(function () {
                console.info(_this.connection.url + ' reconnected.');
                _this.server.reportClientType(ClientType.Browser).fail(function () {
                    console.error('Fail to reportClientType');
                });
            })
                .reconnecting(function () {
                console.info('Reconnecting ' + _this.connection.url + ' ...');
            })
                .connectionSlow(function () {
                console.warn('HubConnection_ConnectionSlow: Connection is about to timeout.');
            })
                .error(function (error) {
                var context = error.context;
                if (context && context.status != 0) {
                    if (context.status === 401) {
                        console.warn('Due to 401, the connection wont be resumed.' + context.statusText);
                        _this.stopListening();
                    }
                }
                console.error(error.message);
            });
        };
        LoggingHubStarter.prototype.invoke = function (method) {
            var msg = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                msg[_i - 1] = arguments[_i];
            }
            if (!this.connection || this.connection.state != 1) {
                console.debug("Invoking " + method + " when connection or hub state is not good.");
                return $.when(null);
            }
            return (_a = this.proxy).invoke.apply(_a, [method].concat(msg));
            var _a;
        };
        LoggingHubStarter.prototype.start = function () {
            var _this = this;
            if (!this.connection) {
                console.error('Cannot obtain $.hubconnection. so LoggingHubStarter was not really created.');
                if (!this.init()) {
                    return $.when(null);
                }
            }
            return this.connection.start({ transport: ['webSockets', 'longPolling'] })
                .done(function () {
                _this.listeningStoped = false;
                $('input#clients').click(function () {
                    _this.server.getAllClients().done(function (clientsInfo) {
                        webUiFunctions.renderClientsInfo(clientsInfo);
                    });
                });
                _this.server.reportClientType(ClientType.Browser).fail(function () {
                    console.error('Fail to reportClientType');
                });
                ;
                _this.server.retrieveClientSettings()
                    .done(function (result) {
                    clientSettings = result;
                    $('input#clients').toggle(clientSettings.advancedMode);
                    clientFunctions.bufferSize = clientSettings.bufferSize;
                    _this.server.getAllClients().done(function (clientsInfo) {
                        if (clientsInfo == null) {
                            $('input#clients').hide();
                        }
                        else {
                            _this.server.getAllClients().done(function (clientsInfo) {
                                if (clientsInfo == null) {
                                    $('input#clients').hide();
                                }
                            });
                        }
                    });
                })
                    .fail(function () {
                    console.error("Fail to retrieveClientSettings.");
                });
            })
                .fail(function () {
                console.error('Couldnot start loggingHub connection.');
            });
        };
        return LoggingHubStarter;
    }());
    Fonlow_Logging.LoggingHubStarter = LoggingHubStarter;
    var WebUiFunctions = (function () {
        function WebUiFunctions() {
        }
        WebUiFunctions.prototype.renderClientsInfo = function (clientsInfo) {
            if (clientsInfo == null)
                return false;
            if (clientsInfo.length == 0)
                return true;
            var evenLine = false;
            var divs = clientsInfo.map(function (m) {
                var div = $('<li/>', { class: 'hubClientInfo' + (evenLine ? ' even' : ' odd') });
                evenLine = !evenLine;
                div.append($('<span/>', { class: 'hc-type' }).text(Fonlow_Logging.ClientType[m.clientType]));
                div.append($('<span/>', { class: 'hc-userAgent' }).text(m.userAgent));
                div.append($('<span/>', { class: 'hc-ip' }).text(m.ipAddress));
                div.append($('<span/>', { class: 'time' }).text(m.connectedTimeUtc.toString()));
                if (m.clientType == Fonlow_Logging.ClientType.TraceListener) {
                    div.append($('<span/>', { class: 'hc-template' }).text(m.template));
                    div.append($('<span/>', { class: 'origin' }).text(m.origin));
                }
                div.append($('<span/>', { class: 'hc-user' }).text(m.username));
                div.append($('<span/>', { class: 'hc-id' }).text(m.id));
                return div;
            });
            var list = $('<div/>', { class: 'hubClients' });
            list.append(divs);
            $('#clientList').empty();
            $('#clientList').append(list);
            return true;
        };
        return WebUiFunctions;
    }());
    Fonlow_Logging.WebUiFunctions = WebUiFunctions;
    var ClientFunctions = (function () {
        function ClientFunctions() {
            var _this = this;
            this.bufferSize = 10000; //this will be altered by Web.config through a server call retrieveClientSettings once the signalR connection is established.
            this.stayWithLatest = true;
            this.sourceLevels = -1; //all
            this.writeTrace = function (tm) {
                if ((tm.eventType & _this.sourceLevels) == 0)
                    return;
                _this.addLine(tm);
            };
            //Write traces in fixed size queue defined by this.bufferSize 
            this.writeTraces = function (tms) {
                if (_this.sourceLevels > 0) {
                    tms = tms.filter(function (m) { return (m.eventType & _this.sourceLevels) != 0; });
                }
                else if (_this.sourceLevels === 0) {
                    return;
                }
                //Clean up some space first
                if (lineCount + tms.length > _this.bufferSize) {
                    var numberOfLineToRemove = lineCount + tms.length - _this.bufferSize;
                    $('#traces li:nth-child(-n+' + numberOfLineToRemove + ')').remove(); //Thanks to this trick http://stackoverflow.com/questions/9443101/how-to-remove-the-n-number-of-first-or-last-elements-with-jquery-in-an-optimal, much faster than my loop
                    lineCount -= numberOfLineToRemove;
                }
                //Buffer what to add
                var itemsToAppend = $();
                $.each(tms, function (index, tm) {
                    itemsToAppend = itemsToAppend.add(_this.createNewLine(tm)); //append siblings
                    evenLine = !evenLine; //Silly, I should have used math :), but I wanted simplicity
                });
                $('#traces').append(itemsToAppend);
                lineCount += tms.length;
                _this.scrollToBottom();
            };
        }
        ClientFunctions.prototype.eventTypeToString = function (t) {
            switch (t) {
                case 1:
                    return "Critical";
                case 2:
                    return "Error";
                case 4:
                    return "Warning";
                case 8:
                    return "Info";
                case 16:
                    return "Verbose";
                case 256:
                    return "Start";
                case 512:
                    return "Stop";
                case 1024:
                    return "Suspend";
                case 2048:
                    return "Resume";
                case 4096:
                    return "Transfer";
                default:
                    return "Misc ";
            }
        };
        ClientFunctions.prototype.createNewLine = function (tm) {
            var et = this.eventTypeToString(tm.eventType);
            var $eventText = $('<span/>', { class: et + ' et' }).text(et + ': ');
            var $timeText = $('<span/>', { class: 'time', value: tm.timeUtc }).text(' ' + this.getShortTimeText(new Date(tm.timeUtc.toString())) + ' '); //The Json object seem to become string rather than Date. A bug in SignalR JS? Now I have to cast it 
            var $originText = $('<span/>', { class: 'origin btn-xs btn-primary', onclick: 'void(0)' }).text(' ' + tm.origin + '  ');
            var $messageText = $('<span/>', { class: 'message' }).text(tm.message);
            var newLine = $('<li/>', { class: evenLine ? 'even' : 'odd' });
            newLine.append($eventText);
            newLine.append($timeText);
            newLine.append($originText);
            newLine.append($messageText);
            return newLine;
        };
        ClientFunctions.prototype.addLine = function (tm) {
            //Clean up some space
            if (lineCount + 1 > this.bufferSize) {
                $('#traces li:first').remove();
                lineCount--;
            }
            var newLine = this.createNewLine(tm);
            $('#traces').append(newLine);
            evenLine = !evenLine;
            lineCount++;
            this.scrollToBottom();
        };
        ClientFunctions.prototype.getShortTimeText = function (dt) {
            var h = dt.getHours().toString();
            var m = dt.getMinutes().toString();
            var s = dt.getSeconds().toString();
            var pp = '00';
            return pp.substring(0, 2 - h.length) + h + ':' + pp.substring(0, 2 - m.length) + m + ':' + pp.substring(0, 2 - s.length) + s;
        };
        ClientFunctions.prototype.writeMessage = function (m) {
            $('#traces').append('<li>' + m + '</li>');
        };
        ClientFunctions.prototype.writeMessages = function (ms) {
            ms.forEach(function (m) {
                $('#traces').append('<li><strong>' + m + '</li>');
            });
        };
        ClientFunctions.prototype.scrollToBottom = function () {
            if (this.stayWithLatest) {
                $('html, body').scrollTop($(document).height());
            }
        };
        ClientFunctions.prototype.scrollToBottomSuspendedToggle = function (checked, id) {
            this.stayWithLatest = checked;
        };
        return ClientFunctions;
    }());
    Fonlow_Logging.ClientFunctions = ClientFunctions;
    var ManagementFunctions = (function () {
        function ManagementFunctions() {
        }
        ManagementFunctions.prototype.clear = function () {
            $('#traces').empty();
            lineCount = 0;
        };
        return ManagementFunctions;
    }());
    Fonlow_Logging.ManagementFunctions = ManagementFunctions;
})(Fonlow_Logging || (Fonlow_Logging = {}));
var evenLine = false;
var lineCount = 0;
var clientFunctions = new Fonlow_Logging.ClientFunctions();
var webUiFunctions = new Fonlow_Logging.WebUiFunctions();
var managementFunctions = new Fonlow_Logging.ManagementFunctions();
var originalText = "saveTime";
var clientSettings;
$(document).on("mouseenter", "span.time", function () {
    originalText = $(this).text();
    $(this).text($(this).attr("value"));
});
$(document).on("mouseleave", "span.time", function () {
    $(this).text(originalText);
});
$(document).on("click", "span.origin", function () {
    $(this).siblings('.message').replaceWith(function () {
        return $(this).prop('tagName') == 'SPAN' ?
            $('<pre/>', {
                class: 'message',
                text: $(this).text()
            })
            :
                $('<span/>', {
                    class: 'message',
                    text: $(this).text()
                });
    });
});
$(document).on('change', 'select#sourceLevels', function () {
    clientFunctions.sourceLevels = parseInt(this.value);
});
//# sourceMappingURL=logging.js.map