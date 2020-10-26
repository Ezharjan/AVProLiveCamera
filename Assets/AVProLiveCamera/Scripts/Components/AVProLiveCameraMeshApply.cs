using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2017 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

[AddComponentMenu("AVPro Live Camera/Mesh Apply")]
public class AVProLiveCameraMeshApply : MonoBehaviour 
{
	public MeshRenderer _mesh;
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
		if (_mesh != null)
		{
			foreach (Material m in _mesh.materials)
			{
				m.mainTexture = texture;
			}
		}
	}
	
	public void OnDisable()
	{
		ApplyMapping(null);
	}
}
