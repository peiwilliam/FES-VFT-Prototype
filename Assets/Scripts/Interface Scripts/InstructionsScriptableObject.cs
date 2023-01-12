using UnityEngine;

/// <summary>
/// This class handles the behaviour of the isntruction scriptable objects. Each scriptable object comes with a predefined instruction
/// text as well as a set picture.
/// </summary>
[CreateAssetMenu(fileName = "Instruction", menuName = "Instructions Scriptable Object", order = 0)]
public class InstructionsScriptableObject : ScriptableObject
{
    [Tooltip("The text to be used for the instruction step")]
    [TextArea(10, 14)] [SerializeField] private string _instructionsText;
    [Tooltip("Link to the previous instruction")]
    [SerializeField] private InstructionsScriptableObject _previousInstruction;
    [Tooltip("Link to the next instruction")]
    [SerializeField] private InstructionsScriptableObject _nextInstruction;
    [Tooltip("The image to be used for this object")]
    [SerializeField] private Sprite _image;

    /// <summary>
    /// Property to get the text associated with instruction scriptable object.
    /// </summary>
    public string InstructionText
    {
        get => _instructionsText;
    }

    /// <summary>
    /// Property to get the picture associated with instruction scriptable object.
    /// </summary>
    public Sprite Image
    {
        get => _image;
    }

    /// <summary>
    /// Property to get the next instruction scriptable object.
    /// </summary>
    public InstructionsScriptableObject NextInstruction
    {
        get => _nextInstruction;
    }

    /// <summary>
    /// Property to get the previous instruction scriptable object.
    /// </summary>
    public InstructionsScriptableObject PreviousInstruction
    {
        get => _previousInstruction;
    }
}