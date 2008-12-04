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
        #region variables
        ObservableCollection<CharListChar> _mycharacters = new ObservableCollection<CharListChar>();
        ObservableCollection<TabItem> _tabItemCollection = new ObservableCollection<TabItem>();
        Socket CharListSock;
        Socket GameLoginSock;
        Queue<byte[]> serverReceiveQueue = new Queue<byte[]>();
        List<string> sentmessages = new List<string>();
        int sentmessageindex = 0;

        List<Tibia.Objects.Channel> availablechannels = new List<Tibia.Objects.Channel>();

        string AccName;
        string AccPass;
        string charname;
        byte[] dataLoginServer = new byte[8192];
        byte[] dataGameServer = new byte[8192];
        byte[] xtea = new byte[16];
        byte[] CharLoginPacket;
        byte[] GameLoginPacket;
        byte[] CharListDecrypted;
        double currprogress;
        double maxprogress;
        string waitingmessage = null;
        int loginsvindex;
        int charindex;
        int partialRemaining = 0;
        PacketBuilder partial;
        bool loggedin = false;
        bool disconnecting = false;
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
        System.Threading.Timer progressTimer;
        bool blocktimer;
        #endregion

        #region Events
        /// <summary>
        /// A generic function prototype for packet events.
        /// </summary>
        /// <param name="packet">The unencrypted packet that was received.</param>
        /// <returns>true to continue forwarding the packet, false to drop the packet</returns>
        public delegate bool PacketListener(Packet packet);
        

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
            //attaching received message event..
            this.ReceivedChatMessagePacket += ChatMessageReceived;
            this.ReceivedChannelListPacket += ChannelListReceived;
            this.ReceivedPrivateChannelOpenPacket += PrivateChannelOpenReceived;
            GradientStopCollection gsc=new GradientStopCollection();
            gsc.Add(new GradientStop(Color.FromArgb(255, 204, 232, 255),0.3));
            gsc.Add(new GradientStop(Color.FromArgb(255, 125, 175, 255), 1));
            this.Background = new LinearGradientBrush(gsc);
        }

        private void wndLoaded(object sender, RoutedEventArgs e)
        {
            //adding default tab and giving "focus" to it
            AddTab("Default");
            convosTab.SelectedItem = convosTab.Items[0];
        }

        private void LoginBtnClick(object sender, RoutedEventArgs e)
        {            
            loginsvindex = 0;
            GetChars(loginsvindex);
        }

        private void ConnectBtnClick(object sender, RoutedEventArgs e)
        {
            //Connect char to the game world
            if (ConnectBtn.Content.ToString() == "Connect")
            {
                if (charsListView.SelectedItem != null)
                {
                    charindex = charsListView.SelectedIndex;
                    charname = _mycharacters[charindex].charName;
                    //creating random xtea key..
                    Random rnd = new Random();
                    rnd.NextBytes(xtea);
                    ConnectChar();
                }
            }
            //disconnect from g. w.
            else if (ConnectBtn.Content.ToString() == "Disconnect")
            {
                if (GameLoginSock.Connected)
                {
                    SendPacket(new byte[] { 0x01, 0x00, 0x14 });
                    GameLoginSock.BeginDisconnect(true, (AsyncCallback)GameDisconnect, null);
                }
                ConnectBtn.IsEnabled = false;
                SendBtn.IsEnabled = false;
                disconnecting = true;
            }
            //cancel connection while on the waiting list
            else if (ConnectBtn.Content.ToString() == "Abort")
            {
                if (progressTimer != null) progressTimer.Dispose();
                if (GameLoginSock.Connected)
                    try
                    { GameLoginSock.Disconnect(true); }
                    catch { }
                conProgress.Value = 0;
                conProgress.ToolTip = null;
                blocktimer = false;
                LoginBtn.IsEnabled = true;
                ConnectBtn.Content = "Connect";
            }
        }

        private void makeButtonDefault(object sender, RoutedEventArgs e)
        {
            if (sender == txtacc || sender == txtpass || sender == LoginBtn)
            {
                if (sender == txtacc) txtacc.SelectAll();
                else if (sender == txtpass) txtpass.SelectAll();
                LoginBtn.IsDefault = true;
                ConnectBtn.IsDefault = false;
                SendBtn.IsDefault = false;
            }
            else if (sender == charsListView || sender == ConnectBtn)
            {
                LoginBtn.IsDefault = false;
                ConnectBtn.IsDefault = true;
                SendBtn.IsDefault = false;
            }
            else if (sender == convosTab || sender == messagetxt)
            {
                LoginBtn.IsDefault = false;
                ConnectBtn.IsDefault = false;
                SendBtn.IsDefault = true;
            }
        }

        private bool ChatMessageReceived(Packet packet)
        {
            ChatMessagePacket p = new ChatMessagePacket(packet.Data);
            if (p.SenderName == charname) return true;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
            {
                switch (p.MessageType)
                {
                    case ChatType.PrivateMessage:
                        if(!AppendText(p.SenderName.ToLower(),p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n"))
                            AppendText("Default".ToLower(),"(PVT) " + p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n");
                        break;
                    case ChatType.Normal:
                    case ChatType.Whisper:
                    case ChatType.Yell:
                        AppendText("Default".ToLower(), p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n");
                        break;
                    case ChatType.ChannelNormal:
                        foreach (Tibia.Objects.Channel channel in availablechannels)
                        {
                            if (p.Channel == channel.Id)
                            {
                                AppendText(channel.Name.ToLower(), p.SenderName + " [" + p.SenderLevel + "]: " + p.Message + "\n");
                            }
                        }
                        break;                        
                }
            });
            return true;
        }

        private bool ChannelListReceived(Packet packet)
        {
            ChannelListPacket p = new ChannelListPacket(packet.Data);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                availablechannels = p.Channels;
                foreach (Tibia.Objects.Channel channel in availablechannels)
                {
                    //openning the channels
                    SendPacket(new byte[] { 0x03, 0x00, 0x98, (byte)channel.Id ,0x00});
                }
                string newChannel = newchannel.ShowBox(p.Channels);
                if (newChannel != null)
                {
                    foreach (TabItem tab in _tabItemCollection)
                    {
                        if (tab.Header.ToString().ToLower() == newChannel.ToLower())
                            return; 
                    }
                    foreach (Tibia.Objects.Channel channel in availablechannels)
                    {
                        if (channel.Name == newChannel)
                        {
                            AddTab(newChannel);
                            return;
                        }
                    }
                    PacketBuilder pb=new PacketBuilder();
                    pb.AddByte(0x9A);
                    pb.AddString(newChannel);
                    //sending to the server a open private channel request
                    //it'll return the correct name of the player if it exists
                    SendPacket(pb.GetPacket());        
                }
            });
            return true;
        }

        private bool PrivateChannelOpenReceived(Packet packet)
        {
            PrivateChannelOpenPacket p = new PrivateChannelOpenPacket(packet.Data);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                foreach (TabItem ti in _tabItemCollection)
                {
                    if (ti.Header.ToString() == p.Name) return;
                }
                AddTab(p.Name);
            });
            return true;
        }

        private void convosTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((TabItem)convosTab.SelectedItem).Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                ((TextBox)(((TabItem)convosTab.SelectedItem).Content)).ScrollToEnd();
                messagetxt.Focus();
            }
            catch
            {
            }
        }

        #region tabcontrol contextmenu
        private void OpenChannel(object sender, RoutedEventArgs e)
        {
            //requesting available channels
            SendPacket(new byte[] { 0x01, 0x00, 0x97 });
        }

        private void CloseChannel(object sender, RoutedEventArgs e)
        {
            if (((TabItem)convosTab.SelectedItem).Header.ToString() != "Default")
                _tabItemCollection.Remove(((TabItem)convosTab.SelectedItem));
        }

        private void ClearChannel(object sender, RoutedEventArgs e)
        {
            ((TextBox)((TabItem)convosTab.SelectedItem).Content).Clear();
        }
        #endregion

        #region messaging
        private void SendClick(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            if(messagetxt.Text.Length>0)
            {
                foreach (Tibia.Objects.Channel channel in availablechannels)
                {
                    if (channel.Name == ((TabItem)convosTab.SelectedItem).Header.ToString())
                    {
                        SendPacket(PlayerSpeechPacket.Create(new Tibia.Objects.ChatMessage(messagetxt.Text, channel.Id)).Data);
                        sentmessageindex = 0;
                        sentmessages.Add(messagetxt.Text);
                        AppendText(((TabItem)convosTab.SelectedItem).Header.ToString(),
                            charname + " [0]: " + messagetxt.Text + "\n");
                        messagetxt.Clear();
                        return;
                    }
                }
                if(((TabItem)convosTab.SelectedItem).Header.ToString()=="Default")
                {
                    SendPacket(PlayerSpeechPacket.Create(messagetxt.Text).Data);
                }
                else
                {
                    SendPacket(PlayerSpeechPacket.Create(new Tibia.Objects.ChatMessage(
                        messagetxt.Text, 
                        ((TabItem)convosTab.SelectedItem).Header.ToString()))
                        .Data);
                }
                sentmessageindex = 0;
                sentmessages.Add(messagetxt.Text);
                AppendText(((TabItem)convosTab.SelectedItem).Header.ToString(),
                    charname+": " + messagetxt.Text + "\n");
                messagetxt.Clear();
            }
        }
        #endregion

        #region timers
        private void pingTimerCallback(object o)
        {
            if (loggedin)
                SendPacket(new byte[] { 0x01, 0x00, 0x1E });
        }

        private void progressTimerCallback(object o)
        {
            if (!blocktimer)
            {
                currprogress += 500;
                if (currprogress / 1000 < maxprogress)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)
                        delegate()
                        {
                            conProgress.ToolTip = waitingmessage+"Trying to reconnect in "+
                                Convert.ToUInt16(maxprogress-currprogress/1000) + " seconds.";
                            conProgress.Value = currprogress / (maxprogress * 10);
                        });
                }
                else
                {
                    blocktimer = true;
                    progressTimer.Dispose();
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)
                        delegate()
                        {
                            conProgress.ToolTip = null;
                            conProgress.Value = 0;
                        });
                    ConnectChar();
                }
            }
        }
        #endregion

        #region managing tabs and textboxes
        public void AddTab(string header)
        {
            TabItem ti = new TabItem();
            ti.Header = header;
            TextBox tb = new TextBox();
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Margin = new Thickness(3);
            tb.IsReadOnly = true;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            ti.Content = tb;
            _tabItemCollection.Add(ti);
            convosTab.SelectedItem = ti;
        }

        public bool AppendText(string channel, string message)
        {
            foreach (TabItem ti in _tabItemCollection)
            {
                if (ti.Header.ToString().ToLower() == channel.ToLower())
                {
                    if((TabItem)convosTab.SelectedItem!=ti)
                        ti.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((TextBox)ti.Content).AppendText(message);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region binded data

        public ObservableCollection<CharListChar> mycharacters
        { get { return _mycharacters; } }

        public ObservableCollection<TabItem> tabItemCollection
        { get { return _tabItemCollection; } }
        #endregion

        #region charlist
        private void GetChars(int loginsvindex)
        {
            LoginBtn.IsEnabled = false;
            AccName = txtacc.Password;
            AccPass = txtpass.Password;
            //connecting socket to the specified loginserver..
            CharListSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CharListSock.BeginConnect(loginServers[loginsvindex].IP, 
                loginServers[loginsvindex].Port, (AsyncCallback)LoginServerConnect, null);
        }

        private void LoginServerConnect(IAsyncResult ar)
        {
            try
            {
                CharListSock.EndConnect(ar);
                Random rnd = new Random();
                //creating random xtea key
                rnd.NextBytes(xtea);
                CharLoginPacket = Clientless.CreateCharListPacket(AccName, AccPass, "831",
                    new byte[] { 0xB6, 0x1F, 0xDA, 0x48, 0x12, 0xE7, 0xC8, 0x48, 0x06, 0x21, 0x56, 0x48 }, xtea);
                SocketError error = new SocketError();
                //sending the charlist request packet
                CharListSock.Send(CharLoginPacket, 0, CharLoginPacket.Length, SocketFlags.None, out error);
                CharListSock.BeginReceive(dataLoginServer, 0, dataLoginServer.Length, SocketFlags.None, (AsyncCallback)CharListReceived, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Tibia Chat", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        //we received a char list
                        //parse and fill the listview with chars and their respective servers
                        ParseCharList(CharListDecrypted); 
                    }
                    else if (CharListDecrypted[2] == 0x0A)
                    {
                        //we got a "bad login" message
                        MessageBox.Show(Encoding.ASCII.GetString(
                        CharListDecrypted, 5, BitConverter.ToInt16(CharListDecrypted, 3)).Trim(), "Sorry", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        //we got an "unknown" message
                        MessageBox.Show("Unknown message received from the server.", "Sorry", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    CharListSock.BeginDisconnect(true, (AsyncCallback)LoginServerDisconnect, null);
                }
                else
                {
                    //we didn't receive anything
                    ConnectNextLoginServer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectNextLoginServer()
        {
            if (loginsvindex < 9)
            {
                //we didn't get the char list, so we connect to the next login server and retry
                CharListSock.Disconnect(true);
                loginsvindex++;
                CharListSock.BeginConnect(loginServers[loginsvindex].IP, loginServers[loginsvindex].Port, (AsyncCallback)LoginServerConnect, null);
            }
            else
            {
                //we went through all login servers and couldnt get the chars list
                MessageBox.Show("Could not connect to any login server.\r\n"
                , "Tibia Chat", MessageBoxButton.OK, MessageBoxImage.Error);
                CharListSock.BeginDisconnect(true, (AsyncCallback)LoginServerDisconnect, null);
            }
        }

        private void LoginServerDisconnect(IAsyncResult ar)
        {
            CharListSock.EndDisconnect(ar);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
            {
                LoginBtn.IsEnabled = true;
            });
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
                    _mycharacters.Add(new CharListChar
                    {
                        lenCharName = clpacket.chars[i].lenCharName,
                        charName = clpacket.chars[i].charName,
                        lenWorldName = clpacket.chars[i].lenWorldName,
                        worldName = clpacket.chars[i].worldName,
                        worldIP = clpacket.chars[i].worldIP,
                        worldPort = clpacket.chars[i].worldPort,
                    });
                }
                charsExpander.IsEnabled = true;
                charsExpander.IsExpanded = true;
            });
        }
        #endregion

        #region game connection
        private void ConnectChar()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                LoginBtn.IsEnabled = false;
                ConnectBtn.IsEnabled = false;
            });
            //connecting the selected char on the listview
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
            //send the game connection request
            GameLoginSock.Send(GameLoginPacket, 0, 139, SocketFlags.None, out error);
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
                //for some reason weird packets are being received sometimes...
                if (packetlength > 0)
                {
                    // Parse the data into a single packet
                    byte[] packet = new byte[packetlength];
                    Array.Copy(dataGameServer, offset, packet, 0, packetlength);

                    serverReceiveQueue.Enqueue(packet);

                    offset += packetlength;
                }
                else
                {
                }
            }

            ProcessServerReceiveQueue();

            if(!(disconnecting || !GameLoginSock.Connected || !loggedin))
            try
            {
                GameLoginSock.BeginReceive(dataGameServer, 0, dataGameServer.Length,
                    SocketFlags.None, (AsyncCallback)GameServerReceive, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Tibia Chat - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (progressTimer != null) progressTimer.Dispose();
                if (pingTimer != null) pingTimer.Dispose();
                if (GameLoginSock.Connected)
                {
                    disconnecting = true;
                    GameLoginSock.BeginDisconnect(true, (AsyncCallback)GameDisconnect, null);
                }
                else
                    this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (System.Threading.ThreadStart)delegate()
                    {
                        _tabItemCollection.Clear();
                        AddTab("Default");
                        convosTab.SelectedItem = convosTab.Items[0];
                        loggedin = false;
                        messagetxt.IsEnabled = false;
                        convosTab.IsEnabled = false;
                        ConnectBtn.IsEnabled = true;
                        SendBtn.IsEnabled = false;
                        ConnectBtn.Content = "Connect";
                    });
            }
        }

        private void GameDisconnect(IAsyncResult ar)
        {
            GameLoginSock.EndDisconnect(ar);
            disconnecting = false;
            pingTimer.Dispose();
            this.Dispatcher.Invoke(DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate()
            {
                _tabItemCollection.Clear();
                AddTab("Default");
                loggedin = false;
                convosTab.SelectedItem = convosTab.Items[0];
                LoginBtn.IsEnabled = true;
                messagetxt.IsEnabled = false;
                convosTab.IsEnabled = false;
                ConnectBtn.IsEnabled = true;
                SendBtn.IsEnabled = false;
                ConnectBtn.Content = "Connect";
            });
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
                    ReceivedPacketFromServer.BeginInvoke(new Packet(decrypted), null, null);

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
                    partial = new PacketBuilder( decrypted);
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
                            if (!loggedin)
                            {
                                //we are connected now
                                if (decrypted[2] == (int)PacketType.AddCreature || decrypted[2]==0xA3)
                                {
                                    loggedin = true;
                                    //starting to ping the server so as to not be disconnected
                                    pingTimer = new System.Threading.Timer(pingTimerCallback, null, 30000, 30000);
                                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (System.Threading.ThreadStart)delegate()
                                    {
                                        LoginBtn.IsEnabled = true;
                                        SendBtn.IsEnabled = true;
                                        ConnectBtn.Content = "Disconnect";
                                        ConnectBtn.IsEnabled = true;
                                        convExpander.IsExpanded = true;
                                        convosTab.IsEnabled = true;
                                        messagetxt.IsEnabled = true;
                                    });
                                }
                                //waiting list, not our turn or server overloaded
                                else if (decrypted[2] == 0x16)
                                {
                                    maxprogress = Convert.ToDouble(decrypted.Last());
                                    currprogress = 0;
                                    blocktimer = false;
                                    waitingmessage = Encoding.ASCII.GetString(decrypted, 5, BitConverter.ToInt16(decrypted, 3)).Trim();
                                    progressTimer = new System.Threading.Timer(progressTimerCallback, null, 0, 500);
                                    GameLoginSock.Disconnect(true);
                                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (System.Threading.ThreadStart)delegate()
                                    {
                                        ConnectBtn.Content = "Abort";
                                        ConnectBtn.IsEnabled = true;
                                    });                                
                                }
                                else
                                {
                                    disconnecting = true;
                                    GameLoginSock.BeginDisconnect(true, (AsyncCallback)GameDisconnect, null);
                                    MessageBox.Show("Unknown error message received from the game server.", "Tibia Chat",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            break;
                        }

                        length++;
                        if (forward)
                        {
                            if (SplitPacketFromServer != null)
                                SplitPacketFromServer.BeginInvoke(new Packet(Packet.Repackage(decrypted, 2, length)), null, null);

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
            {/*
                case PacketType.AnimatedText:
                    p = new AnimatedTextPacket(packet);
                    length = p.Index;
                    if (ReceivedAnimatedTextPacket != null)
                        return ReceivedAnimatedTextPacket(p);
                    break;
                case PacketType.BookOpen:
                    p = new BookOpenPacket(packet);
                    length = p.Index;
                    if (ReceivedBookOpenPacket != null)
                        return ReceivedBookOpenPacket(p);
                    break;
                case PacketType.CancelAutoWalk:
                    p = new CancelAutoWalkPacket(packet);
                    length = p.Index;
                    if (ReceivedCancelAutoWalkPacket != null)
                        return ReceivedCancelAutoWalkPacket(p);
                    break;*/
                case PacketType.ChannelList:
                    p = new ChannelListPacket(packet);
                    length = p.Index;
                    if (ReceivedChannelListPacket != null)
                        return ReceivedChannelListPacket(p);
                    break;/*
                case PacketType.ChannelOpen:
                    p = new ChannelOpenPacket(packet);
                    length = p.Index;
                    if (ReceivedChannelOpenPacket != null)
                        return ReceivedChannelOpenPacket(p);
                    break;*/
                case PacketType.ChatMessage:
                    p = new ChatMessagePacket(packet);
                    length = p.Index;
                    if (ReceivedChatMessagePacket != null)
                        return ReceivedChatMessagePacket(p);
                    break;/*
                case PacketType.ContainerClosed:
                    p = new ContainerClosedPacket(packet);
                    length = p.Index;
                    if (ReceivedContainerClosedPacket != null)
                        return ReceivedContainerClosedPacket(p);
                    break;
                case PacketType.ContainerItemAdd:
                    p = new ContainerItemAddPacket(packet);
                    length = p.Index;
                    if (ReceivedContainerItemAddPacket != null)
                        return ReceivedContainerItemAddPacket(p);
                    break;
                case PacketType.ContainerItemRemove:
                    p = new ContainerItemRemovePacket(packet);
                    length = p.Index;
                    if (ReceivedContainerItemRemovePacket != null)
                        return ReceivedContainerItemRemovePacket(p);
                    break;
                case PacketType.ContainerItemUpdate:
                    p = new ContainerItemUpdatePacket(packet);
                    length = p.Index;
                    if (ReceivedContainerItemUpdatePacket != null)
                        return ReceivedContainerItemUpdatePacket(p);
                    break;
                case PacketType.ContainerOpened:
                    p = new ContainerOpenedPacket(packet);
                    length = p.Index;
                    if (ReceivedContainerOpenedPacket != null)
                        return ReceivedContainerOpenedPacket(p);
                    break;
                case PacketType.CreatureHealth:
                    p = new CreatureHealthPacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureHealthPacket != null)
                        return ReceivedCreatureHealthPacket(p);
                    break;
                case PacketType.CreatureLight:
                    p = new CreatureLightPacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureLightPacket != null)
                        return ReceivedCreatureLightPacket(p);
                    break;
                case PacketType.CreatureMove:
                    p = new CreatureMovePacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureMovePacket != null)
                        return ReceivedCreatureMovePacket(p);
                    break;
                case PacketType.CreatureOutfit:
                    p = new CreatureOutfitPacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureOutfitPacket != null)
                        return ReceivedCreatureOutfitPacket(p);
                    break;
                case PacketType.CreatureSkull:
                    p = new CreatureSkullPacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureSkullPacket != null)
                        return ReceivedCreatureSkullPacket(p);
                    break;
                case PacketType.CreatureSpeed:
                    p = new CreatureSpeedPacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureSpeedPacket != null)
                        return ReceivedCreatureSpeedPacket(p);
                    break;
                case PacketType.CreatureSquare:
                    p = new CreatureSquarePacket(packet);
                    length = p.Index;
                    if (ReceivedCreatureSquarePacket != null)
                        return ReceivedCreatureSquarePacket(p);
                    break;
                case PacketType.EqItemAdd:
                    p = new EqItemAddPacket(packet);
                    length = p.Index;
                    if (ReceivedEqItemAddPacket != null)
                        return ReceivedEqItemAddPacket(p);
                    break;
                case PacketType.EqItemRemove:
                    p = new EqItemRemovePacket(packet);
                    length = p.Index;
                    if (ReceivedEqItemRemovePacket != null)
                        return ReceivedEqItemRemovePacket(p);
                    break;
                case PacketType.FlagUpdate:
                    p = new FlagUpdatePacket(packet);
                    length = p.Index;
                    if (ReceivedFlagUpdatePacket != null)
                        return ReceivedFlagUpdatePacket(p);
                    break;
                case PacketType.InformationBox:
                    p = new InformationBoxPacket(packet);
                    length = p.Index;
                    if (ReceivedInformationBoxPacket != null)
                        return ReceivedInformationBoxPacket(p);
                    break;
                case PacketType.MapItemAdd:
                    p = new MapItemAddPacket(packet);
                    length = p.Index;
                    if (ReceivedMapItemAddPacket != null)
                        return ReceivedMapItemAddPacket(p);
                    break;
                case PacketType.MapItemRemove:
                    p = new MapItemRemovePacket(packet);
                    length = p.Index;
                    if (ReceivedMapItemRemovePacket != null)
                        return ReceivedMapItemRemovePacket(p);
                    break;
                case PacketType.MapItemUpdate:
                    p = new MapItemUpdatePacket(packet);
                    length = p.Index;
                    if (ReceivedMapItemUpdatePacket != null)
                        return ReceivedMapItemUpdatePacket(p);
                    break;
                case PacketType.NpcTradeList:
                    p = new NpcTradeListPacket(packet);
                    length = p.Index;
                    if (ReceivedNpcTradeListPacket != null)
                        return ReceivedNpcTradeListPacket(p);
                    break;
                case PacketType.NpcTradeGoldCountSaleList:
                    p = new NpcTradeGoldCountSaleListPacket(packet);
                    length = p.Index;
                    if (ReceivedNpcTradeGoldCountPacket != null)
                        return ReceivedNpcTradeGoldCountPacket(p);
                    break;
                case PacketType.PartyInvite:
                    p = new PartyInvitePacket(packet);
                    length = p.Index;
                    if (ReceivedPartyInvitePacket != null)
                        return ReceivedPartyInvitePacket(p);
                    break;*/
                case PacketType.PrivateChannelOpen:
                    p = new PrivateChannelOpenPacket(packet);
                    length = p.Index;
                    if (ReceivedPrivateChannelOpenPacket != null)
                        return ReceivedPrivateChannelOpenPacket(p);
                    break;/*
                case PacketType.Projectile:
                    p = new ProjectilePacket(packet);
                    length = p.Index;
                    if (ReceivedProjectilePacket != null)
                        return ReceivedProjectilePacket(p);
                    break;
                case PacketType.SkillUpdate:
                    p = new SkillUpdatePacket(packet);
                    if (ReceivedSkillUpdatePacket != null)
                        return ReceivedSkillUpdatePacket(p);
                    break;
                case PacketType.StatusMessage:
                    p = new StatusMessagePacket(packet);
                    length = p.Index;
                    if (ReceivedStatusMessagePacket != null)
                        return ReceivedStatusMessagePacket(p);
                    break;
                case PacketType.StatusUpdate:
                    p = new StatusUpdatePacket(packet, true);
                    length = p.Index;
                    if (ReceivedStatusUpdatePacket != null)
                        return ReceivedStatusUpdatePacket(p);
                    break;
                case PacketType.TileAnimation:
                    p = new TileAnimationPacket(packet);
                    length = p.Index;
                    if (ReceivedTileAnimationPacket != null)
                        return ReceivedTileAnimationPacket(p);
                    break;
                case PacketType.VipAdd:
                    p = new VipAddPacket(packet);
                    length = p.Index;
                    if (ReceivedVipAddPacket != null)
                        return ReceivedVipAddPacket(p);
                    break;
                case PacketType.VipLogin:
                    p = new VipLoginPacket(packet);
                    length = p.Index;
                    if (ReceivedVipLoginPacket != null)
                        return ReceivedVipLoginPacket(p);
                    break;
                case PacketType.VipLogout:
                    p = new VipLogoutPacket(packet);
                    length = p.Index;
                    if (ReceivedVipLogoutPacket != null)
                        return ReceivedVipLogoutPacket(p);
                    break;
                case PacketType.WorldLight:
                    p = new WorldLightPacket(packet);
                    length = p.Index;
                    if (ReceivedWorldLightPacket != null)
                        return ReceivedWorldLightPacket(p);
                    break;*/
            }
            return true;
        }

        private void SendPacket(byte[] packet)
        {
            try
            {
                if (GameLoginSock.Connected)
                    GameLoginSock.Send(EncryptPacket(packet));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Tibia Chat - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (progressTimer != null) progressTimer.Dispose();
                if (pingTimer != null) pingTimer.Dispose();
                if (GameLoginSock.Connected)
                {
                    disconnecting = true;
                    GameLoginSock.BeginDisconnect(true, (AsyncCallback)GameDisconnect, null);
                }
                else
                    this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (System.Threading.ThreadStart)delegate()
                    {
                        _tabItemCollection.Clear();
                        AddTab("Default");
                        convosTab.SelectedItem = convosTab.Items[0];
                        loggedin = false;
                        messagetxt.IsEnabled = false;
                        convosTab.IsEnabled = false;
                        ConnectBtn.IsEnabled = true;
                        SendBtn.IsEnabled = false;
                        ConnectBtn.Content = "Connect";
                    });
            }
        }
        #endregion

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

        #region helper classes
        public class CharListChar
        {
            public short lenCharName { get; set; }
            public string charName { get; set; }
            public short lenWorldName { get; set; }
            public string worldName { get; set; }
            public string worldIP { get; set; }
            public short worldPort { get; set; }
        }
        
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
        #endregion

        #region connection packets
        public class Clientless
        {
            public static byte[] CreateCharListPacket(string accname, string password, string version, byte[] signatures, byte[] xtea)
            {
                PacketBuilder p = new PacketBuilder();
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
                PacketBuilder p = new PacketBuilder();
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
        #endregion
    }
}

