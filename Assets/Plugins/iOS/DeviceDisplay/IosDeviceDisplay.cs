using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Baviux {

public class IosDeviceDisplay {
#if UNITY_IOS && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern int _iOSDeviceDisplayScaleFactor();

	public static int scaleFactor{
		get{
			return _iOSDeviceDisplayScaleFactor();
		}
	}
#endif
}

}