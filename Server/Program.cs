using Server.ServerSide;

const int port = 9000;
ServerSide server = new ServerSide();

await server.StartServer(port);