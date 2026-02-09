using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Luzart
{
	public class ApplicationVersion : MonoBehaviour
	{
		public TMP_Text txtVersion;
		void Start()
		{
			if(txtVersion == null)
			{
				txtVersion = GetComponent<TMP_Text>();
            }
            if (txtVersion != null)
                txtVersion.text = "Version " + Application.version;
        }
	}
}
