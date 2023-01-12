using UnityEngine;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

/// <summary>
/// This class is repsonsible for creating the WiiBoard object and also handling Wii Balance Board disconnection via the InTheHand
/// bluetooth .NET library.
/// </summary>
public class WiiBoardLoader : MonoBehaviour
{
    [Tooltip("For storing the wii board object")]
    [SerializeField] private WiiBoard _wiiboard;
    
    /// <summary>
    /// This method creates the WiiBoard object to start the Wii Board Connection.
    /// </summary>
    public void LoadWiiBoard() => Instantiate(_wiiboard, new Vector3(0, 0, 0), Quaternion.identity);

    /// <summary>
    /// This method copies the PIN to the computer's clipboard so that it can pasted when the person goes to connect the board to the
    /// computer.
    /// </summary>
    public void CopyPIN() => GUIUtility.systemCopyBuffer = FindObjectOfType<BluetoothPin>().BTPin;

    /// <summary>
    /// This method disconnects any Wii Balance Boards to the computer. Doesn't consistently work, but this may be because of Unity.
    /// </summary>
    public void DisconnectBoard()
    {
        using(var btClient = new BluetoothClient())
        {
            foreach (var device in btClient.DiscoverDevices())
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
