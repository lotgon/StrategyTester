#include "StdAfx.h"
#include "QuotesManager.h"

QuotesManager::QuotesManager(void)
{
	_client = gcnew Client();
}

#pragma region Singleton mess

QuotesManager^ QuotesManager::Instance::get()
{
	return SingletonCreator::CreatorInstance;
}
QuotesManager^ QuotesManager::SingletonCreator::CreatorInstance::get()
{
	return _instance;
}
#pragma endregion 