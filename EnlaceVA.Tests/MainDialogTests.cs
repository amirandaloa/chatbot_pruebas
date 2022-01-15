// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EnlaceVA.Tests.Utterances;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnlaceVA.Tests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class MainDialogTests : BotTestBase
    {
        [TestMethod]
        public async Task Test_Intro_Message()
        {
            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_Feedback_Intent()
        {
            var RejectPromptVariations = AllResponsesTemplates.ExpandTemplate("CompletedMessage");
            var feedBackPromptVariations = AllResponsesTemplates.ExpandTemplate("FeedBackReceived");
            
            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Question)
                .AssertReply(activity => Assert.AreEqual(GeneralUtterances.QnAAnswer, activity.AsMessageActivity().Text))
                .AssertReply(activity => Assert.AreEqual(RejectPromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .Send(GeneralUtterances.Reject)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.FeedbackScore)
                .AssertReply(activity => Assert.AreEqual(feedBackPromptVariations.FirstOrDefault().ToString(), activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_ChitChat_Message()
        {
            

            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Hello)
                .AssertReply(activity => Assert.AreEqual("Hello.", activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_QnA_Message()
        {
            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Question)
                .AssertReply(activity => Assert.AreEqual(GeneralUtterances.QnAAnswer, activity.AsMessageActivity().Text))
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_MenuySubMenu_Dialog()
        {
            await GetTestFlow()
                .Send(new Activity()
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>() { new ChannelAccount(GeneralUtterances.CompanyTest) }
                })
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.Menu)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .Send(GeneralUtterances.MenuBrilla)
                .AssertReply(activity => Assert.AreEqual(1, activity.AsMessageActivity().Attachments.Count))
                .StartTestAsync();
        }
    }
}