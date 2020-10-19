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


1.首先通过一个叫做Tokenizer的包，将需要处理文件的代码的字符分类处理，比如分成空格，换行符，数字和字母，单引号，双引号，单斜杠，双斜杠等等。
2.之后通过一个叫做语法分析器的包，将之前分类好的字符逐一合并，并且归类成是变量，注释，表达式，还是函数等等，然后将这个类封装。
  外部调用，需要实例该类，当实例化这个类后，通过反复调取get（）函数获从文件头至文件尾的以整个表达式的显示的语句。
3.在每次调取get（）函数的过程中，处理代码中特殊的字符如<,>,&,",空格等等，并替换成html中的相应的符号，。并将处理好的数据写入
新的html文件，并添加依赖关系索引。
4.服务器端处理完文件后，客户端如果有下载请求，会将处理好的文件发回给客户端。

为什么用异步通信而不是同步通信？
考虑到这个程序运行在一台服务器上，并且有多个客户端访问，每个客户端相应请求后（如发送文件，请求下载文件）后，客户端不需要进入队列等待服务器的响应，提高客户端处的效率并且防止程序锁死。
