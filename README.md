# Remote-Code-Page-Management
1.Designed and implemented a remote code page management using C++.

2.Built parser to extract and build type and dependency information from collection of files. Implemented asynchronous message passing communication using Windows Communication Foundation (WCF), for communication between client and server.

3.Built CLI translater to connect C++ code with the C# code.

4.Design graphical user interface for user to send request and display the test result using Windows Presentation Foundation(WPF).


=======Build Instructions=======

1.You must run Visual Studio as administrator

2.class library: AbstractSyntaxTree, AutoTestUnit, CommLibWrapper, Converter, BlockingQueue, CppNoSqlDb, CppProperties, Dependencies, Display,Executive,FileSystem,GrammarHelpers,Loader,Logger,Message,MsgPassingComm,Navigator,Parser,Process,Publisher,ScopeStack,SemiExpression,Sockets,Tokenizer,Translater,Utilities.

3.Build rest as console application

=======Run Instructions======== 

1.Command line for package Navigator:"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "../../../" ".cpp" ".h"
  Explain:1) "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" this is the address of your default browser so you can use application open file in the process.
          2) "../../../"  this is the default for the root address.
          3) ".cpp" ".h" this is for the filter to show the files that you want to transfer

2.Run: multi-start Serverprototype and Navigator
