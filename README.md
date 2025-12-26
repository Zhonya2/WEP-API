TCP Messenger (C# Console Chat)
A simple console-based multi-user chat application built using TCP sockets.
The project contains two separate applications:
ChatServer — manages connections, broadcasts messages, handles PMs, logs events.
ChatClient — connects to the server, sends messages, receives updates.

Features
 Multiple clients connected simultaneously
 Nickname registration (/nick)
 Public chat messages
 Private messaging (/pm username message)
 Exit command (/exit)
 Asynchronous message processing
 Server-side logging of all activity
