﻿///<reference path="../typings/jquery/jquery.d.ts" />
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
        timeUtc: Date;

        origin?: string;
    }

    export interface ClientSettings {
        bufferSize: number;
        advancedMode: boolean;
    }

    export interface ClientInfo {
        id: string;
        username: string;
        ipAddress: string;
        connectedTimeUtc: Date;
        clientType: ClientType;
        userAgent: string;
        template: string;
    }

    export enum ClientType { Undefined = 0, TraceListener = 1, Browser = 2, Console = 4 }

    export class WebUiFunctions {
        renderClientsInfo(clientsInfo: ClientInfo[]) {
            var divs = clientsInfo.map(function (m) {
                var div = $('<div/>', { class: 'hubClientInfo' });
                div.append($('<span/>', { class: 'hc-type' }).text(Fonlow_Logging.ClientType[m.clientType]));
                div.append($('<span/>', { class: 'hc-userAgent' }).text(m.userAgent));
                div.append($('<span/>', { class: 'hc-ip' }).text(m.ipAddress));
                div.append($('<span/>', { class: 'time' }).text(m.connectedTimeUtc.toString()));

                if (m.clientType == Fonlow_Logging.ClientType.TraceListener) {
                    div.append($('<span/>', { class: 'hc-template' }).text(m.template));
                }

                div.append($('<span/>', { class: 'hc-user' }).text(m.username));
                div.append($('<span/>', { class: 'hc-id' }).text(m.id));
                return div;
            });

            var list = $('<li/>', { class: 'hubClients' });
            list.append(divs);
            $('#traces').append(list);
            lineCount++;
        }


    }

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

        bufferSize = 10000;//this will be altered by Web.config through a server call retrieveClientSettings once the signalR connection is established.

        stayWithLatest: boolean = true;

        sourceLevels: number = -1;//all

        private createNewLine(tm: TraceMessage): JQuery {
            var et = this.eventTypeToString(tm.eventType);
            var $eventText = $('<span/>', { class: et + ' et' }).text(et + ': ');
            var $timeText = $('<span/>', { class: 'time', value: tm.timeUtc }).text(' ' + this.getShortTimeText(new Date(tm.timeUtc.toString())) + ' ');//The Json object seem to become string rather than Date. A bug in SignalR JS? Now I have to cast it 
            var $originText = $('<span/>', { class: 'origin btn-xs btn-primary', onclick: 'void(0)' }).text(' ' + tm.origin + '  ');
            var $messageText = $('<span/>', { class: 'message' }).text(tm.message);
            var newLine = $('<li/>', { class: evenLine ? 'even' : 'odd' });
            newLine.append($eventText);
            newLine.append($timeText);
            newLine.append($originText);
            newLine.append($messageText);
            return newLine;
        }

        private addLine(tm: TraceMessage) {
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
        }

        private getShortTimeText(dt: Date) {
            var h = dt.getHours().toString();
            var m = dt.getMinutes().toString();
            var s = dt.getSeconds().toString();
            var pp = '00';
            return pp.substring(0, 2 - h.length) + h + ':' + pp.substring(0, 2 - m.length) + m + ':' + pp.substring(0, 2 - s.length) + s;
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
            if ((tm.eventType & this.sourceLevels) == 0)
                return;

            this.addLine(tm);
        }

        //Write traces in fixed size queue defined by this.bufferSize 
        writeTraces = (tms: TraceMessage[]) => {
            if (this.sourceLevels > 0) {
                tms = tms.filter((m) => (m.eventType & this.sourceLevels) != 0);
            } else if (this.sourceLevels === 0) {
                return;
            }


            //Clean up some space first
            if (lineCount + tms.length > this.bufferSize) {
                var numberOfLineToRemove = lineCount + tms.length - this.bufferSize;
                $('#traces li:nth-child(-n+' + numberOfLineToRemove + ')').remove();//Thanks to this trick http://stackoverflow.com/questions/9443101/how-to-remove-the-n-number-of-first-or-last-elements-with-jquery-in-an-optimal, much faster than my loop

                lineCount -= numberOfLineToRemove;
            }


            //Buffer what to add
            var itemsToAppend = $();
            $.each(tms, (index, tm) => {
                itemsToAppend = itemsToAppend.add(this.createNewLine(tm));//append siblings
                evenLine = !evenLine; //Silly, I should have used math :), but I wanted simplicity
            });

            $('#traces').append(itemsToAppend);

            lineCount += tms.length;

            this.scrollToBottom();
        }

        private scrollToBottom() {
            if (this.stayWithLatest) {
                $('html, body').scrollTop($(document).height());
            }
        }

        scrollToBottomSuspendedToggle(checked: boolean, id: string) {
            this.stayWithLatest = checked;
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

var webUiFunctions = new Fonlow_Logging.WebUiFunctions();

var managementFunctions = new Fonlow_Logging.ManagementFunctions();

var originalText = "saveTime";

var clientSettings: Fonlow_Logging.ClientSettings;

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
            })
            ;

    });
});

$(document).on('change', 'select#sourceLevels', function () {
    clientFunctions.sourceLevels = parseInt(this.value);
});
