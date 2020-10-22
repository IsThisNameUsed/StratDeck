using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSizeSlider : MonoBehaviour
{
    public Text value;

    public void ChangeValue()
    {
        string newValue = gameObject.GetComponent<Slider>().value.ToString();
        float newValueFloat = float.Parse(newValue);
        Debug.Log(newValueFloat);
        newValue = newValueFloat.ToString();
        value.text = newValue;
    }
}
