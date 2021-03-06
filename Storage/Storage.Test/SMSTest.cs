﻿using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCloud;
using LeanCloud.Storage;

namespace Storage.Test {
    public class SMSTest {
        [SetUp]
        public void SetUp() {
            LCLogger.LogDelegate += Utils.Print;
            LCApplication.Initialize(Utils.AppId, Utils.AppKey, Utils.AppServer);
        }

        [TearDown]
        public void TearDown() {
            LCLogger.LogDelegate -= Utils.Print;
        }

        //[Test]
        public async Task RequestSMS() {
            await LCSMSClient.RequestSMSCode("15101006007",
                template: "test_template",
                signature: "flutter-test",
                variables: new Dictionary<string, object> {
                    { "k1", "v1" }
                });
        }

        //[Test]
        public async Task RequestVoice() {
            await LCSMSClient.RequestVoiceCode("+8615101006007");
        }

        //[Test]
        public async Task Verify() {
            await LCSMSClient.VerifyMobilePhone("15101006007", "");
        }
    }
}
