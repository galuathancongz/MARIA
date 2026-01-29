using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Luzart
{
    public class Level2Manager : SingletonSaveLoad<Level2Data, Level2Manager>
    {
        public static bool IsSendStartIdeationLab = false;
        protected override string KEYLOAD => "Level2_AI";
        public void Send(byte indexConversation, string str, Action<string> onResult)
        {
            Action<string> newOnResult = (result) =>
            {
                var converstationState = Data.AddConvesationData(indexConversation, new ConverstationData() { str = result, role = ERole.AI });
                converstationState.State = EState.CanWrite;
                onResult?.Invoke(result);
            };
            APIManager.Instance.Send(str, newOnResult);
            var state = Data.AddConvesationData(indexConversation, new ConverstationData() { str = str, role = ERole.Me });
            state.State = EState.WaitAI;
        }
        public List<ConverstationData> GetCurrentConverstation(byte index)
        {
            return Data.GetConverstationData(index);
        }
    }

    [System.Serializable]
    public class Level2Data
    {
        public ESubject subject;
        
        public string question2_3_1;
        public List<ConversationState> listConverstationState;
        public ConversationState AddConvesationData(byte indexConverstation, ConverstationData data)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if(converstationDataList == null)
            {
                converstationDataList = new ConversationState()
                {
                    indexConverstation = indexConverstation,
                    listConverstationData = new List<ConverstationData>(),
                };
                listConverstationState.Add(converstationDataList);
            }
            converstationDataList.listConverstationData.Add(data);
            return converstationDataList;
        }
        public List<ConverstationData> GetConverstationData(byte indexConverstation)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if (converstationDataList == null)
            {
                return new List<ConverstationData>();
            }
            return converstationDataList.listConverstationData;
        }
        public EState GetConverstationState(byte indexConverstation)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if (converstationDataList == null)
            {
                return EState.CanWrite;
            }
            return converstationDataList.State;
        }
    }
    [System.Serializable]
    public class ConversationState
    {
        public EState State = EState.CanWrite;
        public byte indexConverstation;
        public List<ConverstationData> listConverstationData;
    }
    [System.Serializable]
    public class ConverstationData
    {
        public string str;
        public ERole role;
    }
    public enum ERole : byte
    {
        Me = 0,
        AI = 1,
    }
    public enum  EState : byte
    {
        CanWrite = 0,
        WaitAI = 1,
    }

    public static class MentorSubjectExtension
    {
        public static string GetNameMentor(ESubject subject)
        {
            switch (subject)
            {
                case ESubject.English:
                    return "Austen";
                case ESubject.Math:
                    return "Euclidea";
                case ESubject.History:
                    return "Thucy";
                case ESubject.Science:
                    return "Drawina";
                default:
                    return "Mentor";
            }
        }
        public static string GetSubjectName(ESubject subject)
        {
            switch (subject)
            {
                case ESubject.English:
                    return "English";
                case ESubject.Math:
                    return "Math";
                case ESubject.History:
                    return "History";
                case ESubject.Science:
                    return "Science";
                default:
                    return "Subject";
            }
        }
    }
}

