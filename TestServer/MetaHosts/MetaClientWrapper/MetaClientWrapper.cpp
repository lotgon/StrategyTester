// This is the main DLL file.

#include "stdafx.h"

#include "MetaClientWrapper.h"
#include "QuotesManager.h"
using namespace System::Runtime::InteropServices;


 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_Connect(char * uri, char *marketId) 
 {
	 try
	 {
		return QuotesManager::Instance->_client->Connect(gcnew String(uri), gcnew String(marketId));
	 }
 	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_Connect", ex);
		 return -1;
	 }
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_GetCommand(int token) 
 {
	 try
	 {
		 int command = QuotesManager::Instance->_client->GetAdvisorProxy(token)->GetNextCommand(token);
		 return command;
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetCommand", ex);
		 return -1;
	 }
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_OnTick(int token, double bid, double ask, int dateTime) 
 {
	 try
	 {
		QuotesManager::Instance->_client->GetAdvisorProxy(token)->OnTick(token, bid, ask, dateTime);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_OnTick", ex);
		 return -1;
	 }
	 
	 return 1;
 }
 
 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetCmdResultD(int token, double result) 
 {
	 try
	 {
		QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetCmdResult(token, result);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_SetCmdResultD", ex);
		 return -1;
	 }
	 return 1;
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetCmdResultI(int token, int result) 
 {
	 try
	 {
		QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetCmdResult(token, result);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_SetCmdResultI", ex);
		 return -1;
	 }
	 return 1;
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetCmdResultB(int token, int result) 
 {
	 try
	 {
		QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetCmdResult(token, result!=0);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_SetCmdResultB", ex);
		 return -1;
	 }
	 return 1;
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetCmdResultS(int token, char* result) 
 {
	try
	 {
		QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetCmdResult(token, gcnew String(result));
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_SetCmdResultS", ex);
		 return -1;
	 }
	 return 1;
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_GetIntCmdParam(int token) 
 {
	 try
	 {
		 return QuotesManager::Instance->_client->GetAdvisorProxy(token)->GetIntCmdParam(token);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_GetStrCmdParam(int token, char* buffer, int bufferLen) 
 {
	 try
	 {
		 String^ param = QuotesManager::Instance->_client->GetAdvisorProxy(token)->GetStringCmdParam(token);
		 MarshalStr(param, buffer, bufferLen);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
	 return 1;
 }

  extern "C"__declspec(dllexport) double __stdcall FxAdvisor_GetDoubleCmdParam(int token)
 {
	 try
	 {
		 return QuotesManager::Instance->_client->GetAdvisorProxy(token)->GetDoubleCmdParam(token);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
 }

 //extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetStartupParameters(int token, char* parameters, char *additionalParameters1, char *additionalParameters2, char* additionalParameters3) 
 //{
	// try
	// {
	//	 QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetStartupParameters(token, gcnew String(parameters));
	// }
	// catch(Exception^ ex)
	// {
	//	 QuotesManager::Instance->_client->LogError("FxAdvisor_SetParameters", ex);
	//	 return -1;
	// }
	// return 1;
 //}

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_SetParameter(int token, char* name, char* value) 
 {
	 try
	 {
		 QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetParameter(token, gcnew String(name), gcnew String(value));
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
	 return 1;
 }
 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_Init(int token) 
 {
	 try
	 {
		 QuotesManager::Instance->_client->GetAdvisorProxy(token)->SetInit(token);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
	 return 1;
 }

 extern "C"__declspec(dllexport) int __stdcall FxAdvisor_GetCommandParamCount(int token) 
 {
	 try
	 {
		 return QuotesManager::Instance->_client->GetAdvisorProxy(token)->GetCommandParamCount(token);
	 }
	 catch(Exception^ ex)
	 {
		 QuotesManager::Instance->_client->LogError("FxAdvisor_GetIntCmdParam", ex);
		 return -1;
	 }
 }


 void MarshalStr(String^ str, char* buffer, int bufferLen)
 {
	 void* temp = Marshal::StringToHGlobalAnsi(str).ToPointer();
	 errno_t result = strcpy_s(buffer, bufferLen, (const char*)temp) ;
	 Marshal::FreeHGlobal(IntPtr(temp));
 }