﻿using ColossalFramework;
using CSM.Commands;
using CSM.Networking;
using ICities;
using System.Reflection;

namespace CSM.Extensions
{
    /// <summary>
    /// Handles game economy. Sends the MoneyAmount between Server and Client
    /// Also sets the income and the expences to 0 on the client side, making the server handling income and expenses
    ///
    /// TODO: The UI keeps track of the income and expenses by using the private arrays m_totalIncome and m_totalExpanses in EconomyManager
    /// To get the income showing on the client UI these have to be send and copied to the client side, right now income and expences just show 0.
    ///
    /// </summary>
    public class EconomyExtension : EconomyExtensionBase
    {
        private long _lastMoneyAmount;

        public override long OnUpdateMoneyAmount(long internalMoneyAmount) //function that checks if the money updates
        {
            if (_lastMoneyAmount != (long)typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<EconomyManager>.instance))
            {
                switch (MultiplayerManager.Instance.CurrentRole)
                {
                    case MultiplayerRole.Client:
                        MultiplayerManager.Instance.CurrentClient.SendToServer(CommandBase.MoneyCommandID, new MoneyCommand
                        {
                            InternalMoneyAmount = (long)typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<EconomyManager>.instance)
                        });
                        break;

                    case MultiplayerRole.Server:
                        MultiplayerManager.Instance.CurrentServer.SendToClients(CommandBase.MoneyCommandID, new MoneyCommand
                        {
                            InternalMoneyAmount = (long)typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<EconomyManager>.instance)
                        });
                        break;
                }
            }
            _lastMoneyAmount = (long)typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<EconomyManager>.instance);
            return (internalMoneyAmount);
        }

        public override int OnAddResource(EconomyResource resource, int amount, Service service, SubService subService, Level level)
        {
            switch (MultiplayerManager.Instance.CurrentRole)
            {
                case MultiplayerRole.Client:
                    {
                        typeof(EconomyManager).GetField("m_taxMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Singleton<EconomyManager>.instance, 0);
                        break;
                    }
            }
            return amount;
        }

        public override int OnGetMaintenanceCost(int originalMaintenanceCost, Service service, SubService subService, Level level)
        {
            switch (MultiplayerManager.Instance.CurrentRole)
            {
                case MultiplayerRole.Client:
                    {
                        return 0;
                    }
                case MultiplayerRole.Server:
                    {
                        return originalMaintenanceCost;
                    }
            }
            return originalMaintenanceCost;
        }
    }
}