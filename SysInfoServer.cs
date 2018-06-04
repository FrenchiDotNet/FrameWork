using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace FrameWork {

    /**
     * Class:       SysInfoServer
     * @author:     Ryan French
     * @version:    1.3a
     * Description: ...
     */
    public class SysInfoServer {

        //===================// Members //===================//

        private TCPServer Server;

        // Server Settings
        public int port { get; private set; }
        public int bufferSize { get; private set; }

        private string clientMessage;

        private DebugConsole Debug;

        //===================// Constructor //===================//

        public SysInfoServer(int _port, int _buffer) {

            this.port       = _port;
            this.bufferSize = _buffer;

            this.Debug = new DebugConsole("[SysInfoServer]");
            this.Debug.debugEnabled = true;

        }

        //===================// Methods //===================//

        /**
         * Method: startServer
         * Access: public
         * @return: void
         * Description: Create TCPServer object and register event handlers.
         */
        public void startServer() {

            Server = new TCPServer("0.0.0.0", port, bufferSize, EthernetAdapterType.EthernetLANAdapter, 1);
            Server.SocketStatusChange += new TCPServerSocketStatusChangeEventHandler(socketStatusHandler);
            Server.WaitForConnectionAsync(new TCPServerClientConnectCallback(clientConnectHandler));
            Debug.print("Started Server on port "+port);

        }

        /**
         * Method: stopServer
         * Access: public
         * @return: void
         * Description: Disconnect all socket clients and remove event handlers.
         */
        public void stopServer() {

            Server.DisconnectAll();
            Server.SocketStatusChange -= socketStatusHandler;
            Debug.print("Stopped Server.");

        }

        /**
         * Method: replyAndEndConnection
         * Access: private
         * @return: void
         * @param: string _msg
         * @param: uint _cli
         * Description: Encodes and transmits string _msg to connected client _cli, then disconnects the client and
         *              listens for a new connection.
         */
        private void replyAndEndConnection(string _msg, uint _cli) {

            Server.SendData(_cli, Encoding.ASCII.GetBytes(_msg), _msg.Length);
            Server.Disconnect(_cli);
            Server.WaitForConnectionAsync(new TCPServerClientConnectCallback(clientConnectHandler));

        }

        /**
         * Method: replyAndWait
         * Access: private
         * @return: void
         * @param: string _msg
         * @param: uint _cli
         * Description: Encodes and transmits string _msg to connected client _cli, then listens for new data.
         */
        private void replyAndWait(string _msg, uint _cli) {

            Server.SendData(_cli, Encoding.ASCII.GetBytes(_msg), _msg.Length);
            Server.SendData(_cli, Encoding.ASCII.GetBytes("\r\n>> "), 5);
            Server.ReceiveDataAsync(new TCPServerReceiveCallback(serverReceiveCallback));

        }

        /**
         * Method: ParseRequest
         * Access: private
         * @return: void
         * @param: string _msg
         * @param: uint _cli
         * Description: Check incoming command against a list of supported terms.
         */
        private void ParseRequest(string _cmd, uint _cli) {
            
            int count;
            string command, reply;
            string[] args, sTmp;
            reply = "";

            // Break incoming message into command and args
            sTmp = _cmd.Split(' ');
            command = sTmp[0];
            args = sTmp.Skip(1).ToArray();

            // Check for supported functions
            switch (command.ToLower()) {
                case "getroomlist":
                    count = Core.ZoneList.Count;
                    for(int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.ZoneList[i].id, Core.ZoneList[i].name);
                    }
                    break;
                case "getsourcelist":
                    count = Core.SourceList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.SourceList[i].id, Core.SourceList[i].name);
                    }
                    break;
                case "getroomstatus":
                    if (args.Length > 0) {
                        reply = cmdGetRoomStatus(Int32.Parse(args[0]));
                    } else
                        reply = "No ZoneID Specified for GetRoomStatus\r\n";
                    break;
                case "getroomsources":
                    if (args.Length > 0) {
                        reply = cmdGetRoomSources(Int32.Parse(args[0]));
                    } else
                        reply = "No ZoneID Specified for GetRoomSources\r\n";
                    break;
                case "getroomlights":
                    reply = cmdGetRoomLights(Int32.Parse(args[0]));
                    break;
                case "getroomshades":
                    reply = cmdGetRoomShades(Int32.Parse(args[0]));
                    break;
                case "getroomhvac":
                    reply = cmdGetRoomHVAC(Int32.Parse(args[0]));
                    break;
                case "setroomvol":
                    reply = cmdSetRoomVol(args);
                    break;
                case "setroomsource":
                    reply = cmdSetRoomSource(args);
                    break;
                case "displaycmd":
                    reply = cmdDisplayCmd(args);
                    break;
                case "getlightslist":
                    count = Core.LightList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.LightList[i].id, Core.LightList[i].name);
                    }
                    break;
                case "getshadeslist":
                    count = Core.ShadeList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.ShadeList[i].id, Core.ShadeList[i].name);
                    }
                    break;
                case "gethvaclist":
                    count = Core.HVACList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.HVACList[i].id, Core.HVACList[i].name);
                    }
                    break;
                case "getinterfacelist":
                    count = Core.InterfaceList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.InterfaceList[i].id, Core.InterfaceList[i].name);
                    }
                    break;
                case "getsecuritylist":
                    count = Core.SecurityKeypadList.Count;
                    for (int i = 0; i < count; i++) {
                        reply += String.Format("[{0}] {1}\r\n", Core.SecurityKeypadList[i].id, Core.SecurityKeypadList[i].name);
                    }
                    break;
                case "getinterfacesecurity":
                    if (args.Length > 0) {
                        reply = cmdGetInterfaceSecurity(Int32.Parse(args[0]));
                    } else
                        reply = "No InterfaceID Specified for GetInterfaceSecurity\r\n";
                    break;
                case "help":
                    reply = "getroomlist\r\n" +
                            "getsourcelist\r\n" +
                            "getroomstatus <ZoneID>\r\n" +
                            "getroomsources <ZoneID>\r\n" +
                            "setroomsource <ZoneID> <SourceID>\r\n" +
                            "getroomlights <ZoneID>\r\n" +
                            "displaycmd <ZoneID> <DisplayCmd>\r\n" +
                            "getlightslist\r\n" +
                            "getshadeslist\r\n" +
                            "gethvaclist\r\n" +
                            "getinterfacelist\r\n" +
                            "getsecuritylist\r\n" +
                            "getinterfacesecurity <InterfaceID>\r\n";
                    break;
                case "bye":
                    replyAndEndConnection("Goodbye.", _cli);
                    return;
                default:
                    reply = "Unknown Command: " + _cmd + "\r\n";
                    break;
            }

            replyAndWait(reply, _cli);

        }

        /**
         * Method: cmdGetRoomStatus
         * Access: private
         * @return: string
         * @param: int _roomID
         * Description: Look up Room object by _roomID and return the current Source, Volume Level, and Mute Status.
         */
        private string cmdGetRoomStatus(int _roomID) {

            string msg = "";

            if (Core.Zones.ContainsKey((ushort)_roomID)) {
                Zone zn = Core.Zones[(ushort)_roomID];
                msg += String.Format("Status of [{0}] {1}:\r\n", zn.id, zn.name);
                msg += String.Format("Source: {0}\r\nVolume: {1}\r\nMute: {2}\r\n", zn.currentSourceName, zn.volume, zn.muteState ? "On" : "Off");
            } else
                msg = String.Format("Unknown ZoneID: {0}\r\n", _roomID);

            return msg;

        }

        /**
         * Method: cmdGetRoomSources
         * Access: private
         * @return: string
         * @param: int _roomID
         * Description: Look up Room object by _roomID and return a formatted list of sourceID's and Sources.
         */
        private string cmdGetRoomSources(int _roomID) {

            string msg = "";
            int i;
            Zone zn;
            Source src;

            if (Core.Zones.ContainsKey((ushort)_roomID)) {
                zn = Core.Zones[(ushort)_roomID];
                
                // Debug
                Debug.print(String.Format("Zone [{0}] audioSources: {1}, videoSources: {2}", zn.id, zn.audioSources.Count, zn.videoSources.Count));

                // Audio Sources
                if (zn.hasAudio) {
                    msg += "Audio Sources:\r\n";
                    for(i = 0; i < zn.audioSources.Count; i++) {
                        src = zn.audioSources[i];
                        msg += String.Format("\t[{0}] {1}\r\n", src.id, src.name);
                    }
                } else
                    msg += "\tNo Audio Sources\r\n";

                // Video Sources
                if (zn.hasVideo) {
                    msg += "Video Sources:\r\n";
                    for (i = 0; i < zn.videoSources.Count; i++) {
                        src = zn.videoSources[i];
                        msg += String.Format("\t[{0}] {1}\r\n", src.id, src.name);
                    }
                } else
                    msg += "\tNo Video Sources\r\n";

            } else
                msg += String.Format("Unknown ZoneID: {0}\r\n", _roomID);

            return msg;

        }

        /**
         * Method: cmdGetRoomLights
         * Access: private
         * @return: string
         * @param: int _roomID
         * Description: Look up Room object by _roomID and return a formatted list of LightID's.
         */
        private string cmdGetRoomLights(int _roomID) {

            string msg = "";
            int i;
            Zone zn;
            Light light;

            if (Core.Zones.ContainsKey((ushort)_roomID)) {
                zn = Core.Zones[(ushort)_roomID];

                if (zn.hasLights) {
                    if (zn.lightingLoads.Count > 0) {
                        msg += "Lighting Loads:\r\n";
                        for (i = 0; i < zn.lightingLoads.Count; i++) {
                            light = zn.lightingLoads[i];
                            msg += String.Format("\t[{0}] {1}\r\n", light.id, light.name);
                        }
                    }
                    if (zn.lightingPresets.Count > 0) {
                        msg += "Lighting Presets:\r\n";
                        for (i = 0; i < zn.lightingPresets.Count; i++) {
                            light = zn.lightingPresets[i];
                            msg += String.Format("\t[{0}] {1}\r\n", light.id, light.name);
                        }
                    }
                } else
                    msg += "No Lights Assigned\r\n";

            } else
                msg += String.Format("Unknown ZoneID: {0}\r\n", _roomID);

            return msg;

        }

        /**
         * Method: cmdGetRoomShades
         * Access: private
         * @return: string
         * @param: int _roomID
         * Description: Look up Room object by _roomID and return a formatted list of ShadeID's.
         */
        private string cmdGetRoomShades(int _roomID) {

            string msg = "";
            int i;
            Zone zn;
            Shade shade;

            if (Core.Zones.ContainsKey((ushort)_roomID)) {
                zn = Core.Zones[(ushort)_roomID];

                if (zn.hasShades) {
                    if (zn.shades.Count > 0) {
                        msg += "Shades:\r\n";
                        for (i = 0; i < zn.shades.Count; i++) {
                            shade = zn.shades[i];
                            msg += String.Format("\t[{0}] {1}\r\n", shade.id, shade.name);
                        }
                    }
                } else
                    msg += "No Shades Assigned\r\n";

            } else
                msg += String.Format("Unknown ZoneID: {0}\r\n", _roomID);

            return msg;

        }

        /**
         * Method: cmdGetRoomHVAC
         * Access: private
         * @return: string
         * @param: int _roomID
         * Description: Look up Room object by _roomID and return a formatted list of ShadeID's.
         */
        private string cmdGetRoomHVAC(int _roomID) {

            string msg = "";
            int i;
            Zone zn;
            HVAC hvc;

            if (Core.Zones.ContainsKey((ushort)_roomID)) {
                zn = Core.Zones[(ushort)_roomID];

                if (zn.hasHVAC) {
                    if (zn.hvacs.Count > 0) {
                        msg += "HVAC Zones:\r\n";
                        for (i = 0; i < zn.hvacs.Count; i++) {
                            hvc = zn.hvacs[i];
                            msg += String.Format("\t[{0}] {1}\r\n", hvc.id, hvc.name);
                        }
                    }
                } else
                    msg += "No HVAC Zones Assigned\r\n";

            } else
                msg += String.Format("Unknown ZoneID: {0}\r\n", _roomID);

            return msg;

        }

        /**
         * Method: cmdGetInterfaceSecurity
         * Access: private
         * @return: string
         * @param: int _intID
         * Description: ..
         */
        private string cmdGetInterfaceSecurity(int _intID) {

            string msg = "";
            int i;
            Interface inf;
            SecurityKeypad kp;

            if (Core.Interfaces.ContainsKey((ushort)_intID)) {
                inf = Core.Interfaces[(ushort)_intID];

                if (inf.hasSecurityKeypad) {
                    msg += "Security Keypads:\r\n";
                    for (i = 0; i < inf.securityKeypadList.Count; i++) {
                        kp = inf.securityKeypadList[i];
                        msg += String.Format("\t[{0}] {1}\r\n", kp.id, kp.name);
                    }
                } else
                    msg += "No Security Keypads Assigned\r\n";

            } else
                msg += String.Format("Unknown InterfaceID: {0}\r\n", _intID);

            return msg;

        }

        /**
         * Method: cmdSetRoomVol
         * Access: private
         * @return: string
         * @param: string[] args
         * Description: Look up Zone object by zoneID and send direct volume command with defined level
         */
        private string cmdSetRoomVol(string[] args) {

            // arg[0] = RoomID
            // arg[1] = volume

            string msg = "";
            
            if (args.Length >= 2) {
                ushort rid = ushort.Parse(args[0]);
                ushort vol = ushort.Parse(args[1]);
                
                if (Core.Zones.ContainsKey(rid)) {
                    Zone zn = Core.Zones[rid];
                    zn.SetVolumeDirect(vol);
                    msg = String.Format("[{0}] {1} - Volume {2}\r\n", zn.id, zn.name, zn.volume);
                } else
                    msg = "Invalid ZoneID: " + rid + "\r\n";
            } else
                msg = "Incorrect number of arguments: setRoomVol (ZoneID) (Level)\r\n";

            return msg;

        }

        /**
         * Method: cmdSetRoomSource
         * Access: private
         * @return: string
         * @param: string[] args
         * Description: Look up Zone object by ZoneID and send source select command with defined SourceID
         */
        private string cmdSetRoomSource(string[] args) {

            // arg[0] = RoomID
            // arg[1] = sourceID

            string msg = "";

            if (args.Length >= 2) {
                ushort rid = ushort.Parse(args[0]);
                ushort sid = ushort.Parse(args[1]);
                if (Core.Zones.ContainsKey(rid)) {
                    Zone zn = Core.Zones[rid];

                    // Check if source is valid
                    if(Core.Sources.ContainsKey(sid)) {
                        zn.SetCurrentSource(sid);
                        msg = String.Format("[{0}] {1} - Current Source {2}\r\n", zn.id, zn.name, zn.currentSource);
                    } else 
                        msg += "Invalid SourceID: " + sid + "\r\n";
                } else
                    msg = "Invalid ZoneID: " + rid + "\r\n";
            } else
                msg = "Incorrect number of arguments: setRoomSource (ZoneID) (SourceID)\r\n";

            return msg;

        }

        /**
         * Method: cmdDisplayCmd
         * Access: private
         * @return: string
         * @param: string[] args
         * Description: Look up Display object by ZoneID and send DisplayCommand from enum lookup
         */
        private string cmdDisplayCmd(string[] args) {

            // arg[0] = ZoneID
            // arg[1] = DisplayCommands enum

            string msg = "";

            if (args.Length >= 2) {
                ushort zid = ushort.Parse(args[0]);
                if (Core.Zones.ContainsKey(zid)) {
                    Zone zn = Core.Zones[zid];
                    if (zn.hasDisplay) {
                        try {
                            DisplayCommands cmd = (DisplayCommands)Enum.Parse(typeof(DisplayCommands), args[1], true);
                            if (Enum.IsDefined(typeof(DisplayCommands), cmd)) {
                                zn.display.DisplayCommandPulse((ushort)cmd);
                                msg = "Display Command Sent: " + args[1] + "\r\n"; ;
                            }
                        }
                        catch (ArgumentException) {
                            msg = String.Format("Unknown Display Command {0}\r\n", args[1]);
                        }

                    } else
                        msg = "Zone " + zid + " does not have a registered display.\r\n";
                } else
                    msg = "Invalid ZoneID: " + zid + "\r\n";
            } else
                msg = "Incorrect number of arguments: displayCmd (ZoneID) (DisplayCommand)\r\n";

            return msg;

        }

        //===================// Event Handlers //===================//

        /**
         * Method: socketStatusHandler
         * Access: private
         * @return: void
         * @param: TCPServer _serv
         * @param: uint _cIndex
         * @param: SocketStatus _stat
         * Description: ...
         */
        private void socketStatusHandler(TCPServer _serv, uint _cIndex, SocketStatus _stat) {

            string message = "";

            switch (_stat) {
                case SocketStatus.SOCKET_STATUS_NO_CONNECT:
                    message = "Not Connected";
                    break;
                case SocketStatus.SOCKET_STATUS_WAITING:
                    message = "Waiting for Connection.";
                    break;
                case SocketStatus.SOCKET_STATUS_CONNECTED:
                    message = "Connected.";
                    break;

            }

            Debug.print("Socket Status Changed: " + message);

        }

        /**
         * Method: clientConnectHandler
         * Access: private
         * @return: void
         * @param: TCPServer _serv
         * @param: uint _cli
         * Description: Sends command prompt to connected client and listens for data.
         */
        private void clientConnectHandler(TCPServer _serv, uint _cli) {

            Debug.print(String.Format("Client {0} Connected from IP {1}", _cli, _serv.GetAddressServerAcceptedConnectionFromForSpecificClient(_cli)));
            Server.SendData(_cli, Encoding.ASCII.GetBytes(">> "), 3);
            Server.ReceiveDataAsync(new TCPServerReceiveCallback(serverReceiveCallback));

        }

        /**
         * Method: serverReceiveCallback
         * Access: private
         * @return: void
         * @param: TCPServer _serv
         * @param: uint _cli
         * @param: int _size
         * Description: Scrapes incoming data from buffer and checks for line terminator. When found, sends gathered input to ParseRequest
         */
        private void serverReceiveCallback(TCPServer _serv, uint _cli, int _size) {

            string data = System.Text.Encoding.UTF8.GetString(_serv.GetIncomingDataBufferForSpecificClient(_cli), 0, _size);
            clientMessage += data;

            if (!clientMessage.Contains("\x0D\x0A")) 
                // Data still incoming, keep reading
                _serv.ReceiveDataAsync(_cli, serverReceiveCallback);
            else { 
                // CRLF detected, parse request and clean up
                int index = clientMessage.IndexOf("\x0D\x0A");
                clientMessage = clientMessage.Remove(index, 2);
                ParseRequest(clientMessage, _cli);

                clientMessage = "";
            }

        }

    } // End SysInfoServer Class

    internal class DebugConsole {

        public bool debugEnabled;
        private string prefix;

        public DebugConsole(string _prefix) {
            this.prefix = _prefix;
        }

        public void print(string _msg) {
            if(debugEnabled)
                Core.ConsoleMessage(String.Format("{0} {1}", prefix, _msg));
        }

    }
}