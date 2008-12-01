using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tibia.Packets;

namespace Tibia_Chat
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        ObservableCollection<CharListChar> _mycharacters = new ObservableCollection<CharListChar>();

        Socket CharListSock;
        Socket GameLoginSock;
        Queue<byte[]> serverReceiveQueue = new Queue<byte[]>();
        string AccName;
        string AccPass;
        byte[] dataLoginServer = new byte[8192];
        byte[] dataGameServer = new byte[8192];
        byte[] xtea = new byte[16];
        byte[] CharLoginPacket;
        byte[] GameLoginPacket;
        byte[] CharListDecrypted;
        int loginsvindex;
        int charindex;
        int tempLen = 0;
        int partialRemaining = 0;
        PacketBuilder partial;
        bool loggedin=false;
        private LoginServer[] loginServers = new LoginServer[] {
            new LoginServer("login01.tibia.com", 7171),
            new LoginServer("login02.tibia.com", 7171),
            new LoginServer("login03.tibia.com", 7171),
            new LoginServer("login04.tibia.com", 7171),
            new LoginServer("login05.tibia.com", 7171),
            new LoginServer("tibia01.cipsoft.com", 7171),
            new LoginServer("tibia02.cipsoft.com", 7171),
            new LoginServer("tibia03.cipsoft.com", 7171),
            new LoginServer("tibia04.cipsoft.com", 7171),
            new LoginServer("tibia05.cipsoft.com", 7171)
        };
        System.Threading.Timer pingTimer;

        #region Events
        /// <summary>
        /// A generic function prototype for packet events.
        /// </summary>
        /// <param name="packet">The unencrypted packet that was received.</param>
        /// <returns>true to continue forwarding the packet, false to drop the packet</returns>
        public delegate bool PacketListener(Packet packet);

        /// <summary>
        /// A function prototype for proxy notifications.
        /// </summary>
        /// <returns></returns>
        public delegate void ProxyNotification(string message);

        /// <summary>
        /// Called when the client has logged in.
        /// </summary>
        public ProxyNotification OnLogIn;

        /// <summary>
        /// Called when the client has logged out.
        /// </summary>
        public ProxyNotification OnLogOut;

        /// <summary>
        /// Called when the client crashes.
        /// </summary>
        public ProxyNotification OnCrash;

        /// <summary>
        /// Called when the user enters bad login details.
        /// </summary>
        public ProxyNotification OnBadLogin;

        /// <summary>
        /// Called when a packet is received from the server.
        /// </summary>
        public PacketListener ReceivedPacketFromServer;

        /// <summary>
        /// Used for debugging, called for each logical packet in a combined packet.
        /// </summary>
        public PacketListener SplitPacketFromServer;

        /// <summary>
        /// Called when a packet is received from the client.
        /// </summary>
        public PacketListener ReceivedPacketFromClient;

        // Incoming
        public PacketListener ReceivedAnimatedTextPacket;
        public PacketListener ReceivedBookOpenPacket;
        public PacketListener ReceivedCancelAutoWalkPacket;
        public PacketListener ReceivedChannelListPacket;
        public PacketListener ReceivedChannelOpenPacket;
        public PacketListener ReceivedChatMessagePacket;
        public PacketListener ReceivedContainerClosedPacket;
        public PacketListener ReceivedContainerItemAddPacket;
        public PacketListener ReceivedContainerItemRemovePacket;
        public PacketListener ReceivedContainerItemUpdatePacket;
        public PacketListener ReceivedContainerOpenedPacket;
        public PacketListener ReceivedCreatureHealthPacket;
        public PacketListener ReceivedCreatureLightPacket;
        public PacketListener ReceivedCreatureMovePacket;
        public PacketListener ReceivedCreatureOutfitPacket;
        public PacketListener ReceivedCreatureSpeedPacket;
        public PacketListener ReceivedCreatureSkullPacket;
        public PacketListener ReceivedCreatureSquarePacket;
        public PacketListener ReceivedEqItemAddPacket;
        public PacketListener ReceivedEqItemRemovePacket;
        public PacketListener ReceivedFlagUpdatePacket;
        public PacketListener ReceivedInformationBoxPacket;
        public PacketListener ReceivedMapItemAddPacket;
        public PacketListener ReceivedMapItemRemovePacket;
        public PacketListener ReceivedMapItemUpdatePacket;
        public PacketListener ReceivedNpcTradeListPacket;
        public PacketListener ReceivedNpcTradeGoldCountPacket;
        public PacketListener ReceivedPartyInvitePacket;
        public PacketListener ReceivedPrivateChannelOpenPacket;
        public PacketListener ReceivedProjectilePacket;
        public PacketListener ReceivedSkillUpdatePacket;
        public PacketListener ReceivedStatusMessagePacket;
        public PacketListener ReceivedStatusUpdatePacket;
        public PacketListener ReceivedTileAnimationPacket;
        public PacketListener ReceivedVipAddPacket;
        public PacketListener ReceivedVipLoginPacket;
        public PacketListener ReceivedVipLogoutPacket;
        public PacketListener ReceivedWorldLightPacket;


        // Outgoing
        public PacketListener ReceivedPlayerSpeechPacket;

        #endregion

        public Window1()
        {
            InitializeComponent();
            this.ReceivedChatMessagePacket += ChatMessageReceived;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            loginsvindex=0;
            GetChars(loginsvindex);
        }

        private void ConnectCharacter(object sender, RoutedEventArgs e)
        {
            if (conCharBtn.Content.ToString() == "Connect")
            {
                if (charsListView.SelectedItem != null)
                {
                    charindex = charsListView.SelectedIndex;
                    Random rnd = new Random();
                    rnd.NextBytes(xtea);
                    ConnectChar();
                }
            }
            else
            {

                GameLoginSock.Send(Tibia.Util.XTEA.Encrypt(new byte[] { 0x01, 0x00, 0x14 }, xtea, true));
                GameLoginSock.Disconnect(true);
                loggedin = false;
                pingTimer.Dispose();
                conCharBtn.Content = "Disconnect";

            }
        }

        private bool ChatMessageReceived(Packet packet)
        {
            ChatMessagePacket p = new ChatMessagePacket(null, packet.Data);

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
            {
                switch (p.MessageType)
                {
                    case Tibia.Packets.ChatType.PrivateMessage:
                        
                        defaulttxt.AppendText(p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n");
                        break;
                    case Tibia.Packets.ChatType.Normal:
                    case Tibia.Packets.ChatType.Whisper:
                    case Tibia.Packets.ChatType.Yell:
                        defaulttxt.AppendText(p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n");
                        break;

                }
            });
            return true;
        }

        private void GetChars(int loginsvindex)
        {
            connectbtn.IsEnabled = false;
            AccName = txtacc.Text;
            AccPass = txtpass.Text;
            CharListSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CharListSock.BeginConnect(loginServers[loginsvindex].IP, loginServers[loginsvindex].Port, (AsyncCallback)LoginServerConnect, null);
        }

        private void LoginServerConnect(IAsyncResult ar)
        {
            try
            {
                CharListSock.EndConnect(ar);
                Random rnd = new Random();
                rnd.NextBytes(xtea);
                CharLoginPacket=Clientless.CreateCharListPacket(AccName,AccPass,"831",
                    new byte[] { 0xB6, 0x1F, 0xDA, 0x48, 0x12, 0xE7, 0xC8, 0x48, 0x06, 0x21, 0x56, 0x48 },xtea);
                SocketError error = new SocketError();
                CharListSock.Send(CharLoginPacket, 0, CharLoginPacket.Length, SocketFlags.None, out error);
                CharListSock.BeginReceive(dataLoginServer, 0, dataLoginServer.Length, SocketFlags.None, (AsyncCallback)CharListReceived, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Tibia Chat",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void CharListReceived(IAsyncResult ar)
        {
            try
            {                
                int dataLength = CharListSock.EndReceive(ar);
                if (dataLength > 0)
                {
                    byte[] tmp = new byte[dataLength];
                    Array.Copy(dataLoginServer, tmp, dataLength);
                    CharListDecrypted = Tibia.Util.XTEA.Decrypt(tmp, xtea, true);
                    if (CharListDecrypted[2] == 0x14)
                    {
                        ParseCharList(CharListDecrypted);;
                        CharListSock.BeginDisconnect(true, (AsyncCallback)LoginServerDisconnect, null);
                    }
                    else 
                    {
                        if(loginsvindex<=9)
                        {
                            CharListSock.Disconnect(true);
                            CharListSock.BeginConnect(loginServers[loginsvindex++].IP, loginServers[loginsvindex++].Port, (AsyncCallback)LoginServerConnect, null);
                        }
                        else
                        {
                            MessageBox.Show("Could not connect to any login server.\r\n"
                            ,"Tibia Chat",MessageBoxButton.OK,MessageBoxImage.Error);
                            CharListSock.BeginDisconnect(true, (AsyncCallback)LoginServerDisconnect, null);
                        }

                    }
                }
                else
                {
                    if (loginsvindex <= 9)
                    {
                        CharListSock.Disconnect(true);
                        CharListSock.BeginConnect(loginServers[loginsvindex++].IP, loginServers[loginsvindex++].Port, (AsyncCallback)LoginServerConnect, null);
                    }
                    else
                    {
                        MessageBox.Show("Could not connect to any login server.\r\n"
                        , "Tibia Chat", MessageBoxButton.OK, MessageBoxImage.Error);
                        CharListSock.BeginDisconnect(true, (AsyncCallback)LoginServerDisconnect, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginServerDisconnect(IAsyncResult ar)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
            {
                connectbtn.IsEnabled = true;
            });
        }

        private void ConnectChar()
        {
            GameLoginSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            GameLoginSock.BeginConnect(_mycharacters[charindex].worldIP,
                _mycharacters[charindex].worldPort,
                (AsyncCallback)GameServerConnect, null);
        }

        private void GameServerConnect(IAsyncResult ar)
        {
            GameLoginSock.EndConnect(ar);
            SocketError error = new SocketError();
            GameLoginPacket = Clientless.CreateGameLoginPacket(AccName,
                AccPass, _mycharacters[charindex].charName, "831", xtea);
            GameLoginSock.Send(GameLoginPacket, 0, 139, SocketFlags.None, out error);
            pingTimer = new System.Threading.Timer(pingTimerCallback, null, 30000, 30000);
            GameLoginSock.BeginReceive(dataGameServer, 0, dataGameServer.Length, SocketFlags.None,
                (AsyncCallback)GameServerReceive, null);
        }

        private void GameServerReceive(IAsyncResult ar)
        {
            int bytesRead = GameLoginSock.EndReceive(ar);
            int offset = 0;

            while (bytesRead - offset > 0)
            {
                // Get the packet length
                int packetlength = BitConverter.ToInt16(dataGameServer, offset) + 2;
                // Parse the data into a single packet
                byte[] packet = new byte[packetlength];
                Array.Copy(dataGameServer, offset, packet, 0, packetlength);

                serverReceiveQueue.Enqueue(packet);

                offset += packetlength;
            }

            ProcessServerReceiveQueue();

            GameLoginSock.BeginReceive(dataGameServer, 0, dataGameServer.Length,
                SocketFlags.None, (AsyncCallback)GameServerReceive, null);
        }

        private void ProcessServerReceiveQueue()
        {
            if (serverReceiveQueue.Count > 0)
            {

                byte[] original = serverReceiveQueue.Dequeue();
                byte[] decrypted = DecryptPacket(original);



                int remaining = 0; // the bytes worth of logical packets left

                // Always call the default (if attached to)
                if (ReceivedPacketFromServer != null)
                    ReceivedPacketFromServer.BeginInvoke(new Packet(null, decrypted), null, null);

                // Is this a part of a larger packet?
                if (partialRemaining > 0)
                {
                    // Not sure if this works yet...
                    // Yes, stack it onto the end of the partial packet
                    partial.AddBytes(decrypted);

                    // Subtract from the remaining needed
                    partialRemaining -= decrypted.Length;
                }
                else
                {
                    // No, create a new partial packet
                    partial = new PacketBuilder(null, decrypted);
                    remaining = partial.GetInt();
                    partialRemaining = remaining - (decrypted.Length - 2); // packet length - part we already have
                }
                // Do we have a complete packet now?
                if (partialRemaining == 0)
                {
                    int length = 0;
                    bool forward;

                    // Keep going until no more logical packets
                    while (remaining > 0)
                    {
                        forward = RaiseIncomingEvents(decrypted, ref length);

                        // If packet not found in database, skip the rest
                        if (length == -1)
                        {

                            if (decrypted[2] == (int)PacketType.AddCreature)
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (System.Threading.ThreadStart)delegate()
                                {
                                    conCharBtn.Content = "Disconnect";
                                    charsExpander.IsExpanded = false;
                                    convExpander.IsExpanded = true;
                                });
                                loggedin = true;
                            }
                            //SendToClient(decrypted);
                            break;
                        }

                        length++;
                        if (forward)
                        {
                            if (SplitPacketFromServer != null)
                                SplitPacketFromServer.BeginInvoke(new Packet(null, Packet.Repackage(decrypted, 2, length)), null, null);

                            // Repackage it and send
                            //SendToClient(Packet.Repackage(decrypted, 2, length));
                        }

                        // Subtract the amount that was parsed
                        remaining -= length;

                        // Repackage decrypted without the first logical packet
                        if (remaining > 0)
                            decrypted = Packet.Repackage(decrypted, length + 2);
                    }
                }
                // else, delay processing until the rest of the packet arrives

                if (serverReceiveQueue.Count > 0)
                    ProcessServerReceiveQueue();
            }
        }

        private bool RaiseIncomingEvents(byte[] packet, ref int length)
        {
            length = -1;
            if (packet.Length < 3) return true;
            Packet p;
            PacketType type = (PacketType)packet[2];
            switch (type)
            {
                case PacketType.AnimatedText:
                    p = new AnimatedTextPacket(null, packet);
                    length = p.Index;
                    if (ReceivedAnimatedTextPacket != null)
                        return ReceivedAnimatedTextPacket(p);
                    break;
                case PacketType.BookOpen:
                    p = new BookOpenPacket(null, packet);
                    length = p.Index;
                    if (ReceivedBookOpenPacket != null)
                        return ReceivedBookOpenPacket(p);
                    break;
                case PacketType.CancelAutoWalk:
                    p = new CancelAutoWalkPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCancelAutoWalkPacket != null)
                        return ReceivedCancelAutoWalkPacket(p);
                    break;
                case PacketType.ChannelList:
                    p = new ChannelListPacket(null, packet);
                    length = p.Index;
                    if (ReceivedChannelListPacket != null)
                        return ReceivedChannelListPacket(p);
                    break;
                case PacketType.ChannelOpen:
                    p = new ChannelOpenPacket(null, packet);
                    length = p.Index;
                    if (ReceivedChannelOpenPacket != null)
                        return ReceivedChannelOpenPacket(p);
                    break;
                case PacketType.ChatMessage:
                    p = new ChatMessagePacket(null, packet);
                    length = p.Index;
                    if (ReceivedChatMessagePacket != null)
                        return ReceivedChatMessagePacket(p);
                    break;
                case PacketType.ContainerClosed:
                    p = new ContainerClosedPacket(null, packet);
                    length = p.Index;
                    if (ReceivedContainerClosedPacket != null)
                        return ReceivedContainerClosedPacket(p);
                    break;
                case PacketType.ContainerItemAdd:
                    p = new ContainerItemAddPacket(null, packet);
                    length = p.Index;
                    if (ReceivedContainerItemAddPacket != null)
                        return ReceivedContainerItemAddPacket(p);
                    break;
                case PacketType.ContainerItemRemove:
                    p = new ContainerItemRemovePacket(null, packet);
                    length = p.Index;
                    if (ReceivedContainerItemRemovePacket != null)
                        return ReceivedContainerItemRemovePacket(p);
                    break;
                case PacketType.ContainerItemUpdate:
                    p = new ContainerItemUpdatePacket(null, packet);
                    length = p.Index;
                    if (ReceivedContainerItemUpdatePacket != null)
                        return ReceivedContainerItemUpdatePacket(p);
                    break;
                case PacketType.ContainerOpened:
                    p = new ContainerOpenedPacket(null, packet);
                    length = p.Index;
                    if (ReceivedContainerOpenedPacket != null)
                        return ReceivedContainerOpenedPacket(p);
                    break;
                case PacketType.CreatureHealth:
                    p = new CreatureHealthPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureHealthPacket != null)
                        return ReceivedCreatureHealthPacket(p);
                    break;
                case PacketType.CreatureLight:
                    p = new CreatureLightPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureLightPacket != null)
                        return ReceivedCreatureLightPacket(p);
                    break;
                case PacketType.CreatureMove:
                    p = new CreatureMovePacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureMovePacket != null)
                        return ReceivedCreatureMovePacket(p);
                    break;
                case PacketType.CreatureOutfit:
                    p = new CreatureOutfitPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureOutfitPacket != null)
                        return ReceivedCreatureOutfitPacket(p);
                    break;
                case PacketType.CreatureSkull:
                    p = new CreatureSkullPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureSkullPacket != null)
                        return ReceivedCreatureSkullPacket(p);
                    break;
                case PacketType.CreatureSpeed:
                    p = new CreatureSpeedPacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureSpeedPacket != null)
                        return ReceivedCreatureSpeedPacket(p);
                    break;
                case PacketType.CreatureSquare:
                    p = new CreatureSquarePacket(null, packet);
                    length = p.Index;
                    if (ReceivedCreatureSquarePacket != null)
                        return ReceivedCreatureSquarePacket(p);
                    break;
                case PacketType.EqItemAdd:
                    p = new EqItemAddPacket(null, packet);
                    length = p.Index;
                    if (ReceivedEqItemAddPacket != null)
                        return ReceivedEqItemAddPacket(p);
                    break;
                case PacketType.EqItemRemove:
                    p = new EqItemRemovePacket(null, packet);
                    length = p.Index;
                    if (ReceivedEqItemRemovePacket != null)
                        return ReceivedEqItemRemovePacket(p);
                    break;
                case PacketType.FlagUpdate:
                    p = new FlagUpdatePacket(null, packet);
                    length = p.Index;
                    if (ReceivedFlagUpdatePacket != null)
                        return ReceivedFlagUpdatePacket(p);
                    break;
                case PacketType.InformationBox:
                    p = new InformationBoxPacket(null, packet);
                    length = p.Index;
                    if (ReceivedInformationBoxPacket != null)
                        return ReceivedInformationBoxPacket(p);
                    break;
                case PacketType.MapItemAdd:
                    p = new MapItemAddPacket(null, packet);
                    length = p.Index;
                    if (ReceivedMapItemAddPacket != null)
                        return ReceivedMapItemAddPacket(p);
                    break;
                case PacketType.MapItemRemove:
                    p = new MapItemRemovePacket(null, packet);
                    length = p.Index;
                    if (ReceivedMapItemRemovePacket != null)
                        return ReceivedMapItemRemovePacket(p);
                    break;
                case PacketType.MapItemUpdate:
                    p = new MapItemUpdatePacket(null, packet);
                    length = p.Index;
                    if (ReceivedMapItemUpdatePacket != null)
                        return ReceivedMapItemUpdatePacket(p);
                    break;
                case PacketType.NpcTradeList:
                    p = new NpcTradeListPacket(null, packet);
                    length = p.Index;
                    if (ReceivedNpcTradeListPacket != null)
                        return ReceivedNpcTradeListPacket(p);
                    break;
                case PacketType.NpcTradeGoldCountSaleList:
                    p = new NpcTradeGoldCountSaleListPacket(null, packet);
                    length = p.Index;
                    if (ReceivedNpcTradeGoldCountPacket != null)
                        return ReceivedNpcTradeGoldCountPacket(p);
                    break;
                case PacketType.PartyInvite:
                    p = new PartyInvitePacket(null, packet);
                    length = p.Index;
                    if (ReceivedPartyInvitePacket != null)
                        return ReceivedPartyInvitePacket(p);
                    break;
                case PacketType.PrivateChannelOpen:
                    p = new PrivateChannelOpenPacket(null, packet);
                    length = p.Index;
                    if (ReceivedPrivateChannelOpenPacket != null)
                        return ReceivedPrivateChannelOpenPacket(p);
                    break;
                case PacketType.Projectile:
                    p = new ProjectilePacket(null, packet);
                    length = p.Index;
                    if (ReceivedProjectilePacket != null)
                        return ReceivedProjectilePacket(p);
                    break;
                case PacketType.SkillUpdate:
                    p = new SkillUpdatePacket(null, packet);
                    if (ReceivedSkillUpdatePacket != null)
                        return ReceivedSkillUpdatePacket(p);
                    break;
                case PacketType.StatusMessage:
                    p = new StatusMessagePacket(null, packet);
                    length = p.Index;
                    if (ReceivedStatusMessagePacket != null)
                        return ReceivedStatusMessagePacket(p);
                    break;
                case PacketType.StatusUpdate:
                    p = new StatusUpdatePacket(null, packet, true);
                    length = p.Index;
                    if (ReceivedStatusUpdatePacket != null)
                        return ReceivedStatusUpdatePacket(p);
                    break;
                case PacketType.TileAnimation:
                    p = new TileAnimationPacket(null, packet);
                    length = p.Index;
                    if (ReceivedTileAnimationPacket != null)
                        return ReceivedTileAnimationPacket(p);
                    break;
                case PacketType.VipAdd:
                    p = new VipAddPacket(null, packet);
                    length = p.Index;
                    if (ReceivedVipAddPacket != null)
                        return ReceivedVipAddPacket(p);
                    break;
                case PacketType.VipLogin:
                    p = new VipLoginPacket(null, packet);
                    length = p.Index;
                    if (ReceivedVipLoginPacket != null)
                        return ReceivedVipLoginPacket(p);
                    break;
                case PacketType.VipLogout:
                    p = new VipLogoutPacket(null, packet);
                    length = p.Index;
                    if (ReceivedVipLogoutPacket != null)
                        return ReceivedVipLogoutPacket(p);
                    break;
                case PacketType.WorldLight:
                    p = new WorldLightPacket(null, packet);
                    length = p.Index;
                    if (ReceivedWorldLightPacket != null)
                        return ReceivedWorldLightPacket(p);
                    break;
            }
            return true;
        }

        #region Encryption
        /// <summary>
        /// Wrapper for XTEA.Encrypt
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public byte[] EncryptPacket(byte[] packet)
        {
            return Tibia.Util.XTEA.Encrypt(packet, xtea, true);
        }

        /// <summary>
        /// Wrapper for XTEA.Decrypt
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public byte[] DecryptPacket(byte[] packet)
        {
            return Tibia.Util.XTEA.Decrypt(packet, xtea, true);
        }

        /// <summary>
        /// Get the type of an encrypted packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private byte GetPacketType(byte[] packet)
        {
            return Tibia.Util.XTEA.DecryptType(packet, xtea, true);
        }
        #endregion

        private void pingTimerCallback(object o)
        {
            if(loggedin)
                GameLoginSock.Send(Tibia.Util.XTEA.Encrypt(new byte[] { 0x01, 0x00, 0x1E }, xtea, true));
        }

        private void ParseCharList(byte[] packet)
        {
            CharListPacket clpacket = new CharListPacket(CharListDecrypted);
            clpacket.ParseData(packet);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
                        {
                            
                _mycharacters.Clear();
                for (int i = 0; i < clpacket.chars.Length; i++)
                {
                    _mycharacters.Add(new CharListChar{
                        lenCharName=clpacket.chars[i].lenCharName,
                        charName=clpacket.chars[i].charName,
                        lenWorldName=clpacket.chars[i].lenWorldName,
                        worldName=clpacket.chars[i].worldName,
                        worldIP=clpacket.chars[i].worldIP,
                        worldPort=clpacket.chars[i].worldPort,
                    });
                }
                conExpander.IsExpanded = false;
                charsExpander.IsEnabled = true;
                charsExpander.IsExpanded = true;
            });
        }

        public class CharListChar
        {
            public short lenCharName { get; set; }
            public string charName { get; set; }
            public short lenWorldName { get; set; }
            public string worldName { get; set; }
            public string worldIP { get; set; }
            public short worldPort { get; set; }
        }

        public ObservableCollection<CharListChar> mycharacters
        { get { return _mycharacters; } }

        public class LoginServer
        {
            public string IP = null;
            public ushort Port;
            public LoginServer(string ip, ushort port)
            {
                IP = ip;
                Port = port;
            }
        }

        public class Clientless
        {
            public static byte[] CreateCharListPacket(string accname, string password, string version, byte[] signatures, byte[] xtea)
            {                
                PacketBuilder p = new PacketBuilder(null);
                p.AddByte(0x01);
                p.AddInt(0x02);//OS
                p.AddInt(Convert.ToUInt16(version));
                p.AddBytes(signatures);
                //Beginning of the packet part to be encrypted with rsa
                p.AddByte(0x0);
                if (xtea != null)
                {
                    if (xtea.Length != 16)
                    {
                        xtea = new byte[16];
                        Random rnd = new Random();
                        rnd.NextBytes(xtea);
                    }
                }
                else
                {
                    xtea = new byte[16];
                    Random rnd = new Random();
                    rnd.NextBytes(xtea);
                }
                p.AddBytes(xtea);
                p.AddString(accname);
                p.AddString(password);              
                byte[] toencrypt = new byte[128];
                Array.Copy(p.Data, 17, toencrypt, 0, 128);
                BigInteger message = new BigInteger(toencrypt);
                BigInteger n = new BigInteger("124710459426827943004376449897985582167801707960697037164044904862948569380850421396904597686953877022394604239428185498284169068581802277612081027966724336319448537811441719076484340922854929273517308661370727105382899118999403808045846444647284499123164879035103627004668521005328367415259939915284902061793", 10);
                BigInteger e = new BigInteger("65537", 10);
                BigInteger encrypted = message.modPow(e, n);
                byte[] encryptedbytes = encrypted.getBytes();
                p.Index = 17;
                p.AddBytes(encryptedbytes);
                return Tibia.Util.XTEA.AddAdlerChecksum(p.GetPacket());
            }


            public static byte[] CreateGameLoginPacket(string accname, string password, string charname, string version, byte[] xtea)
            {
                PacketBuilder p=new PacketBuilder(null);
                p.AddByte(0xA);
                p.AddInt(0x2);
                p.AddInt(Convert.ToUInt16(version));
                //Beginning of the packet part to be encrypted with rsa
                p.AddByte(0x0);

                if (xtea != null)
                {
                    if (xtea.Length != 16)
                    {
                        xtea = new byte[16];
                        Random rnd = new Random();
                        rnd.NextBytes(xtea);
                    }
                }
                else
                {
                    xtea = new byte[16];
                    Random rnd = new Random();
                    rnd.NextBytes(xtea);
                }
                p.AddBytes(xtea);
                p.AddByte(0x0);
                p.AddString(accname);
                p.AddString(charname);
                p.AddString(password);
                byte[] toencrypt = new byte[128];
                Array.Copy(p.Data, 5, toencrypt, 0, 128);
                BigInteger message = new BigInteger(toencrypt);
                BigInteger n = new BigInteger("124710459426827943004376449897985582167801707960697037164044904862948569380850421396904597686953877022394604239428185498284169068581802277612081027966724336319448537811441719076484340922854929273517308661370727105382899118999403808045846444647284499123164879035103627004668521005328367415259939915284902061793", 10);
                BigInteger e = new BigInteger("65537", 10);
                BigInteger encrypted = message.modPow(e, n);
                byte[] encryptedbytes = encrypted.getBytes();
                p.Index = 5;
                p.AddBytes(encryptedbytes);
                return Tibia.Util.XTEA.AddAdlerChecksum(p.GetPacket());
            }
        }

        }
    }

