using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;

namespace FrameWork {

    public class FWHttpServ {

        private HttpServer Server;
        private const int Port = 88;

        public FWHttpServ() {
            Server = new HttpServer();
            Server.Port = Port;
            Server.ServerName = "FrameWork HTTP";
            Server.OnHttpRequest += new OnHttpRequestHandler(httpRequestHandler);
        }

        public void startServer() {
            Server.Active = true;
            CrestronConsole.PrintLine("[FWHttpServ]Started Server on port {0}", Port);
        }
        public void stopServer() {
            Server.Active = false;
            CrestronConsole.PrintLine("[FWHttpServ]Stopped Server", Port);
        }

        //===================// Event Handlers //===================//

        private void httpRequestHandler(object _sender, OnHttpRequestArgs _e) {
            int bytesSent = 0;
            
            switch (_e.Request.Header.RequestPath) {
                case "/":
                case "/home": {
                    // Test Reply
                    _e.Response.ContentSource = ContentSource.ContentString;
                    _e.Response.ContentString = "This<br/>is<br/>only<br/>a<br/>test.<br/>";
                    _e.Response.Header.SetHeaderValue("Content-Type", "text/html");
                    break;
                }
                case "/rooms": { // Change this to a POST request eventually
                    string data   = "<rooms>";

                    foreach (KeyValuePair<ushort, Zone> zn in Core.Zones) {
                        //data += String.Format("[{0}] {1}<br/>", zn.Value.id, zn.Value.name); // HTML
                        data += String.Format("<room><id>{0}</id><name>{1}</name></room>", zn.Value.id, zn.Value.name);
                    }

                    _e.Response.ContentSource = ContentSource.ContentString;
                    _e.Response.Header.SetHeaderValue("Content-Type", "text/html");
                    _e.Response.ContentString = data + "</rooms>";
                    _e.Response.Code = 200;
                    break;
                }
                default: {

                    break;
                }
            }
        }

    }

}