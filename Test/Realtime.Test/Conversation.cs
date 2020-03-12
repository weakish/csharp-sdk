﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCloud;
using LeanCloud.Common;
using LeanCloud.Realtime;

namespace Realtime.Test {
    public class Conversation {
        [SetUp]
        public void SetUp() {
            LCLogger.LogDelegate += Utils.Print;
            LCApplication.Initialize("ikGGdRE2YcVOemAaRbgp1xGJ-gzGzoHsz", "NUKmuRbdAhg1vrb2wexYo1jo", "https://ikggdre2.lc-cn-n1-shared.com");
        }

        [TearDown]
        public void TearDown() {
            LCLogger.LogDelegate -= Utils.Print;
        }

        [Test]
        public async Task CreateConversation() {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            string clientId = Guid.NewGuid().ToString();
            LCIMClient client = new LCIMClient(clientId);

            await client.Open();

            client.OnInvited = (conv, initBy) => {
                TestContext.WriteLine($"on invited: {initBy}");
            };

            client.OnMembersJoined = (conv, memberList, initBy) => {
                TestContext.WriteLine($"on members joined: {initBy}");
                tcs.SetResult(null);
            };

            List<string> memberIdList = new List<string> { "world" };
            string name = Guid.NewGuid().ToString();
            await client.CreateConversation(memberIdList, name: name, unique: false);

            await tcs.Task;
        }

        [Test]
        public async Task CreateChatRoom() {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            string clientId = Guid.NewGuid().ToString();
            LCIMClient client = new LCIMClient(clientId);

            await client.Open();

            client.OnInvited = (conv, initBy) => {
                TestContext.WriteLine($"on invited: {initBy}");
                tcs.SetResult(null);
            };

            string name = Guid.NewGuid().ToString();
            await client.CreateChatRoom(name);

            await tcs.Task;
        }

        [Test]
        public async Task CreateTemporaryConversation() {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            string clientId = Guid.NewGuid().ToString();
            LCIMClient client = new LCIMClient(clientId);

            await client.Open();

            client.OnInvited = (conv, initBy) => {
                TestContext.WriteLine($"on invited: {initBy}");
            };

            client.OnMembersJoined = (conv, memberList, initBy) => {
                TestContext.WriteLine($"on members joined: {initBy}");
                tcs.SetResult(null);
            };

            List<string> memberIdList = new List<string> { "world" };
            await client.CreateTemporaryConversation(memberIdList);

            await tcs.Task;
        }
    }
}
