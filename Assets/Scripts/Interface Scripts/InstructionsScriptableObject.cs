using UnityEngine;

[CreateAssetMenu(fileName = "Instruction", menuName = "Instructions Scriptable Object", order = 0)]
public class InstructionsScriptableObject : ScriptableObject
{
    [TextArea(10, 14)] [SerializeField] private string _instructionsText;
    [SerializeField] private InstructionsScriptableObject _previousInstruction;
    [SerializeField] private InstructionsScriptableObject _nextInstruction;
    [SerializeField] private Sprite _image;

    public string InstructionText
    {
        get => _instructionsText;
    }

    public Sprite Image
    {
        get => _image;
    }

    public InstructionsScriptableObject NextInstruction
    {
        get => _nextInstruction;
    }

    public InstructionsScriptableObject PreviousInstruction
    {
        get => _previousInstruction;
    }
}