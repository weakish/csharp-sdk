﻿using System.Collections.Generic;
using Newtonsoft.Json;
using LeanCloud.Storage.Internal.Codec;
using LeanCloud.Storage.Internal;

namespace LeanCloud.Realtime {
    public abstract class LCIMTypedMessage : LCIMMessage {
        private Dictionary<string, object> customProperties;

        internal virtual int MessageType {
            get; private set;
        }

        public object this[string key] {
            get {
                if (customProperties == null) {
                    return null;
                }
                return customProperties[key];
            }
            set {
                if (customProperties == null) {
                    customProperties = new Dictionary<string, object>();
                }
                customProperties[key] = value;
            }
        }

        protected LCIMTypedMessage() {
        }

        internal virtual Dictionary<string, object> Encode() {
            Dictionary<string, object> msgData = new Dictionary<string, object> {
                { "_lctype", MessageType }
            };
            if (customProperties != null && customProperties.Count > 0) {
                msgData["_lcattrs"] = LCEncoder.Encode(customProperties);
            }
            return msgData;
        }

        protected virtual void DecodeMessageData(Dictionary<string, object> msgData) {
            MessageType = (int)msgData["_lctype"];
            if (msgData.TryGetValue("_lcattrs", out object attrObj)) {
                customProperties = LCDecoder.Decode(attrObj) as Dictionary<string, object>;
            }
        }

        internal static LCIMTypedMessage Deserialize(string json) {
            Dictionary<string, object> msgData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json,
                new LCJsonConverter());
            LCIMTypedMessage message = null;
            int msgType = (int)msgData["_lctype"];
            switch (msgType) {
                case TextMessageType:
                    message = new LCIMTextMessage();
                    break;
                case ImageMessageType:
                    message = new LCIMImageMessage();
                    break;
                case AudioMessageType:
                    message = new LCIMAudioMessage();
                    break;
                case VideoMessageType:
                    message = new LCIMVideoMessage();
                    break;
                case LocationMessageType:
                    message = new LCIMLocationMessage();
                    break;
                case FileMessageType:
                    message = new LCIMFileMessage();
                    break;
                case RecalledMessageType:
                    message = new LCIMRecalledMessage();
                    break;
                default:
                    // TODO 用户自定义类型消息

                    break;
            }
            message.DecodeMessageData(msgData);
            return message;
        }
    }
}
