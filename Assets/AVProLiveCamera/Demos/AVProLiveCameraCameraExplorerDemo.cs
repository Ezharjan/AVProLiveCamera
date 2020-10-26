using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AVProLiveCameraCameraExplorerDemo : MonoBehaviour
{
	public GUISkin _guiSkin;
	public bool _updateDeviceSettings;
	private List<Vector2> _scrollPos = new List<Vector2>();
	private List<Vector2> _scrollVideoInputPos = new List<Vector2>();
	private Vector2 _horizScrollPos = Vector2.zero;

	private GUIStyle _buttonStyle;
	
	private Texture _zoomed = null;
	private const float ZoomTime = 0.25f;
	private float _zoomTimer;
	private bool _zoomUp;
	private Rect _zoomSrcDest;

	private static int _propApplyGamma;
	private static Shader _shaderGammaConversion;
	private Material _material;

	void Awake()
	{
		if (_propApplyGamma == 0)
		{
			_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
		}
	}

	public void Start()
	{
		Application.runInBackground = true;

		if (_shaderGammaConversion == null)
		{
			_shaderGammaConversion = Shader.Find("Hidden/AVProLiveCamera/IMGUI");
		}

		EnumerateDevices();

		int numDevices = AVProLiveCameraManager.Instance.NumDevices;
		for (int i = 0; i < numDevices; i++)
		{
			AVProLiveCameraDevice device = AVProLiveCameraManager.Instance.GetDevice(i);

			// Optionally update various camera internals, depending on which features are required
			device.UpdateHotSwap = AVProLiveCameraManager.Instance._supportHotSwapping;
			device.UpdateFrameRates = true;
			device.UpdateSettings = _updateDeviceSettings;
		}
	}

	void OnDestroy()
	{
		// Destroy existing material
		if (_material != null)
		{
#if UNITY_EDITOR
			Material.DestroyImmediate(_material);
#else
			Material.Destroy(_material);
#endif
			_material = null;
		}
	}

	private static Shader GetRequiredShader()
	{
		Shader result = null;

		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			result = _shaderGammaConversion;
		}

		return result;
	}

	private void EnumerateDevices()
	{
		// Enumerate all cameras
		int numDevices = AVProLiveCameraManager.Instance.NumDevices;
		print("num devices: " + numDevices);
		for (int i = 0; i < numDevices; i++)
		{
			AVProLiveCameraDevice device = AVProLiveCameraManager.Instance.GetDevice(i);
			
			// Enumerate video inputs (only for devices with multiple analog input sources, eg TV cards)
			print("device " + i + ": " + device.Name + " has " + device.NumVideoInputs + " videoInputs");
			for (int j = 0; j < device.NumVideoInputs; j++)
			{
				print("  videoInput " + j + ": " + device.GetVideoInputName(j));
			}
			
			// Enumerate camera modes
			print("device " + i + ": " + device.Name + " has " + device.NumModes + " modes");
			for (int j = 0; j < device.NumModes; j++)
			{
				AVProLiveCameraDeviceMode mode = device.GetMode(j);
				print("  mode " + j + ": " + mode.Width + "x" + mode.Height + " @" + mode.FPS.ToString("F2") + "fps [" + mode.Format + "]");
			}

			// Enumerate camera settings
			print("device " + i + ": " + device.Name + " has " + device.NumSettings + " video settings");
			for (int j = 0; j < device.NumSettings; j++)
			{
				AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByIndex(j);
				switch (settingBase.DataTypeValue)
				{
					case AVProLiveCameraSettingBase.DataType.Boolean:
						{
							AVProLiveCameraSettingBoolean settingBool = (AVProLiveCameraSettingBoolean)settingBase;
							print(string.Format("  setting {0}: {1}({2}) value:{3} default:{4} canAuto:{5} isAuto:{6}", j, settingBase.Name, settingBase.PropertyIndex, settingBool.CurrentValue, settingBool.DefaultValue, settingBase.CanAutomatic, settingBase.IsAutomatic));
						}
						break;
					case AVProLiveCameraSettingBase.DataType.Float:
						{
							AVProLiveCameraSettingFloat settingFloat = (AVProLiveCameraSettingFloat)settingBase;
							print(string.Format("  setting {0}: {1}({2}) value:{3} default:{4} range:{5}-{6} canAuto:{7} isAuto:{8}", j, settingBase.Name, settingBase.PropertyIndex, settingFloat.CurrentValue, settingFloat.DefaultValue, settingFloat.MinValue, settingFloat.MaxValue, settingBase.CanAutomatic, settingBase.IsAutomatic));
						}
						break;
				}
			}
			
			_scrollPos.Add(new Vector2(0, 0));
			_scrollVideoInputPos.Add(new Vector2(0, 0));
		}		
	}

	private void UpdateCameras()
	{
		// Update all cameras
		int numDevices = AVProLiveCameraManager.Instance.NumDevices;
		for (int i = 0; i < numDevices; i++)
		{
			AVProLiveCameraDevice device = AVProLiveCameraManager.Instance.GetDevice(i);

			// Update the actual image
			device.Update(false);
		}
	}

	private void UpdateMaterial()
	{
		// Get required shader
		Shader currentShader = null;
		if (_material != null)
		{
			currentShader = _material.shader;
		}
		Shader nextShader = GetRequiredShader();

		// If the shader requirement has changed
		if (currentShader != nextShader)
		{
			// Destroy existing material
			if (_material != null)
			{
#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}

			// Create new material
			if (nextShader != null)
			{
				_material = new Material(nextShader);
				if (_material.HasProperty(_propApplyGamma))
				{
					if (QualitySettings.activeColorSpace == ColorSpace.Linear)
					{
						_material.EnableKeyword("APPLY_GAMMA");
					}
					else
					{
						_material.DisableKeyword("APPLY_GAMMA");
					}
				}
			}
		}
	}

	private int _lastFrameCount;
	void OnRenderObject()
	{
		if (_lastFrameCount != Time.frameCount)
		{
			_lastFrameCount = Time.frameCount;

			UpdateCameras();
		}
	}

	public void Update()
	{
		UpdateMaterial();

		// Handle mouse click to unzoom
		if (_zoomed != null)
		{
			if (_zoomUp)
			{
				if (Input.GetMouseButtonDown(0))
				{
					_zoomTimer = ZoomTime;
					_zoomUp = false;
				}
				else
				{
					_zoomTimer += Time.deltaTime;
				}
				
			}
			else
			{
				if (_zoomTimer <= 0.0f)
				{
					_zoomed = null;
				}
				_zoomTimer -= Time.deltaTime;
			}
		}
	}
	
	public void NewDeviceAdded()
	{
		EnumerateDevices();
	}
	
	public void OnGUI()
	{
		if (_guiSkin != null)
		{
			if (_buttonStyle == null)
			{
				_buttonStyle = _guiSkin.FindStyle("LeftButton");
			}
			GUI.skin = _guiSkin;
		}
		if (_buttonStyle == null)
		{
			_buttonStyle = GUI.skin.button;
		}
		
		_horizScrollPos = GUILayout.BeginScrollView(_horizScrollPos, false, false);
		GUILayout.BeginHorizontal();
		for (int i = 0; i < AVProLiveCameraManager.Instance.NumDevices; i++)
		{		
			GUILayout.BeginVertical("box", GUILayout.MaxWidth(300));
			
			AVProLiveCameraDevice device = AVProLiveCameraManager.Instance.GetDevice(i);
			
			GUI.enabled = device.IsConnected;
		
			Rect cameraRect = GUILayoutUtility.GetRect(300, 168);
			if (GUI.Button(cameraRect, ""))
			{
				if (_zoomed == null)
				{
					_zoomed = device.OutputTexture;
					_zoomSrcDest = cameraRect;
					_zoomUp = true;;
				}
			}

			// Thumbnail image
			if (device.OutputTexture != null)
			{
				if (_material != null)
				{
					DrawTexture(cameraRect, device.OutputTexture, ScaleMode.ScaleToFit, _material);
				}
				else
				{
					GUI.DrawTexture(cameraRect, device.OutputTexture, ScaleMode.ScaleToFit, false);
				}
			}
							
			GUILayout.Box("Camera " + i + ": " + device.Name);
			if (!device.IsRunning)
			{
				GUILayout.Box("Stopped");
				if (GUILayout.Button("Start\n(using default/last mode)"))
				{
					if (_zoomed == null)
					{
						device.Start(-1);
					}
				}
			}
			else
			{
				GUILayout.Box(string.Format("{0}x{1} {2}", device.CurrentWidth, device.CurrentHeight, device.CurrentFormat));
				GUILayout.Box(string.Format("Capture Rate {0}hz", device.CaptureFPS.ToString("F2")));
				GUILayout.Box(string.Format("Display Rate {0}hz", device.DisplayFPS.ToString("F2")));
				if (GUILayout.Button("Stop"))
				{
					if (_zoomed == null)
					{
						device.Close();
					}
				}
			}
			GUI.enabled = device.CanShowConfigWindow();
			if (GUILayout.Button("Configure"))
			{
				if (_zoomed == null)
				{
					device.ShowConfigWindow();
				}
			}
			GUI.enabled = true;
			
			if (device.NumVideoInputs > 0)
			{
				GUILayout.Label("Select a video input:");
				_scrollVideoInputPos[i] = GUILayout.BeginScrollView(_scrollVideoInputPos[i], false, false); 
				for (int j = 0; j < device.NumVideoInputs; j++)
				{	
					if (GUILayout.Button(device.GetVideoInputName(j)))
					{
						if (_zoomed == null)
						{
							// Start selected device
							device.Close();
							device.Start(-1, j);
						}
					}
				}
				GUILayout.EndScrollView();
			}
			
			if (device.Deinterlace != GUILayout.Toggle(device.Deinterlace, "Deinterlace", GUILayout.ExpandWidth(true)))
			{
				device.Deinterlace = !device.Deinterlace;
				if (device.IsRunning)
				{
					device.Close();
					device.Start(-1, -1);
				}
			}
			
			GUILayout.BeginHorizontal();
			device.FlipX = GUILayout.Toggle(device.FlipX, "Flip X", GUILayout.ExpandWidth(true));
			device.FlipY = GUILayout.Toggle(device.FlipY, "Flip Y", GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			_scrollPos[i] = GUILayout.BeginScrollView(_scrollPos[i], false, false); 

			if (device.NumSettings > 0)
			{
				GUILayout.Label("Settings:");
				device.UpdateSettings = GUILayout.Toggle(device.UpdateSettings, "Live Update", GUILayout.ExpandWidth(true));
				
				for (int j = 0; j < device.NumSettings; j++)
				{
					AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByIndex(j);
					GUILayout.BeginHorizontal();
					GUI.enabled = !settingBase.IsAutomatic;
					if (GUILayout.Button("D", GUILayout.ExpandWidth(false)))
					{
						settingBase.SetDefault();
					}
					GUI.enabled = true;
					GUILayout.Label(settingBase.Name, GUILayout.ExpandWidth(false));
					GUI.enabled = !settingBase.IsAutomatic;
					switch (settingBase.DataTypeValue)
					{
						case AVProLiveCameraSettingBase.DataType.Boolean:
							AVProLiveCameraSettingBoolean settingBool = (AVProLiveCameraSettingBoolean)settingBase;
							settingBool.CurrentValue = GUILayout.Toggle(settingBool.CurrentValue, "", GUILayout.ExpandWidth(true));
							break;
						case AVProLiveCameraSettingBase.DataType.Float:
							AVProLiveCameraSettingFloat settingFloat = (AVProLiveCameraSettingFloat)settingBase;
							settingFloat.CurrentValue = GUILayout.HorizontalSlider(settingFloat.CurrentValue, settingFloat.MinValue, settingFloat.MaxValue, GUILayout.ExpandWidth(true));

							GUI.enabled = settingBase.CanAutomatic;
							settingBase.IsAutomatic = GUILayout.Toggle(settingBase.IsAutomatic, "", GUILayout.Width(32.0f));
							GUI.enabled = true;

							break;

					}
					GUI.enabled = true;
					GUILayout.EndHorizontal();
				}

				if (GUILayout.Button("Defaults"))
				{
					for (int j = 0; j < device.NumSettings; j++)
					{
						AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByIndex(j);
						settingBase.SetDefault();
					}
				}
			}

			
			GUILayout.Label("Select a mode:");

			for (int j = 0; j < device.NumModes; j++)
			{	
				AVProLiveCameraDeviceMode mode = device.GetMode(j);
				if (GUILayout.Button("" + mode.Width + "x" + mode.Height + " " + mode.FPS.ToString("F2") + "hz " + "[" + mode.Format + "]", _buttonStyle))
				{
					if (_zoomed == null)
					{
						// Start selected device
						device.Close();
						Debug.Log("Selecting mode: " + j);
						device.Start(mode);
					}
				}
			}
			GUILayout.EndScrollView();
			
			
			GUILayout.EndVertical();
		}
		
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		
		// Show zoomed camera image
		if (_zoomed != null)
		{
			Rect fullScreenRect = new Rect(0f, 0f, Screen.width, Screen.height);
			
			float t = Mathf.Clamp01(_zoomTimer / ZoomTime);
			t = Mathf.SmoothStep(0f, 1f, t);
			Rect r = new Rect();
			r.x = Mathf.Lerp(_zoomSrcDest.x, fullScreenRect.x, t);
			r.y = Mathf.Lerp(_zoomSrcDest.y, fullScreenRect.y, t);
			r.width = Mathf.Lerp(_zoomSrcDest.width, fullScreenRect.width, t);
			r.height = Mathf.Lerp(_zoomSrcDest.height, fullScreenRect.height, t);

			if (_material != null)
			{
				DrawTexture(r, _zoomed, ScaleMode.ScaleToFit, _material);
			}
			else
			{
				GUI.DrawTexture(r, _zoomed, ScaleMode.ScaleToFit, false);
			}
		}		
	}

	private static void DrawTexture(Rect screenRect, Texture texture, ScaleMode scaleMode, Material material)
	{
		if (Event.current.type == EventType.Repaint)
		{
			float textureWidth = texture.width;
			float textureHeight = texture.height;

			float aspectRatio = (float)textureWidth / (float)textureHeight;
			Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
			switch (scaleMode)
			{
				case ScaleMode.ScaleAndCrop:
					{
						float screenRatio = screenRect.width / screenRect.height;
						if (screenRatio > aspectRatio)
						{
							float adjust = aspectRatio / screenRatio;
							sourceRect = new Rect(0f, (1f - adjust) * 0.5f, 1f, adjust);
						}
						else
						{
							float adjust = screenRatio / aspectRatio;
							sourceRect = new Rect(0.5f - adjust * 0.5f, 0f, adjust, 1f);
						}
					}
					break;
				case ScaleMode.ScaleToFit:
					{
						float screenRatio = screenRect.width / screenRect.height;
						if (screenRatio > aspectRatio)
						{
							float adjust = aspectRatio / screenRatio;
							screenRect = new Rect(screenRect.xMin + screenRect.width * (1f - adjust) * 0.5f, screenRect.yMin, adjust * screenRect.width, screenRect.height);
						}
						else
						{
							float adjust = screenRatio / aspectRatio;
							screenRect = new Rect(screenRect.xMin, screenRect.yMin + screenRect.height * (1f - adjust) * 0.5f, screenRect.width, adjust * screenRect.height);
						}
					}
					break;
				case ScaleMode.StretchToFill:
					break;
			}
			Graphics.DrawTexture(screenRect, texture, sourceRect, 0, 0, 0, 0, GUI.color, material);
		}
	}
}
