using System;
using Tibia.Packets;

namespace Tibia.Objects
{
    /// <summary>
    /// A message in Tibia.
    /// </summary>
    public struct ChatMessage
    {
        public string Text;
        public string Recipient;
        public Packets.ChatChannel Channel;
        public Packets.ChatType Type;

        /// <summary>
        /// Create a default message.
        /// </summary>
        /// <param name="text"></param>
        public ChatMessage(string text)
        {
            this.Text = text;
            this.Recipient = "";
            this.Channel = Packets.ChatChannel.None;
            this.Type = Packets.ChatType.Normal;
        }

        /// <summary>
        /// Create a private message.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="recipient"></param>
        public ChatMessage(string text, string recipient)
        {
            this.Text = text;
            this.Recipient = recipient;
            this.Channel = Packets.ChatChannel.None;
            this.Type = Packets.ChatType.PrivateMessage;
        }

        /// <summary>
        /// Create a channel message.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="channel"></param>
        public ChatMessage(string text, Packets.ChatChannel channel)
        {
            this.Text = text;
            this.Recipient = "";
            this.Channel = channel;
            this.Type = Packets.ChatType.ChannelNormal;
        }

        /// <summary>
        /// Create a yell or whisper message.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public ChatMessage(string text, Packets.ChatType type)
        {
            this.Text = text;
            this.Recipient = "";
            this.Channel = Packets.ChatChannel.None;
            this.Type = type;
        }
    }
}
