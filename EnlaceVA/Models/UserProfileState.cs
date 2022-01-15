// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;

namespace EnlaceVA.Models
{
    public class UserProfileState
    {
        public string Name { get; set; }
        public bool DidBotWelcomeUser { get; set; } = false;
        public int numberErrorMessage { get; set; } = 0;
        public int countFailContract { get; set; } = 0;
        public bool groupQuestion { get; set; } = true;        
        public List<Conversation>  Conversations { get; set; }
        public string Dialogturn { get; set; }
        public string Company { get; set; }
        public string CompanyName { get; set; }
        public string ContactMessage { get; set; }
        public bool MenuStarted { get; set; }
        public bool SubMenuStarted { get; set; }
        public bool SubMenuFind { get; set; }

        public string[] LastSubMenu { get; set; }
        public string[] LastMenu { get; set; }
        public int SubMenustep { get; set; }
        public bool MenuFinished { get; set; }
        public string ChannelId { get; set; }
        public DateTime lastUpdate { get; set; }
        public Utilities.MenuSubMenuEnum.userState UserState { get; set; }
        public bool WorkStarted { get; set; }
        public bool TerminateWork { get; set; }
        public string TaskState { get; set; }
        public bool ConversationStarted { get; set; }
        public int Delay { get; set; }
        public string conversationId { get; set; }
        public string PrincipalDialog { get; set; }
    }
}
