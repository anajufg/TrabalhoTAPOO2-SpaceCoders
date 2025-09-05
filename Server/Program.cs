using Server.GameServer;

const int Port = 9000;
var server = new GameServer();
await server.StartServer(Port);
