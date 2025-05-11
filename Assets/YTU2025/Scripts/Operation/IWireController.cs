using UnityEngine;

public interface IWireController
{
    void ConnectToSocket(SocketController socket);
    bool IsConnected();
    void Reset();
    int GetWireIndex();
}
