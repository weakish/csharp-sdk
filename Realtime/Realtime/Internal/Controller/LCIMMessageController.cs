﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Google.Protobuf;
using LeanCloud.Realtime.Internal.Protocol;

namespace LeanCloud.Realtime.Internal.Controller {
    internal class LCIMMessageController : LCIMController {
        internal LCIMMessageController(LCIMClient client) : base(client) {

        }

        #region 内部接口

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal async Task<LCIMMessage> Send(string convId,
            LCIMMessage message,
            LCIMMessageSendOptions options) {
            DirectCommand direct = new DirectCommand {
                FromPeerId = Client.Id,
                Cid = convId,
            };
            if (message is LCIMTypedMessage typedMessage) {
                direct.Msg = JsonConvert.SerializeObject(typedMessage.Encode());
            } else if (message is LCIMBinaryMessage binaryMessage) {
                direct.BinaryMsg = ByteString.CopyFrom(binaryMessage.Data);
            } else {
                throw new ArgumentException("Message MUST be LCIMTypedMessage or LCIMBinaryMessage.");
            }
            // 暂态消息
            if (options.Transient) {
                direct.Transient = options.Transient;
            }
            // 消息接收回执
            if (options.Receipt) {
                direct.R = options.Receipt;
            }
            // 遗愿消息
            if (options.Will) {
                direct.Will = options.Will;
            }
            // 推送数据
            if (options.PushData != null) {
                direct.PushData = JsonConvert.SerializeObject(options.PushData);
            }
            // 提醒所有人
            if (message.MentionAll) {
                direct.MentionAll = message.MentionAll;
            }
            // 提醒用户列表
            if (message.MentionIdList != null &&
                message.MentionIdList.Count > 0) {
                direct.MentionPids.AddRange(message.MentionIdList);
            }
            GenericCommand command = NewCommand(CommandType.Direct);
            command.DirectMessage = direct;
            // 优先级
            if (command.Priority > 0) {
                command.Priority = (int)options.Priority;
            }
            GenericCommand response = await Connection.SendRequest(command);
            // 消息发送应答
            AckCommand ack = response.AckMessage;
            message.Id = ack.Uid;
            message.SentTimestamp = ack.T;
            message.FromClientId = Client.Id;
            return message;
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal async Task RecallMessage(string convId,
            LCIMMessage message) {
            PatchCommand patch = new PatchCommand();
            PatchItem item = new PatchItem {
                Cid = convId,
                Mid = message.Id,
                From = Client.Id,
                Recall = true,
                Timestamp = message.SentTimestamp,
                PatchTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
            patch.Patches.Add(item);
            GenericCommand request = NewCommand(CommandType.Patch, OpType.Modify);
            request.PatchMessage = patch;
            await Connection.SendRequest(request);
        }

        /// <summary>
        /// 修改消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="oldMessage"></param>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        internal async Task UpdateMessage(string convId,
            LCIMMessage oldMessage,
            LCIMMessage newMessage) {
            PatchCommand patch = new PatchCommand();
            PatchItem item = new PatchItem {
                Cid = convId,
                Mid = oldMessage.Id,
                From = Client.Id,
                Recall = false,
                Timestamp = oldMessage.SentTimestamp,
                PatchTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
            if (newMessage is LCIMTypedMessage typedMessage) {
                item.Data = JsonConvert.SerializeObject(typedMessage.Encode());
            } else if (newMessage is LCIMBinaryMessage binaryMessage) {
                item.BinaryMsg = ByteString.CopyFrom(binaryMessage.Data);
            }
            if (newMessage.MentionIdList != null) {
                item.MentionPids.AddRange(newMessage.MentionIdList);
            }
            if (newMessage.MentionAll) {
                item.MentionAll = newMessage.MentionAll;
            }
            patch.Patches.Add(item);
            GenericCommand request = NewCommand(CommandType.Patch, OpType.Modify);
            request.PatchMessage = patch;
            GenericCommand response = await Connection.SendRequest(request);
        }

        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="direction"></param>
        /// <param name="limit"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        internal async Task<ReadOnlyCollection<LCIMMessage>> QueryMessages(string convId,
            LCIMMessageQueryEndpoint start = null,
            LCIMMessageQueryEndpoint end = null,
            LCIMMessageQueryDirection direction = LCIMMessageQueryDirection.NewToOld,
            int limit = 20,
            int messageType = 0) {
            LogsCommand logs = new LogsCommand {
                Cid = convId
            };
            if (start != null) {
                logs.T = start.SentTimestamp;
                logs.Mid = start.MessageId;
                logs.TIncluded = start.IsClosed;
            }
            if (end != null) {
                logs.Tt = end.SentTimestamp;
                logs.Tmid = end.MessageId;
                logs.TtIncluded = end.IsClosed;
            }
            logs.Direction = direction == LCIMMessageQueryDirection.NewToOld ?
                LogsCommand.Types.QueryDirection.Old : LogsCommand.Types.QueryDirection.New;
            logs.Limit = limit;
            if (messageType != 0) {
                logs.Lctype = messageType;
            }
            GenericCommand request = NewCommand(CommandType.Logs, OpType.Open);
            request.LogsMessage = logs;
            GenericCommand response = await Connection.SendRequest(request);
            // 反序列化聊天记录
            return response.LogsMessage.Logs.Select(item => {
                LCIMMessage message;
                if (item.Bin) {
                    // 二进制消息
                    byte[] bytes = Convert.FromBase64String(item.Data);
                    message = LCIMBinaryMessage.Deserialize(bytes);
                } else {
                    // 类型消息
                    message = LCIMTypedMessage.Deserialize(item.Data);
                }
                message.ConversationId = convId;
                message.Id = item.MsgId;
                message.FromClientId = item.From;
                message.SentTimestamp = item.Timestamp;
                message.DeliveredTimestamp = item.AckAt;
                message.ReadTimestamp = item.ReadAt;
                message.PatchedTimestamp = item.PatchTimestamp;
                message.MentionAll = item.MentionAll;
                message.MentionIdList = item.MentionPids.ToList();
                message.Mentioned = message.MentionAll ||
                    message.MentionIdList.Contains(Client.Id);
                return message;
            }).ToList().AsReadOnly();
        }

        /// <summary>
        /// 确认收到消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        internal async Task Ack(string convId,
            string msgId) {
            AckCommand ack = new AckCommand {
                Cid = convId,
                Mid = msgId
            };
            GenericCommand command = NewCommand(CommandType.Ack);
            command.AckMessage = ack;
            await Connection.SendCommand(command);
        }

        /// <summary>
        /// 确认已读消息
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal async Task Read(string convId,
            LCIMMessage msg) {
            ReadCommand read = new ReadCommand();
            ReadTuple tuple = new ReadTuple {
                Cid = convId,
                Mid = msg.Id,
                Timestamp = msg.SentTimestamp
            };
            read.Convs.Add(tuple);
            GenericCommand command = NewCommand(CommandType.Read);
            command.ReadMessage = read;
            await Connection.SendCommand(command);
        }

        #endregion

        #region 消息处理

        internal override void HandleNotification(GenericCommand notification) {
            if (notification.Cmd == CommandType.Direct) {
                _ = OnMessaage(notification);
            } else if (notification.Cmd == CommandType.Patch) {
                _ = OnMessagePatched(notification);
            } else if (notification.Cmd == CommandType.Rcp) {
                _ = OnMessageReceipt(notification);
            }
        }

        /// <summary>
        /// 接收消息事件
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private async Task OnMessaage(GenericCommand notification) {
            DirectCommand direct = notification.DirectMessage;
            // 反序列化消息
            LCIMMessage message;
            if (direct.HasBinaryMsg) {
                // 二进制消息
                byte[] bytes = direct.BinaryMsg.ToByteArray();
                message = LCIMBinaryMessage.Deserialize(bytes);
            } else {
                // 类型消息
                message = LCIMTypedMessage.Deserialize(direct.Msg);
            }
            // 填充消息数据
            message.ConversationId = direct.Cid;
            message.Id = direct.Id;
            message.FromClientId = direct.FromPeerId;
            message.SentTimestamp = direct.Timestamp;
            message.MentionAll = direct.MentionAll;
            message.MentionIdList = direct.MentionPids.ToList();
            message.Mentioned = message.MentionAll ||
                message.MentionIdList.Contains(Client.Id);
            message.PatchedTimestamp = direct.PatchTimestamp;
            message.IsTransient = direct.Transient;
            // 通知服务端已接收
            if (!message.IsTransient) {
                // 只有非暂态消息才需要发送 ack
                _ = Ack(message.ConversationId, message.Id);
            }
            // 获取对话
            LCIMConversation conversation = await Client.GetOrQueryConversation(direct.Cid);
            conversation.Unread++;
            conversation.LastMessage = message;
            Client.OnMessage?.Invoke(conversation, message);
        }

        /// <summary>
        /// 消息被修改事件
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private async Task OnMessagePatched(GenericCommand notification) {
            PatchCommand patchMessage = notification.PatchMessage;
            foreach (PatchItem patch in patchMessage.Patches) {
                // 获取对话
                LCIMConversation conversation = await Client.GetOrQueryConversation(patch.Cid);
                LCIMMessage message;
                if (patch.HasBinaryMsg) {
                    byte[] bytes = patch.BinaryMsg.ToByteArray();
                    message = LCIMBinaryMessage.Deserialize(bytes);
                } else {
                    message = LCIMTypedMessage.Deserialize(patch.Data);
                }
                message.ConversationId = patch.Cid;
                message.Id = patch.Mid;
                message.FromClientId = patch.From;
                message.SentTimestamp = patch.Timestamp;
                message.PatchedTimestamp = patch.PatchTimestamp;
                if (message is LCIMRecalledMessage recalledMessage) {
                    // 消息撤回
                    Client.OnMessageRecalled?.Invoke(conversation, recalledMessage);
                } else {
                    // 消息修改
                    Client.OnMessageUpdated?.Invoke(conversation, message);
                }
            }
        }

        /// <summary>
        /// 消息回执事件
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private async Task OnMessageReceipt(GenericCommand notification) {
            RcpCommand rcp = notification.RcpMessage;
            string convId = rcp.Cid;
            string msgId = rcp.Id;
            long timestamp = rcp.T;
            bool isRead = rcp.Read;
            string fromId = rcp.From;
            LCIMConversation conversation = await Client.GetOrQueryConversation(convId);
            if (isRead) {
                Client.OnMessageRead?.Invoke(conversation, msgId);
            } else {
                Client.OnMessageDelivered?.Invoke(conversation, msgId);
            }
        }

        #endregion
    }
}
