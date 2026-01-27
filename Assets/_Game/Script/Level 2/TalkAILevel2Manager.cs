using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Luzart
{
    public class TalkAILevel2Manager : SingletonSaveLoad<TalkAILevel2Data, TalkAILevel2Manager>
    {
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
    public class TalkAILevel2Data
    {
        public ESubject subject;
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
}

