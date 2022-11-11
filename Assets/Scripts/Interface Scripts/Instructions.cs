using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Instructions : MonoBehaviour
{
    [SerializeField] private InstructionsScriptableObject _startingInstruction;
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private RawImage _image;

    private InstructionsScriptableObject _step; //stores the current step
    private float _defaultYLevelOfText; //keeps a default y level for the text when there is a picture

    private void Start()
    {
        // start off with the initial instruction
        ManageStep(_startingInstruction);
        _defaultYLevelOfText = -328f;
    }

    private void ManageStep(InstructionsScriptableObject stepToGo)
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

    public void NextStep()
    {
        if (_step.NextInstruction == null)
        {
            ManageStep(_startingInstruction);

            return;
        }
            
        ManageStep(_step.NextInstruction);
    }
    
    public void PreviousStep()
    {
        if (_step.PreviousInstruction != null)
            ManageStep(_step.PreviousInstruction);
    }
}
