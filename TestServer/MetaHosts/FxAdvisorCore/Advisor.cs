using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxAdvisorCore;
using System.Threading;
using FxAdvisorCore.Interface;
using Common;
using System.Reflection;
using FxAdvisorCore.Logging;
using Log4Smart;
using System.IO;

namespace FxAdvisorCore
{
    public abstract class SimpleAdvisor : AdvisorProxy
    {
        protected const string ReadOnlyParamName = "ReadOnly";
        protected StrategyParameter param = null;
        protected IMeta Meta { get; private set; }
        protected IOrderOperation OrderOperation{get;private set;}
        protected IAccount Account { get; private set; }
        protected IHistory History { get; private set; }
        protected IRegisterTickToHandle RegisterTickToHandle { get; private set; }
        protected int _maxTicks = 0;
        protected int _currentTicks = 0;
        protected string Symbol { get; private set; }
        protected bool TestingMode { get; set; }
        protected IStrategyLogger logger;

        internal override void OnRemmoteAdvisorInit(RemoteAdvisor rmAdviser)
        {
            try
            {
                Meta = rmAdviser;
                ForexAPI fAPI = new ForexAPI(Meta, rmAdviser.MagicNumber);

                OrderOperation = fAPI;
                Account = fAPI;
                History = fAPI;
                this.Symbol = rmAdviser.Symbol();

                if (this.param != null)
                    throw new ApplicationException("Param was initalized");
                this.param = new StrategyParameter();

                foreach (string paramKey in rmAdviser.Parameters.Keys)
                {
                    if (paramKey == "NewsFilePath")
                    {
                        this.param.NewsFilePath = rmAdviser.Parameters[paramKey];
                        continue;
                    }
                    if (paramKey.EndsWith("String"))
                    {
                        this.param.Add(paramKey, rmAdviser.Parameters[paramKey]);
                        continue;
                    }
                    if (paramKey == "InvertedMetaStrategy")
                    {
                        rmAdviser.IsInverted = rmAdviser.Parameters[paramKey] == "1" || rmAdviser.Parameters[paramKey] == "true";
                        continue;
                    }

                    this.param[paramKey] = Int32.Parse(rmAdviser.Parameters[paramKey]);
                }


                this.logger = new MetaStrategyLogger(Meta);

                onStart(0);
                this.Meta.Print("onStart finidshed successfully");
            }
            catch (Exception exc)
            {
                string excString = "Start up exception: " + exc.ToString();
                Meta.Comment(excString);
                Meta.Print(excString);
            }

        }
        internal override void OnRemmoteAdvisorDeinit(RemoteAdvisor rmAdviser)
        {
            onEnd();
        }
        internal override void OnRemoteAdvisorStart(RemoteAdvisor rmAdviser)
        {
            try
            {
                Tick<int> newTick = new Tick<int>();
                newTick.Ask = Convertor.TranslateToPipPrice(rmAdviser.Symbol(), rmAdviser.Ask);
                newTick.Bid = Convertor.TranslateToPipPrice(rmAdviser.Symbol(), rmAdviser.Bid);
                newTick.DateTime = rmAdviser.LastTickDateTime;

                UpdateProperties();

                onTick(newTick);
                _currentTicks++;
            }
            catch (Exception exc)
            {
                string excString = "Exception: " + exc.ToString();
                Meta.Comment(excString);
                //Meta.Print("excString");
            }
        }

        public bool TesterStart(int ticks, IOrderOperation OrderOperation, IAccount Account, IHistory History, IMeta Meta, IRegisterTickToHandle registerTickToHandle, string symbol)
        {
            _maxTicks = ticks;
            this.OrderOperation = OrderOperation;
            this.Account = Account;
            this.History = History;
            this.Meta = Meta;
            this.Symbol = symbol;
            this.RegisterTickToHandle = registerTickToHandle;
            return onStart(ticks);
        }
        public void TesterTick(Tick<int> tick)
        {
            onTick(tick);
            _currentTicks++;
        }
        public void TesterEnd()
        {
            onEnd();
            if( logger != null )
                this.logger.AddMessage("{0}", this.Account.Account.Statistics.ToString());
        }
        public bool TesterInit(StrategyParameter param, IStrategyLogger logger)
        {
            TestingMode = true;
            this.param = param;
            this.logger = logger;

            return true;
        }

        public virtual bool onStart(int ticks)
        {
            return true;
        }
        public abstract void onTick(Tick<int> tick);
        public virtual void onEnd()
        {
        }

        public bool SetReadOnly(bool value)
        {
            if (param.Contains(ReadOnlyParamName))
            {
                param[ReadOnlyParamName] = value ? 1 : 0;
                return true;
            }
            return false;
        }
        protected void UpdateProperties()
        {
            foreach (System.Reflection.PropertyInfo currPropertyInfo in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (currPropertyInfo.GetCustomAttributes(typeof(AutoUpdatableAttribute), true).Length > 0)
                {
                    Order order = currPropertyInfo.GetValue(this, null) as Order;
                    if (order != null)
                    {
                        Order updatedOrder = this.OrderOperation.GetOrderByID(order.ID);
                        if (updatedOrder == null)
                            this.History.GetOrderByIDFromHistory(order.ID);
                        if (updatedOrder == null)
                            throw new ApplicationException("Can`t find order with ticket = " + order.ID.ToString());

                        currPropertyInfo.SetValue(this, updatedOrder, null);
                    }
                }
            }
        }

        #region Comment
        StringBuilder tickComment2 = new StringBuilder();
        bool IsWasChanged = false;
        int previousID;
        string previousIDstring;
        StreamWriter sWriter = new StreamWriter("output"+DateTime.UtcNow.Ticks.ToString(), true);
        StringBuilder TickComment
        {
            get
            {
                IsWasChanged = true;
                return tickComment2;
            }
        } 

        protected void InitComment()
        {
            if (TestingMode)
                return;
            TickComment.Clear();
            previousID = -1;
            previousIDstring = string.Empty;
            TickComment.Append(param.ToString());
            IsWasChanged = false;
        }
        protected void AddComment(string stringParam)
        {
            if (TestingMode)
                return;
            TickComment.Append(stringParam);
        }
        protected void AddComment(int id, string stringParam)
        {
            if (TestingMode)
                return;
            if (previousID != id)
            {
                TickComment.AppendLine();
                previousID = id;
                TickComment.Append(id.ToString() + ": ");
            }
            TickComment.Append(stringParam + " ");
        }
        protected void AddComment(string id, string stringParam)
        {
            if (TestingMode)
                return;
            if (previousIDstring != id)
            {
                TickComment.AppendLine();
                previousIDstring = id;
                TickComment.Append(id.ToString() + ": ");
            }
            TickComment.Append(stringParam + " ");
        }
        protected void AddCommentLine(string id, string stringParam)
        {
            AddComment(id, stringParam + Environment.NewLine);
        }
        protected void AddComment(Func<string> stringParam)
        {
            if (TestingMode)
                return;
            TickComment.Append(stringParam.Invoke());
        }
        protected void ShowComment()
        {
            if (TestingMode)
                return;
            if (!IsWasChanged)
                return;
            for( int i=0;i<20;i++)
                TickComment.AppendLine();
            Meta.Comment(TickComment.ToString());
            sWriter.WriteLine(TickComment.ToString());
        }
        #endregion


    }

    public abstract class AdvisorProxy : MarshalByRefObject, IAdvisorProxy
    {
        private object _syncAdvisorCollection = new object();
        private IDictionary<int, RemoteAdvisor> rmAdvisors = new Dictionary<int, RemoteAdvisor>();
        private int idSeed = 1;

        #region Abstract
        internal abstract void OnRemmoteAdvisorInit(RemoteAdvisor rmAdviser);
        internal abstract void OnRemmoteAdvisorDeinit(RemoteAdvisor rmAdviser);
        internal abstract void OnRemoteAdvisorStart(RemoteAdvisor rmAdviser);
        #endregion Abstract

        private object GetCommandParam(int token)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor == null || rmAdvisor.Command == null)
                return null;
            return rmAdvisor.Command.getNextParam();
        }

        protected virtual RemoteAdvisor CreateRemoteAdvisor()
        {
            return new RemoteAdvisor();
        }

        #region IAdvisor Members

        int IAdvisorProxy.StartSession(string markeId)
        {
            //todo update for external advisor
            lock (_syncAdvisorCollection)
            {
                int id = idSeed++;
                RemoteAdvisor rmAdvisor = CreateRemoteAdvisor();
                rmAdvisor.MarketId = markeId;
                id = rmAdvisor.Init(this);
                rmAdvisors[id] = rmAdvisor;
                return id;
            }
        }

        void IAdvisorProxy.EndSession(int token)
        {
            lock (_syncAdvisorCollection)
            {
                RemoteAdvisor rmAdvisor = rmAdvisors[token];
                if (rmAdvisor != null)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(rmAdvisor.AsyncDeinit));
                    rmAdvisors.Remove(token);
                }
            }
        }

        void IAdvisorProxy.OnTick(int token, double bid, double ask, int dateTime)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null)
            {
                rmAdvisor.Ask = ask;
                rmAdvisor.Bid = bid;
                rmAdvisor.LastTickDateTime = Convertor.SecondsToDateTime(dateTime);
                ThreadPool.QueueUserWorkItem(new WaitCallback(rmAdvisor.AsyncStart));
            }
        }

        int IAdvisorProxy.GetNextCommand(int token)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null && rmAdvisor.Command != null)
            {
                return (int)rmAdvisor.Command.Name;
            }
            return -2;
        }

        int IAdvisorProxy.GetCommandParamCount(int token)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null && rmAdvisor.Command != null)
            {
                return (int)rmAdvisor.Command.ParamCount;
            }
            return -2;
        }

        double IAdvisorProxy.GetDoubleCmdParam(int token)
        {
            return (double)GetCommandParam(token);
        }

        int IAdvisorProxy.GetIntCmdParam(int token)
        {
            return (int)GetCommandParam(token);
        }

        string IAdvisorProxy.GetStringCmdParam(int token)
        {
            return (string)GetCommandParam(token);
        }

        void IAdvisorProxy.SetCmdResult(int token, object result)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null)
            {
                rmAdvisor.SetCommandResult(result);
            }
        }

        void IAdvisorProxy.SetInit(int token)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(rmAdvisor.AsyncInit));
            }
        }

        void IAdvisorProxy.SetParameter(int token, string key, string value)
        {
            RemoteAdvisor rmAdvisor = rmAdvisors[token];
            if (rmAdvisor != null )
            {
                rmAdvisor.SetParameter(key, value);
            }
        }

        #endregion

        public long Operations
        {
            get
            {
                lock(_syncAdvisorCollection)
                {
                    long result = 0;
                    foreach (RemoteAdvisor adv in this.rmAdvisors.Values)
                        result += adv.Operations;
                    return result;
                }
            }
        }

        public IEnumerable<RemoteAdvisor> FindByMarketId(string marketId)
        {
            lock (_syncAdvisorCollection)
            {
                foreach (RemoteAdvisor rmAdvisor in this.rmAdvisors.Values)
                    if (rmAdvisor.MarketId == marketId)
                        yield return rmAdvisor;
            }
        }
    }
}
