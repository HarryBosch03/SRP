using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BMRP.Runtime.Assets;
using BMRP.Runtime.Core;
using BMRP.Runtime.PostFX;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class QuickSettings : MonoBehaviour
{
    [SerializeField] private BmrPipelineAsset pipeline;

    private TMP_InputField
        fov,
        downscale,
        wobble;

    private void Awake()
    {
        fov = transform.DeepFind("Fov Input").GetComponent<TMP_InputField>();
        downscale = transform.DeepFind("Downscale Input").GetComponent<TMP_InputField>();
        wobble = transform.DeepFind("Wobble Input").GetComponent<TMP_InputField>();
    }

    static void SyncField(TMP_InputField field, Action<float> callback)
    {
        if (float.TryParse(field.text, out var v))
        {
            callback(v);
        }
    }

    private void Update()
    {
        SyncField(fov, v => Camera.main.fieldOfView = v);
        SyncField(downscale, v => ((DownscaleEffect)pipeline.Settings.postFXSettings.Effects.First(e => e.GetType() == typeof(DownscaleEffect))).factor = (int)v);
        SyncField(wobble, v => pipeline.Settings.globalVertexWobbleAmount = v);
    }
}