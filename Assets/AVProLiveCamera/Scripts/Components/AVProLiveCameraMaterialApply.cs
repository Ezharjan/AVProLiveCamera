using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2017 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

[AddComponentMenu("AVPro Live Camera/Material Apply")]
public class AVProLiveCameraMaterialApply : MonoBehaviour 
{
	public Material _material;
	public AVProLiveCamera _liveCamera;
	
	void Start()
	{
		if (_liveCamera != null && _liveCamera.OutputTexture != null)
		{
			ApplyMapping(_liveCamera.OutputTexture);
		}
	}
	
	void Update()
	{
		if (_liveCamera != null && _liveCamera.OutputTexture != null)
		{
			ApplyMapping(_liveCamera.OutputTexture);
		}
	}
	
	private void ApplyMapping(Texture texture)
	{
		if (_material != null)
		{
			_material.mainTexture = texture;
		}
	}
	
	public void OnDisable()
	{
		ApplyMapping(null);
	}
}
