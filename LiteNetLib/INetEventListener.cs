using System;
using LiteNetLib.Utils;

namespace LiteNetLib
{
    public enum UnconnectedMessageType
    {
        Default,
        DiscoveryRequest,
        DiscoveryResponse
    }

    public enum DisconnectReason
    {
        SocketReceiveError,
        ConnectionFailed,
        Timeout,
        SocketSendError,
        RemoteConnectionClose,
        DisconnectPeerCalled
    }

    internal enum ConnectionRequestResult
    {
        Accept,
        Reject
    }

    public struct DisconnectInfo
    {
        public DisconnectReason Reason;
        public int SocketErrorCode;
        public NetDataReader AdditionalData;
    }

    public class ConnectionRequest
    {
        private readonly Action<ConnectionRequest, ConnectionRequestResult> _onUserAction;
        private bool _used;

        public readonly long ConnectionId;
        public readonly NetEndPoint RemoteEndPoint;
        public readonly NetDataReader Data;

        internal ConnectionRequest(
            long connectionId, 
            NetEndPoint remoteEndPoint, 
            NetDataReader netDataReader,
            Action<ConnectionRequest, ConnectionRequestResult> onUserAction)
        {
            ConnectionId = connectionId;
            RemoteEndPoint = remoteEndPoint;
            Data = netDataReader;
            _onUserAction = onUserAction;
        }

        public void Accept()
        {
            if (_used)
                return;
            _used = true;
            _onUserAction(this, ConnectionRequestResult.Accept);
        }

        public void Reject()
        {
            if (_used)
                return;
            _used = true;
            _onUserAction(this, ConnectionRequestResult.Reject);
        }
    }

    public interface INetEventListener
    {
        /// <summary>
        /// New remote peer connected to host, or client connected to remote host
        /// </summary>
        /// <param name="peer">Connected peer object</param>
        void OnPeerConnected(NetPeer peer);

        /// <summary>
        /// Peer disconnected
        /// </summary>
        /// <param name="peer">disconnected peer</param>
        /// <param name="disconnectInfo">additional info about reason, errorCode or data received with disconnect message</param>
        void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);

        /// <summary>
        /// Network error (on send or receive)
        /// </summary>
        /// <param name="endPoint">From endPoint (can be null)</param>
        /// <param name="socketErrorCode">Socket error code</param>
        void OnNetworkError(NetEndPoint endPoint, int socketErrorCode);

        /// <summary>
        /// Received some data
        /// </summary>
        /// <param name="peer">From peer</param>
        /// <param name="reader">DataReader containing all received data</param>
        void OnNetworkReceive(NetPeer peer, NetDataReader reader);

        /// <summary>
        /// Received unconnected message
        /// </summary>
        /// <param name="remoteEndPoint">From address (IP and Port)</param>
        /// <param name="reader">Message data</param>
        /// <param name="messageType">Message type (simple, discovery request or responce)</param>
        void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType);

        /// <summary>
        /// Latency information updated
        /// </summary>
        /// <param name="peer">Peer with updated latency</param>
        /// <param name="latency">latency value in milliseconds</param>
        void OnNetworkLatencyUpdate(NetPeer peer, int latency);
    }

    public class EventBasedNetListener : INetEventListener
    {
        public delegate void OnPeerConnected(NetPeer peer);
        public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
        public delegate void OnNetworkError(NetEndPoint endPoint, int socketErrorCode);
        public delegate void OnNetworkReceive(NetPeer peer, NetDataReader reader);
        public delegate void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType);
        public delegate void OnNetworkLatencyUpdate(NetPeer peer, int latency);

        public event OnPeerConnected PeerConnectedEvent;
        public event OnPeerDisconnected PeerDisconnectedEvent;
        public event OnNetworkError NetworkErrorEvent;
        public event OnNetworkReceive NetworkReceiveEvent;
        public event OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;
        public event OnNetworkLatencyUpdate NetworkLatencyUpdateEvent; 
         
        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if (PeerConnectedEvent != null)
                PeerConnectedEvent(peer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (PeerDisconnectedEvent != null)
                PeerDisconnectedEvent(peer, disconnectInfo);
        }

        void INetEventListener.OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            if (NetworkErrorEvent != null)
                NetworkErrorEvent(endPoint, socketErrorCode);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            if (NetworkReceiveEvent != null)
                NetworkReceiveEvent(peer, reader);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            if (NetworkReceiveUnconnectedEvent != null)
                NetworkReceiveUnconnectedEvent(remoteEndPoint, reader, messageType);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            if (NetworkLatencyUpdateEvent != null)
                NetworkLatencyUpdateEvent(peer, latency);
        }
    }
}
