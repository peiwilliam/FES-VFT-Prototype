using UnityEngine;

[CreateAssetMenu(fileName = "Instruction", menuName = "Instructions Scriptable Object", order = 0)]
public class InstructionsScriptableObject : ScriptableObject
{
    [TextArea(10, 14)] [SerializeField] private string _instructionsText;
    [SerializeField] private Instructions _nextInstruction;
    [SerializeField] private SpriteRenderer _imageRef;

    public string GetInstructionsText() => _instructionsText;
}