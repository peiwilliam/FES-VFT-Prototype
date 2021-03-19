using System.Collections.Generic;
using FilterManager;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private Vector2 _initialCOP; //unused for now

    private const float _Length = 433; //units are mm
    private const float _Width = 228; //units are mm
    private CSVWriter _writer;
    private List<WiiBoardData> _dataList;
    
    private void Start() 
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            var initSenVals = new float[]
            {
                PlayerPrefs.GetFloat("Top Left Sensor"),
                PlayerPrefs.GetFloat("Top Right Sensor"),
                PlayerPrefs.GetFloat("Bottom Left Sensor"),
                PlayerPrefs.GetFloat("Bottom Right Sensor")
            };

            var copX = (initSenVals[0] + initSenVals[2] - initSenVals[1] - initSenVals[3]) / (initSenVals[0] + initSenVals[1] + initSenVals[2] + initSenVals[3]);
            var copY = (initSenVals[0] + initSenVals[1] - initSenVals[3] - initSenVals[4]) / (initSenVals[0] + initSenVals[1] + initSenVals[2] + initSenVals[3]);
            
            _initialCOP = new Vector2(copX, copY); //actually centre of pressure, this is not going to be exactly 0,0
            _writer = new CSVWriter();
            _dataList = new List<WiiBoardData>();

            _writer.WriteHeader();
        }
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>() && Wii.IsActive(0) && Wii.GetExpType(0) == 3)
        {
            var data = GetBoardValues();
            _dataList.Add(data);

            if (_dataList.Count >= 100)
            {
                _writer.WriteData(_dataList);
                _dataList.Clear();
            }

            var pos = new Vector2(transform.position.x, transform.position.y);
            pos.x = Mathf.Clamp(data.copX * _maxX / 2 + Camera.main.transform.position.x, _minX, _maxX);
            pos.y = Mathf.Clamp(data.copY * _maxY / 2 + Camera.main.transform.position.y, _minY, _maxY);

            if (true)
            {
                var filterX = new Filter(PlayerPrefs.GetFloat("Cutoff Frequency"), 
                                        PlayerPrefs.GetFloat("Sample Frequency"),
                                        PlayerPrefs.GetInt("Filter Order"));
                var filterY = new Filter(PlayerPrefs.GetFloat("Cutoff Frequency"), 
                                        PlayerPrefs.GetFloat("Sample Frequency"),
                                        PlayerPrefs.GetInt("Filter Order"));
                
                pos.x = filterX.Compute(pos.x);
                pos.y = filterY.Compute(pos.y);
            }

            transform.position = pos;
        }
        else
        {
            // debugging using mouse
            var pos = new Vector2(transform.position.x, transform.position.y);
            pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
            pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.height * _maxY, _minY, _maxY);
            transform.position = pos;
        }
    }

    private WiiBoardData GetBoardValues()
    {
        var boardSensorValues = Wii.GetBalanceBoard(0);
        var taredCOP = Wii.GetCenterOfBalance(0) - _initialCOP;
        var data = new WiiBoardData(Time.fixedUnscaledTime, 
                                    taredCOP.x, taredCOP.y, 
                                    boardSensorValues.y, 
                                    boardSensorValues.x, 
                                    boardSensorValues.w, 
                                    boardSensorValues.z);

        return data;
    }
}
