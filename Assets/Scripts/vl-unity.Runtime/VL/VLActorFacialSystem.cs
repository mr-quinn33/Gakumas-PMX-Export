using System;
using VL.FaceSystem;

namespace VL
{
	public class VLActorFacialSystem : VLActorExtension
	{
		private VLActorFaceModel _faceModel; // 0x68
		public float blinkTimeScale; // 0x70
		public bool useBlink; // 0x74
		public bool autoBlinkDisabled; // 0x75
		private bool _currentBlink; // 0x7c
		private float _currentBlinkTime; // 0x80
		private float _nextBlinkTime; // 0x84
		private float _blinkWeight; // 0x88
		private bool _isUpdateBlink; // 0x8c
	}
}