// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EnlaceVA.TokenExchange;
using Microsoft.Bot.Solutions;

namespace EnlaceVA.Services
{
    public class BotSettings : BotSettingsBase
    {
        public TokenExchangeConfig TokenExchangeConfig { get; set; }
    }
}