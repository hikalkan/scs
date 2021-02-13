SCS - TCP Server/Client Communication and RMI Framework
======================================================================

### What is SCS?

- It is well documented open source server/client framework.
- Allows remote method calls from client to server and from server to client easily. Can throw exceptions across applications.
- Allows acynhronous or synchronous low level messaging instead of remote method calls.
- It is scalable (15000+ clients concurrently connected and communicating while server has only 50-60 threads) and fast (5,500 remote method calls, 62,500 messages transfer between applications in a second running in a regular PC).
- Allows clients to automatically reconnect to the server.
- Allows clients to automatically ping to the server to keep the connection available when no communication occurs with the server for a while.
- Allows a server to register events for new client connections, disconnecting of a client, etc.
- Allows a client to register events for connecting and disconnecting.
- It is suitable for long session connections between clients and server.

### Detailed Documentation

- Usage: http://www.codeproject.com/Articles/153938/A-Complete-TCP-Server-Client-Communication-and-RMI
- Implementation: http://www.codeproject.com/Articles/155282/A-Complete-TCP-Server-Client-Communication-and-RMI

### How to download

You can download SCS binaries and source codes directly from here (Github).

If you're using Visual Studio, you can get from Nuget (https://www.nuget.org/packages/SCS).
