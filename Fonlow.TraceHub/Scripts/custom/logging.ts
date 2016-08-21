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
        }

        bufferSize  = 10000;

        private createNewLine(tm: TraceMessage): JQuery {
            var et = this.eventTypeToString(tm.eventType);
            var $eventText = $('<span/>', { class: et }).text(et + ': ');
            var $timeText = $('<span/>', { class: 'time' }).text(' ' + tm.timeUtc + ' ');
            var $originText = $('<span/>', { class: 'origin' }).text(' ' + tm.origin + '  ');
            var newLine = $('<li/>', { class: evenLine ? 'even' : 'odd' });
            newLine.append($eventText);
            newLine.append($timeText);
            newLine.append($originText);
            newLine.append(tm.message);
            return newLine;
        }

        private addLine(tm: TraceMessage) {
            var newLine = this.createNewLine(tm);
            $('#traces').prepend(newLine);
            evenLine = !evenLine;
            lineCount++;

            if (lineCount > this.bufferSize) {
                $('#traces li:last').remove();
            }
            
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
            //Clean up some space first
            if (lineCount + tms.length > this.bufferSize) {
                var numberOfLineToRemove = lineCount + tms.length - this.bufferSize;
                $('#traces li:nth-last-child(-n+' + numberOfLineToRemove + ')').remove();//Thanks to this trick http://stackoverflow.com/questions/9443101/how-to-remove-the-n-number-of-first-or-last-elements-with-jquery-in-an-optimal, much faster than my loop

                lineCount -= numberOfLineToRemove;
            }


            //Buffer what to add
            var itemsToPrepend = $();
            $.each(tms.reverse(), (index, tm) => {
                itemsToPrepend = itemsToPrepend.add(this.createNewLine(tm));//prepend of siblings
                evenLine = !evenLine; //Silly, I should have used math :), but I wanted simplicity
            });

            $('#traces').prepend(itemsToPrepend);
            lineCount += tms.length;

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

