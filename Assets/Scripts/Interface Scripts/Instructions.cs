using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class is responsible for determining the behaviour of the instructions page for connecting the Wii Balance Board to the
/// program.
/// </summary>
public class Instructions : MonoBehaviour
{
    [Tooltip("Place to put the starting instruction scriptable object so that the program knows where to start")]
    [SerializeField] private InstructionsScriptableObject _startingInstruction;
    [Tooltip("The text that is displayed at the current step. For debugging purposes only")]
    [SerializeField] private TMP_Text _instructionText;
    [Tooltip("The image that is displayed at the current step. For debugging purposes only")]
    [SerializeField] private RawImage _image;

    private InstructionsScriptableObject _step; //stores the current step
    private float _defaultYLevelOfText; //keeps a default y level for the text when there is a picture

    private void Start() //runs only at the beginning when the object is instantiated
    {
        // start off with the initial instruction
        ManageStep(_startingInstruction);
        _defaultYLevelOfText = -328f;
    }

    private void ManageStep(InstructionsScriptableObject stepToGo) //manages the text and picture depending on the text and picture
    {
        _step = stepToGo;
        _instructionText.text = _step.InstructionText;

        if (_step.Image == null) //if the image hasn't been set, set the texture to null and set the alpha to 0 so it doesn't appear
        {
            _image.texture = null;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);
            _instructionText.rectTransform.localPosition = new Vector3(0f, -100f);

            return;
        }

        _image.texture = _step.Image.texture;
        _image.rectTransform.localScale = new Vector3(_step.Image.texture.width, _step.Image.texture.height); //need to rescale the image according to how big the image actually is
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1f); //put the alpha back to 1
        _instructionText.rectTransform.localPosition = new Vector3(0f, _defaultYLevelOfText);
    }

    /// <summary>
    /// This method is attached to the next button on the instructions page and switches the instruction scriptable object being used.
    /// </summary>
    public void NextStep()
    {
        if (_step.NextInstruction == null) //loop back to the beginning if we're at the end
        {
            ManageStep(_startingInstruction);

            return;
        }
            
        ManageStep(_step.NextInstruction);
    }
    
    /// <summary>
    /// This method is attached to the previous button on the instructions page and switches the instruction scriptable object being used.
    /// </summary>
    public void PreviousStep()
    {
        if (_step.PreviousInstruction != null)
            ManageStep(_step.PreviousInstruction);
    }
}
