using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class MainStringPromptComponent : MonoBehaviour
    {
        public void SetStringPrompt(StepKey stepKey, string prompt)
        {
            MainStringPromptManager.Instance.Data.SetStringPrompt(stepKey, prompt);
        }
    }
}
