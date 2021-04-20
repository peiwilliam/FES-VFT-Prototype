using UnityEngine;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

public class WiiBoardLoader : MonoBehaviour
{
    [SerializeField] private WiiBoard _wiiboard;

    private BluetoothClient _btClient;
    
    public void LoadWiiBoard()
    {
        Instantiate(_wiiboard, new Vector3(0, 0, 0), Quaternion.identity);
    }

    public void DisconnectBoard() //doesn't work consistently, might be because of unity editor
    {
        using(_btClient = new BluetoothClient())
        {
            foreach (var device in _btClient.DiscoverDevices())
            {
                if (device.DeviceName.Contains("Nintendo"))
                {
                    BluetoothSecurity.RemoveDevice(device.DeviceAddress);
                    device.SetServiceState(BluetoothService.HumanInterfaceDevice, false);
                    Debug.Log("Wii Device removed");
                }
            }
        }
    }
}
