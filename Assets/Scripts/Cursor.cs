using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    [SerializeField] private Vector2 _initialCOP; //unused for now

    // private const float _Length = 433; //units are mm
    // private const float _Width = 228; //units are mm
    private CSVWriter _writer;
    private List<WiiBoardData> _dataList;
    
    private void Start() 
    {
        //_initialCOP = Wii.GetCenterOfBalance(0); //actually centre of pressure, this is not going to be exactly 0,0
        _writer = new CSVWriter();
        _writer.WriteHeader();
        _dataList = new List<WiiBoardData>();
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
            CollectAndWriteBoardData(data);

            var pos = new Vector2(transform.position.x, transform.position.y);
            pos.x = Mathf.Clamp(data.COPx * _maxX / 2 + Camera.main.transform.position.x, _minX, _maxX);
            pos.y = Mathf.Clamp(data.COPy * _maxY / 2 + Camera.main.transform.position.y, _minY, _maxY);
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
        var centreCOP = Wii.GetCenterOfBalance(0) - _initialCOP;
        var data = new WiiBoardData
        {
            Time = Time.fixedUnscaledTime,
            COPx = centreCOP.x,
            COPy = centreCOP.y,
            TopLeft = boardSensorValues.y,
            TopRight = boardSensorValues.x,
            BottomLeft = boardSensorValues.w,
            BottomRight = boardSensorValues.z
        };

        return data;
    }

    private void CollectAndWriteBoardData(WiiBoardData data)
    {
        _dataList.Add(data);

        if (_dataList.Count >= 100)
        {
            _writer.WriteData(data);
            _dataList.Clear();
        }
    }
}
