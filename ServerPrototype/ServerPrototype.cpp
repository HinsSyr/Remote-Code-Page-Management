/////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages  //
// ver 1.3                                                             //
// Source: Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018   //
// Author: Bo Qiu                                                      //
/////////////////////////////////////////////////////////////////////////

#include "ServerPrototype.h"
#include "../FileSystem/FileSystem.h"
#include "../Process/Process.h"
#include <chrono>
#include "../Executive/Executive.h"
#include "../Converter/Converter.h"
namespace MsgPassComm = MsgPassingCommunication;

using namespace Repository;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;

//----< return name of every file on path >----------------------------

Files Server::getFiles(const Repository::SearchPath& path)
{
  return Directory::getFiles(path);
}
//----< return name of every subdirectory on path >--------------------

Dirs Server::getDirs(const Repository::SearchPath& path)
{
  return Directory::getDirectories(path);
}

namespace MsgPassingCommunication
{
  // These paths, global to MsgPassingCommunication, are needed by 
  // several of the ServerProcs, below.
  // - should make them const and make copies for ServerProc usage

  std::string sendFilePath;
  std::string saveFilePath;

  //----< show message contents >--------------------------------------

  template<typename T>
  void show(const T& t, const std::string& msg)
  {
    std::cout << "\n  " << msg.c_str();
    for (auto item : t)
    {
      std::cout << "\n    " << item.c_str();
    }
  }
  //----< test ServerProc simply echos message back to sender >--------

  std::function<Msg(Msg)> echo = [](Msg msg) {
    Msg reply = msg;
    reply.to(msg.from());
    reply.from(msg.to());
    return reply;
  };
  //----< singleDownload ServerProc sends single file back to the requester >----------------

  std::function<Msg(Msg)> singleDownload = [](Msg msg) {
  
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("singleDownload");
	  std::string filename = msg.value("file");
	  reply.attribute("sendingFile", filename);
	 

	  std::string st = storageRoot + "RemoteFiles";
	  std::string filespec = st + "\\" + filename;
	  std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filespec);
	  reply.file(fullSrcPath);
	  return reply;
	  
  };
  //----< getRemoteFiles ServerProc sends back the files in the RemoteFiles >----------------

  std::function<Msg(Msg)> getRemoteFiles = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getRemoteFiles");

	  std::vector<std::string> filevec;

	  std::string st = storageRoot + "RemoteFiles";
	  filevec = FileSystem::Directory::getFiles(st);
	  std::string files;
	  for (auto ele : filevec)
		  files += ele + '@';
	  std::cout << std::endl << "files value : ---------------" << files;
	  reply.attribute("files", files);
	  return reply;
  
  };
  //----< getCmd ServerProc to return the command line into seperate part back to requester >----------------

  std::function<Msg(Msg)> getCmd = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getCmd");
	  std::string count = msg.value("count");
	  std::vector<std::string> vec;
	  std::string path = msg.value("cmdline");
	  int index = 0;
	  for (int i = 0; i < (int)path.size(); i++)
	  {
		  if (path[i] == '@')
		  {
			  std::string temp = path.substr(index, i-index);
			  vec.push_back(temp);
			  index = i+1;
		  }
	  }
	  int argc = std::stoi(count);
	  char** argv = new char*[argc];
	  for (int i = 0; i < argc; i++)
	  {
		  argv[i] = (char*)vec[i].c_str();
	  }
	  Executive exec(argc, argv);
	  std::string browsepath=exec.get_browser_path();
	  std::vector<std::string> patterns= exec.get_patterns();
	  std::string cmdpath=exec.get_command_path();
	  reply.attribute("browsepath", browsepath);
	  reply.attribute("cmdpath", cmdpath);
	  std::string patternstr;
	  for (auto ele : patterns)
		  patternstr+= ele + '@';
	  reply.attribute("patterns", patternstr);
	  return reply;
  };
  //----< getFiles ServerProc returns list of files on path >----------

  std::function<Msg(Msg)> getFiles = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getFiles");
    std::string path = msg.value("path");
	std::cout << std::endl << "path value is: " << path;
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files files = Server::getFiles(searchPath);
      size_t count = 0;
      for (auto item : files)
      {
		  std::cout << std::endl << "item value is: " << item;

        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("file" + countStr, item);
      }
    }
    else
    {
      std::cout << "\n  getFiles message did not define a path attribute";
    }
    return reply;
  };
  //----< getDirs ServerProc returns list of directories on path >-----

  std::function<Msg(Msg)> getDirs = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getDirs");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files dirs = Server::getDirs(searchPath);
      size_t count = 0;
      for (auto item : dirs)
      {
        if (item != ".." && item != ".")
        {
          std::string countStr = Utilities::Converter<size_t>::toString(++count);
          reply.attribute("dir" + countStr, item);
        }
      }
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };
  //----< DownLoad ServerProc to send files to the requester >----------------

  std::function<Msg(Msg)> DownLoad = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("DownLoad");
	  std::string path = "/RemoteFiles";
	  int i = std::stoi(msg.value("count"));
	  std::string st = storageRoot + "RemoteFiles";
	  std::vector<std::string> filelist;
	  filelist = FileSystem::Directory::getFiles(st);
	  if (i >= (int)filelist.size())
	  {
		  reply.attribute("end", "yes");
			  return reply;
	  }
	  reply.attribute("sendingFile", filelist[i]);
	  reply.attribute("end", "no");
	  if (path != "")
	  {
		  std::string searchPath = storageRoot;
		  if (path != "." && path != searchPath)
			  searchPath = searchPath + "\\" + path;
		  if (!FileSystem::Directory::exists(searchPath))
		  {
			  std::cout << "\n  file source path does not exist";
			  return reply;
		  }
		  std::string filePath = searchPath + "/" + filelist[i];
		  std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
	
		  std::string fullDstPath = sendFilePath;
		  if (!FileSystem::Directory::exists(fullDstPath))
		  {
			  std::cout << "\n  file destination path does not exist";
			  return reply;
		  }
		  fullDstPath += "/" + filelist[i];
		  reply.file(fullSrcPath);
	  }
	  else
	  {
		  std::cout << "\n  getDirs message did not define a path attribute";
	  }
	  return reply;
  };
  //----< convertfiles ServerProc transform the files in the server >----------------

  std::function<Msg(Msg)> convertfiles = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("convertfiles");
	  std::string count = msg.value("argc");
	  std::vector<std::string> vec;
	  std::string cmd = msg.value("argv");
	  int index = 0;
	  for (int i = 0; i < (int)cmd.size(); i++)
	  {
		  if (cmd[i] == '@')
		  {
			  std::string temp = cmd.substr(index, i - index);
			  vec.push_back(temp);
			  index = i + 1;
		  }
	  }
	  int argc = std::stoi(count);

	  std::string path = msg.value("path");
	  std::cout << std::endl << "path: " << path;
	  if (path != "")
	  {
		  std::string searchPath = storageRoot;
		  if (path != "." && path != searchPath)
			  searchPath = searchPath + "\\" + path;
		  if (!FileSystem::Directory::exists(searchPath))
		  {
			  std::cout << "\n  file source path does not exist";
			  return reply;
		  }
		  std::string filePath = searchPath + "/" + msg.value("fileName");
		  std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
		  std::vector<std::string> pls;
		  std::cout << std::endl << "full path: "<<fullSrcPath;
		  pls.push_back(fullSrcPath);
		  Union uni;
		  uni.convert(argc, vec, pls);
	  }
	  else
	  {
		  std::cout << "\n  getDirs message did not define a path attribute";
	  }
	  return reply;
  };

  //----< sendFile ServerProc sends file to requester >----------------
  /*
  *  - Comm sends bodies of messages with sendingFile attribute >------
  */
  std::function<Msg(Msg)> sendFile = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", msg.value("fileName"));
    reply.attribute("fileName", msg.value("fileName"));
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      std::string filePath = searchPath + "/" + msg.value("fileName");
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
	  std::cout << std::endl << "filePath : "<<filePath;
	  std::cout << std::endl << "fullSrcPath : "<<fullSrcPath;
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += "/" + msg.value("fileName");
	  reply.file(fullSrcPath);
	  
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----< analyze code on current server path >--------------------------
  /*
  *  - Creates process to run CodeAnalyzer on specified path
  *  - Won't return until analysis is done and logfile.txt
  *    is copied to sendFiles directory
  */
  std::function<Msg(Msg)> codeAnalyze = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", "logfile.txt");
    reply.attribute("fileName", "logfile.txt");
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      // run Analyzer using Process class

      Process p;
      p.title("test application");
      //std::string appPath = "c:/su/temp/project4sample/debug/CodeAnalyzer.exe";
      std::string appPath = "CodeAnalyzer.exe";
      p.application(appPath);

      //std::string cmdLine = "c:/su/temp/project4Sample/debug/CodeAnalyzer.exe ";
      std::string cmdLine = "CodeAnalyzer.exe ";
      cmdLine += searchPath + " ";
      cmdLine += "*.h *.cpp /m /r /f";
      //std::string cmdLine = "c:/su/temp/project4sample/debug/CodeAnalyzer.exe ../Storage/path *.h *.cpp /m /r /f";
      p.commandLine(cmdLine);

      std::cout << "\n  starting process: \"" << appPath << "\"";
      std::cout << "\n  with this cmdlne: \"" << cmdLine << "\"";

      CBP callback = []() { std::cout << "\n  --- child process exited ---"; };
      p.setCallBackProcessing(callback);

      if (!p.create())
      {
        std::cout << "\n  can't start process";
      }
      p.registerCallback();

      std::string filePath = searchPath + "\\" + /*msg.value("codeAnalysis")*/ "logfile.txt";
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += std::string("\\") + /*msg.value("codeAnalysis")*/ "logfile.txt";
	  reply.file(fullSrcPath);
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };
}


using namespace MsgPassingCommunication;

int main()
{
  SetConsoleTitleA("Project4Sample Server Console");

  std::cout << "\n  Testing Server Prototype";
  std::cout << "\n ==========================";
  std::cout << "\n";

  //StaticLogger<1>::attach(&std::cout);
  //StaticLogger<1>::start();

  sendFilePath = FileSystem::Directory::createOnPath("../RemoteFiles");
  saveFilePath = FileSystem::Directory::createOnPath("../LocalStorage");

  Server server(serverEndPoint, "ServerPrototype");

  // may decide to remove Context
  MsgPassingCommunication::Context* pCtx = server.getContext();
  pCtx->saveFilePath = saveFilePath;
  pCtx->sendFilePath = sendFilePath;

  server.start();
  
  std::cout << "\n  testing getFiles and getDirs methods";
  std::cout << "\n --------------------------------------";
  Files files = server.getFiles();
  show(files, "Files:");
  Dirs dirs = server.getDirs();
  show(dirs, "Dirs:");
  std::cout << "\n";

  std::cout << "\n  testing message processing";
  std::cout << "\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("sendFile", sendFile);
  server.addMsgProc("codeAnalyze", codeAnalyze);
  server.addMsgProc("convertfiles", convertfiles);
  server.addMsgProc("serverQuit", echo);
  server.addMsgProc("getCmd", getCmd);
  server.addMsgProc("DownLoad", DownLoad);
  server.addMsgProc("getRemoteFiles", getRemoteFiles);
  server.addMsgProc("singleDownload", singleDownload);

  server.processMessages();
  
  Msg msg(serverEndPoint, serverEndPoint);  // send to self
  msg.name("msgToSelf");

  /////////////////////////////////////////////////////////////////////
  // Additional tests here, used during development
  //
 // msg.command("echo");
  //msg.attribute("verbose", "show me");
  //server.postMessage(msg);
  //std::this_thread::sleep_for(std::chrono::milliseconds(1000));

 // msg.command("getFiles");
  //msg.remove("verbose");
  //msg.attributes()["path"] = storageRoot;
  //server.postMessage(msg);
  //std::this_thread::sleep_for(std::chrono::milliseconds(1000));

  //msg.command("getDirs");
  //msg.attributes()["path"] = storageRoot;
  //server.postMessage(msg);
  //std::this_thread::sleep_for(std::chrono::milliseconds(1000));

  std::cout << "\n  press enter to exit\n";
  std::cin.get();
  std::cout << "\n";

  msg.command("serverQuit");
  server.postMessage(msg);
  server.stop();
  
  return 0;
}

