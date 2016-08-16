///<reference path="../typings/jquery/jquery.d.ts" />
///<reference path="../typings/signalr/signalr.d.ts" />

module Fonlow_Logging {
    export interface TraceMessage {
        eventType?: number;
        source?: string;
        id?: number;
        relatedActivityId?: string;
        message: string;

        callstack?: string;
        processId?: number;
        threadId?: number;
        timeUtc?: Date;

        origin?: string;
    }


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

    export class ClientFunctions {

        private eventTypeToString(t: number): string {
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
        }

        private addLine(tm: TraceMessage) {
            var et = this.eventTypeToString(tm.eventType);
            var $eventText = $('<span/>', { class: et }).text(et + ': ');
            var $timeText = $('<span/>', { class: 'time' }).text(' '+tm.timeUtc + ' ');
            var $originText = $('<span/>', { class: 'origin' }).text(' ' + tm.origin + '  ');
            var newLine = $('<li/>', { class: evenLine ? 'even' : 'odd' });
            newLine.append($eventText);
            newLine.append($timeText);
            newLine.append($originText);
            newLine.append(tm.message);
            $('#traces').append(newLine);
            evenLine = !evenLine;
            lineCount++;
        }

        writeMessage(m: string) {
            $('#traces').append('<li>' + m + '</li>');
        }

        writeMessages(ms: string[]) {
            ms.forEach((m) => {
                $('#traces').append('<li><strong>' + m + '</li>');
            });
        }

        writeTrace = (tm: TraceMessage) => { //Arrow function to ensure "this" is about this instance of the class, rather than caller SingleR Hub
            this.addLine(tm);
        }

        writeTraces = (tms: TraceMessage[]) => {
            tms.forEach((tm) => {
                this.addLine(tm);
            });
        }

    }

    export class ManagementFunctions {
        clear() {
            $('#traces').empty();
            lineCount = 0;
        }
    }


}

var evenLine: boolean = false;
var lineCount = 0;

var clientFunctions = new Fonlow_Logging.ClientFunctions();

var managementFunctions = new Fonlow_Logging.ManagementFunctions();

