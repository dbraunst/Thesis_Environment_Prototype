// using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
    This is written using data collected from the following research study: 
    https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3407162/

*/

public enum PolarPatternEnum{
        linear,
        cardioid, 
        hyperCardioid
};

public class HorizontalAttenuation : MonoBehaviour
{
    static float ANGLE_INC_F = 15.0f;
    static int ANGLE_INC_I = 15;

    public AudioListener _listener;

    public AudioMixer _mixer;

    private AudioSource _audioSource;

    public PolarPatternEnum PolarPattern = new PolarPatternEnum();

    public float[] simpleAttenutation = new float[] {62.0f, 62.4f, 61.8f, 
        61.5f, 61.1f, 60.5f, 59.8f, 58.8f, 57.5f, 56.4f, 55.8f, 55.8f, 56.0f};
    public float[,] eightBandAtten = new float[8, 13] 
    {
        {50.2f, 51.0f, 50.1f, 49.8f, 49.6f, 49.1f, 48.6f, 48.1f, 47.5f, 47.1f, 46.7f, 46.6f, 46.6f},
        {56.0f, 56.7f, 55.8f, 55.5f, 55.2f, 54.4f, 53.7f, 52.9f, 52.1f, 51.5f, 51.1f, 51.0f, 50.9f},
        {57.4f, 58.0f, 57.5f, 57.4f, 57.3f, 56.7f, 55.8f, 54.6f, 53.0f, 51.9f, 51.4f, 51.4f, 51.5f},
        {55.0f, 54.8f, 54.3f, 53.6f, 53.5f, 53.6f, 53.4f, 52.5f, 50.4f, 48.0f, 47.1f, 48.1f, 48.7f},
        {51.2f, 50.2f, 51.1f, 50.4f, 48.7f, 46.5f, 44.2f, 43.6f, 43.3f, 41.9f, 38.2f, 36.2f, 38.1f},
        {46.3f, 44.6f, 45.6f, 44.7f, 42.4f, 41.0f, 40.0f, 37.3f, 33.6f, 31.8f, 29.6f, 25.3f, 27.4f},
        {46.4f, 45.4f, 44.7f, 43.3f, 40.6f, 39.4f, 37.7f, 34.9f, 31.4f, 28.6f, 24.9f, 21.4f, 20.1f},
        {38.0f, 37.9f, 37.5f, 35.7f, 33.6f, 32.9f, 30.8f, 27.9f, 23.8f, 20.2f, 16.3f, 14.5f, 11.3f}
    }; 

    private Vector3 _listenerVector;
    private Vector3 _listenerVector_n;
    private float dot;
    private float angleRad;
    private int angleDeg_abs;
    private float dot_n;
    private float cardioid;

    // For finding nearest angles
    private int nearest_angle;
    private int nearest_angle_low;
    private int nearest_angle_hi;

    // Indices
    private int nearest_angle_idx;
    private int nearest_angle_low_idx;
    private int nearest_angle_hi_idx;

    private float angle_lerp_amt;
    private float lerped_gain_db;
    private float lerped_gain_amp;

    private float tmp_lerped_gain_db;
    private float tmp_lerped_gain_amp;

    // Temp String for EQ things
    private string eqParamName;

    private float scaled_value;


    delegate void GetHorizontalAttenuation(float dot_product);
    // delegate void GetHorizontalAttentionation()    

    //**************************************************

    void Awake()
    {
        InitAttenuationArrays();
    }
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        initEQFreq();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Get + Normalize Vector from this object to the listener
        _listenerVector = _listener.gameObject.transform.position - transform.position; 
        _listenerVector_n = Vector3.Normalize(_listenerVector);

        // Get Dot products/angles between this source direction and the listener vector
        dot = Vector3.Dot(_listenerVector_n, this.transform.forward);
        dot_n = (dot + 1) * 0.5f; 

        angleRad = Mathf.Acos(dot);
        angleDeg_abs = (int)(angleRad * Mathf.Rad2Deg);


        // Get nearest angle via round, and low/hi as well
        nearest_angle = (int)(Mathf.Round((float)angleDeg_abs / ANGLE_INC_F) * ANGLE_INC_F);
        nearest_angle_low = (int)(Mathf.Floor((float)angleDeg_abs / ANGLE_INC_F) * ANGLE_INC_F);
        nearest_angle_hi = (int)(Mathf.Ceil((float)angleDeg_abs / ANGLE_INC_F) * ANGLE_INC_F);

        // Get Angle indices from the following
        nearest_angle_idx = nearest_angle / ANGLE_INC_I;
        nearest_angle_low_idx = nearest_angle_low / ANGLE_INC_I;
        nearest_angle_hi_idx = nearest_angle_hi / ANGLE_INC_I;

        // Calculate lerp amount based on position between lower and higher indices and lerp between retrieved values
        angle_lerp_amt = (float)(angleDeg_abs - nearest_angle_low) / ANGLE_INC_F;

        
        Debug.Log("Angle, lo, hi, lerp: " + angleDeg_abs + " " + nearest_angle_low + " " + nearest_angle_hi + " " + angle_lerp_amt);
        // Update the gain parameter of each EQ 
        for (int i = 0; i < 8; i++)
        {
            updateEQGain(i);
        }

        // // If using "simple attentuation", just affect master volume
        // lerped_gain_db = Mathf.Lerp(simpleAttenutation[nearest_angle_low_idx], simpleAttenutation[nearest_angle_hi_idx], angle_lerp_amt);
        // // Convert from dbSPL (>= 0 ) to dBFs (<= 0), and then from DBFS to amplitude (0 <= amp =<= 1.0f)
        // lerped_gain_amp = (float)Mathf.Pow(10, (lerped_gain_db - 62.5f) / 20);
        // _audioSource.volume = lerped_gain_amp;

        //Temp Polar Pattern Stuff
        // cardioid = 1 + Mathf.Cos(angleRad);
    }

    public void InitAttenuationArrays(){
        Debug.Log("Simple Atten: " + string.Join(" ", 
            new List<float>(simpleAttenutation)
            .ConvertAll(i => i.ToString())
            .ToArray()));
    }

    public void initEQFreq() {
        int startFreq = 125;

        // Loops thru EQ's and sets center frequency to 125, 250, 500, ... 16k 
        for (int i = 0; i < 8; i++) {
            eqParamName = "EQ_Freq_" + i;
            Debug.Log("SetFreq: " + eqParamName);
            _mixer.SetFloat(eqParamName, startFreq);
            startFreq *= 2;
        }
    }

    public void updateEQGain(int band) {
        // Original sources are scaled to 114dB reference ( assumption +20dB with 0 at 94 == 1 (Pa))

        eqParamName = "EQ_Gain_" + band;

        // Retrieve db values at index by band, lerp between them based on partial position, convert dBSPL-> dBFS -> amplitude
        tmp_lerped_gain_db = Mathf.Lerp(eightBandAtten[band, nearest_angle_low_idx], eightBandAtten[band, nearest_angle_hi_idx], angle_lerp_amt);
        

        // Note: this first approachfelt 'overly' quiet and pronounced. I think unity does Amp->db calculation under the hood
        // tmp_lerped_gain_amp = (float)Mathf.Pow(10, (tmp_lerped_gain_db - 58.0f) / 20);

        // Debug.Log("Band " + band + " Gain [dB, amp]" + tmp_lerped_gain_db + " " + tmp_lerped_gain_amp + "]");
        
        
        // Alternate attempt, scaling db range with the following formula: 
        //  (V * R2 / R1) + (M2 - M1)
        // V = the value you want to convert. 
        // R1 and R2 are both differential values of each ranges (maximum value - minimum value). 
        // M1 and M2 are both minimal values of each range.
        // 
        // Input Range = 60db - 0db = 94
        // Output Range = 1 - 0
        scaled_value = (tmp_lerped_gain_db * 1 / 80);
        _mixer.SetFloat(eqParamName, scaled_value);

        // Debug.Log("Band " + band + " Gain [dB, amp]" + tmp_lerped_gain_db + " " + scaled_value + "]");
    }
}


// Debug.Log("Normalized Dot Product, Cardioid Val: [" + dot_n + "," + cardioid + "].");
// Debug.Log("Angle [rad, deg, nearest]: [" + angleRad + "," + angleDeg_abs + " " +  nearest_angle + "]. ");
// Debug.Log("Angle [deg, lo, hi, near]: [" + angleDeg_abs + "," + nearest_angle_low + "," + nearest_angle_hi +  "," +  nearest_angle + "]");
// Debug.Log(angleDeg_abs + " " + (float)angle_lerp_amt);
// Debug.Log(angleDeg_abs + " " + nearest_angle_low + " " + nearest_angle_hi + " " + angle_lerp_amt);
// Debug.Log("Gain Lo, Gain Hi, Lerped Gain[dB], Gain[amp]: " 
//     + simpleAttenutation[nearest_angle_low_idx] + " " + simpleAttenutation[nearest_angle_hi_idx]+ " " 
//     + lerped_gain_db + " " + lerped_gain_amp + "]");