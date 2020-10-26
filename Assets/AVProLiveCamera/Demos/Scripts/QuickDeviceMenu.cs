using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2012-2016 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.AVPro.LiveCamera.Demos
{
	public class QuickDeviceMenu : MonoBehaviour
	{
		public AVProLiveCamera _liveCamera;
		public AVProLiveCameraManager _liveCameraManager;
		public GUISkin _guiSkin;

		void OnGUI()
		{
			GUI.skin = _guiSkin;

			if (_liveCameraManager.NumDevices > 0)
			{
				GUILayout.BeginHorizontal();

				// Select device
				GUILayout.BeginVertical();
				GUILayout.Button("SELECT DEVICE");
				for (int i = 0; i < _liveCameraManager.NumDevices; i++)
				{
					GUI.color = Color.white;
					if (_liveCamera._desiredDeviceIndex == i && _liveCamera.Device != null && _liveCamera.Device.IsRunning)
					{
						GUI.color = Color.green;
					}

					string name = _liveCameraManager.GetDevice(i).Name;
					if (GUILayout.Button(name))
					{
						_liveCamera._deviceSelection = AVProLiveCamera.SelectDeviceBy.Index;
						_liveCamera._desiredDeviceIndex = i;
						_liveCamera.Begin();
					}
				}
				GUI.color = Color.white;
				GUILayout.EndVertical();

				if (_liveCamera.Device != null && _liveCamera.Device.IsRunning)
				{
					//Select resolution
					GUILayout.BeginVertical();
					GUILayout.Button("RESOLUTION");
					List<string> usedNames = new List<string>(32);
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{

						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);
						string name = string.Format("{0}x{1}", mode.Width, mode.Height);
						if (!usedNames.Contains(name))
						{
							GUI.color = Color.white;
							if (_liveCamera.Device.CurrentWidth == mode.Width && _liveCamera.Device.CurrentHeight == mode.Height)
							{
								GUI.color = Color.green;
							}

							usedNames.Add(name);
							if (GUILayout.Button(name))
							{
								_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
								_liveCamera._desiredModeIndex = i;
								_liveCamera.Begin();
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndVertical();

					// Select frame rate
					usedNames.Clear();
					GUILayout.BeginVertical();
					GUILayout.Button("FPS");
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{
						string matchName = string.Format("{0}x{1}", _liveCamera.Device.CurrentWidth, _liveCamera.Device.CurrentHeight);

						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);

						string resName = string.Format("{0}x{1}", mode.Width, mode.Height);
						if (resName == matchName)
						{
							string name = string.Format("{0}", mode.FPS.ToString("F2"));
							if (!usedNames.Contains(name))
							{
								GUI.color = Color.white;
								if (_liveCamera.Device.CurrentFrameRate.ToString("F2") == mode.FPS.ToString("F2"))
								{
									GUI.color = Color.green;
								}

								usedNames.Add(name);
								if (GUILayout.Button(name))
								{
									_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
									_liveCamera._desiredModeIndex = i;
									_liveCamera.Begin();
								}
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndVertical();

					// Select format
					usedNames.Clear();
					GUILayout.BeginVertical();
					GUILayout.Button("FORMAT");
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{
						string matchName = string.Format("{0}x{1}@", _liveCamera.Device.CurrentWidth, _liveCamera.Device.CurrentHeight, _liveCamera.Device.CurrentFrameRate.ToString("F2"));

						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);

						string resName = string.Format("{0}x{1}@", mode.Width, mode.Height, mode.FPS.ToString("F2"));
						if (resName == matchName)
						{
							string name = string.Format("{0}", mode.Format);
							if (!usedNames.Contains(name))
							{
								GUI.color = Color.white;
								if (_liveCamera.Device.CurrentDeviceFormat == mode.Format)
								{
									GUI.color = Color.green;
								}

								usedNames.Add(name);
								if (GUILayout.Button(name))
								{
									_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
									_liveCamera._desiredModeIndex = i;
									_liveCamera.Begin();
								}
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndVertical();
				}
			}
			else
			{
				GUILayout.Label("No webcam / capture devices found");
			}
		}
	}
}
