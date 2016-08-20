///<reference path="../typings/jquery/jquery.d.ts" />
///<reference path="../typings/signalr/signalr.d.ts" />
var Fonlow_Logging;
(function (Fonlow_Logging) {
    //export interface ILoggingClient {
    //    writeMessage(m: string);
    //    writeMessages(ms: string[]);
    //    writeTrace(tm: TraceMessage);
    //    writeTraces(tms: TraceMessage[]);
    //}
    //export interface ILogging {
    //    writeTrace(tm: TraceMessage);
    //    writeTraces(tms: TraceMessage[]);
    //}
    var ClientFunctions = (function () {
        function ClientFunctions() {
            var _this = this;
            this.writeTrace = function (tm) {
                _this.addLine(tm);
            };
            this.writeTraces = function (tms) {
                tms.forEach(function (tm) {
                    _this.addLine(tm);
                });
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
                default:
                    return "Misc ";
            }
        };
        ClientFunctions.prototype.addLine = function (tm) {
            var et = this.eventTypeToString(tm.eventType);
            var $eventText = $('<span/>', { class: et }).text(et + ': ');
            var $timeText = $('<span/>', { class: 'time' }).text(' ' + tm.timeUtc + ' ');
            var $originText = $('<span/>', { class: 'origin' }).text(' ' + tm.origin + '  ');
            var newLine = $('<li/>', { class: evenLine ? 'even' : 'odd' });
            newLine.append($eventText);
            newLine.append($timeText);
            newLine.append($originText);
            newLine.append(tm.message);
            $('#traces').append(newLine);
            evenLine = !evenLine;
            lineCount++;
        };
        ClientFunctions.prototype.writeMessage = function (m) {
            $('#traces').append('<li>' + m + '</li>');
        };
        ClientFunctions.prototype.writeMessages = function (ms) {
            ms.forEach(function (m) {
                $('#traces').append('<li><strong>' + m + '</li>');
            });
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
var managementFunctions = new Fonlow_Logging.ManagementFunctions();
//# sourceMappingURL=logging.js.map