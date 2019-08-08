using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.WSA;

public class EqualizerManager : MonoBehaviour
{
    [SerializeField] BytesVariable UDPData;
    [SerializeField] GameObject cubePrefab;
    private GameObject[,] cubeArray;
    void Start()
    {
        InitializeCubeArray();
        //UDPData.onChange += UpdateEqualizer;
    }

    private void Update()
    {
        UpdateEqualizer();
    }

    void UpdateEqualizer()
    {
        if (UDPData.Value.Length == 0) return;
        string text_data = Encoding.UTF8.GetString(UDPData.Value);
        text_data = text_data.Remove(0, 1); //Supprimer le premier [
        text_data = text_data.Remove(text_data.Length - 1); //Supprimer le dernier ]
        string[] array_text_data = text_data.Split(',');
        float[] array_float_data = new float[array_text_data.Length];
        int n_data = array_float_data.Length;
        for (int i = 0; i < array_text_data.Length; i++)
        {
            array_float_data[i] = float.Parse(array_text_data[i]);
        }
        /*
        array_float_data est de taille n. On veut garder disons 16 points. On fait n/16 si on veut les répartir uniformément
        point 0 : 0Hz ; point n : 22050Hz (parce que le micro enregistre à 44100 points/seconde)
        Si je veux récupérer les valeurs de 0Hz, 100Hz, ..., 1000Hz :
        100Hz correspond au point 100*n/22050
        valeur = array_float_data[]
        */
        float[] array_value_equalizer = new float[16];
        int n_eq = array_value_equalizer.Length;
        // Je récupère les valeurs 0Hz, 100Hz, ..., 1500Hz
        int[] desired_frequencies = { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500 };
        for (int i = 0; i < array_value_equalizer.Length - 1; i++)
        {
            array_value_equalizer[i] = array_float_data[desired_frequencies[i] * n_data / 22050];
        }
        // string debug = "";
        // for (int i = 0; i < n_eq - 1; i++)
        //     debug += array_value_equalizer[i] + " ";
        // print(debug);

        //Je scale les valeurs entre de 30 et 70. J'ai 10 cubes en hauteur. C'est un système d'équation : 
        //a*30 + b = 0 ; a * 70 + b = 10 <=> a = 0.25 ; b = -0.25*30
        float[] values = SolveLinEq(40, 90);
        for (int i = 0; i < n_eq - 1; i++)
        {
            for (int j = 0; j < 10 - 1; j++)
            {
                float seuil = (values[0] * array_value_equalizer[i] + values[1]);
                if (j > seuil)
                    cubeArray[i, j].SetActive(false);
                else
                    cubeArray[i, j].SetActive(true);
            }
        }

    }

    void InitializeCubeArray(int columns = 16, int rows = 10)
    {
        cubeArray = new GameObject[columns, rows];
        Color[] colors = GetColors();
        for (int i = 0; i < columns - 1; i++)
        {
            for (int j = 0; j < rows - 1; j++)
            {
                cubeArray[i, j] = GameObject.Instantiate(cubePrefab, new Vector3(i, j, 0), Quaternion.identity);
                cubeArray[i, j].GetComponent<Renderer>().material.color = colors[i];
            }
        }
    }

    Color[] GetColors()
    {
        List<Color> colors = new List<Color>();
        Color test = new Vector4();
        colors.Add(new Vector4(255, 0, 0, 0) / 255);     // Red
        colors.Add(new Vector4(255, 0, 127, 0) / 255);
        colors.Add(new Vector4(255, 0, 255, 0) / 255);   // Magenta
        colors.Add(new Vector4(127, 0, 255, 0) / 255);   // Purple
        colors.Add(new Vector4(0, 0, 255, 0) / 255);     // Blue
        colors.Add(new Vector4(0, 127, 255, 0) / 255);
        colors.Add(new Vector4(0, 255, 255, 0) / 255);   // Cyan
        colors.Add(new Vector4(0, 255, 127, 0) / 255);
        colors.Add(new Vector4(0, 255, 0, 0) / 255);     // Green
        colors.Add(new Vector4(127, 255, 0, 0) / 255);
        colors.Add(new Vector4(255, 255, 0, 0) / 255);   // Yellow
        colors.Add(new Vector4(255, 127, 0, 0) / 255);   // Orange
        colors.Add(new Vector4(255, 0, 0, 0) / 255);     // Red
        colors.Add(new Vector4(255, 0, 127, 0) / 255);
        colors.Add(new Vector4(255, 0, 255, 0) / 255);   // Magenta
        colors.Add(new Vector4(127, 0, 255, 0) / 255);   // Purple
        return colors.ToArray();
    }

    float[] SolveLinEq(float lowval, float highval, int heightCubesAmount = 10)
    {
        float[] values = new float[2];
        float a = heightCubesAmount / (highval - lowval); //10 parce que 10 cubes en hauteur
        float b = -a * lowval;
        values[0] = a;
        values[1] = b;
        return values;
    }
}
