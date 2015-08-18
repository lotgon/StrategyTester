#pragma once
using namespace System;
using namespace FxAdvisorCore::Interface;

ref class QuotesManager
{
	QuotesManager(void);

internal:
	Client^ _client;
public: 

	#pragma region Singleton mess
        /// Return an instance of <see cref="Singleton"/>
	public:
		static property QuotesManager^ Instance
        {
			QuotesManager^ get();
        }
        /// Sealed class to avoid any heritage from this helper class
	private:

		ref class SingletonCreator
        {
            // Retrieve a single instance of a Singleton
		private:
			static QuotesManager^ _instance = gcnew QuotesManager();

		public:
            /// Return an instance of the class <see cref="Singleton"/>
            property static QuotesManager^ CreatorInstance{QuotesManager^ get();}
        };
#pragma endregion 

};
