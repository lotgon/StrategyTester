using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorCore.Interface
{
    public interface IAdvisorProxy
    {
        int StartSession(string marketId);
        void EndSession(int token);

        void OnTick(int token, double ask, double bid, int dateTime);

        int GetNextCommand(int token);
        double GetDoubleCmdParam(int token);
        int GetIntCmdParam(int token);
        string GetStringCmdParam(int token);
        void SetCmdResult(int token, object result);
        int GetCommandParamCount(int token);

        void SetInit(int token);
        void SetParameter(int token, string key, string value);
    }
}
