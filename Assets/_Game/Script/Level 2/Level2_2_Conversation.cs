using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level2_Conversation_MeAndAI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private Level2_ConversationItem itemPrefabMe;
    [SerializeField] private Level2_ConversationItem itemPrefabAI;
    private List<Level2_ConversationItem> listItems = new List<Level2_ConversationItem>();
    private Level2_ConversationItem _itemThinkingAIFirst = null;
    private void OnEnable()
    {
        var allList = Level2Manager.Instance.GetCurrentConverstation(1);
        foreach (var item in allList)
        {
            Level2_ConversationItem newItem = SpawnItem(item);
            listItems.Add(newItem);
        }
        if (_itemThinkingAIFirst == null)
        {
            _itemThinkingAIFirst = SpawnItem(new ConverstationData() { str = "...", role = ERole.AI });
            _itemThinkingAIFirst.SetThinking();
            listItems.Add(_itemThinkingAIFirst);
        }
        Observer.Instance.AddObserver(ObserverKey.OnUpdateNewChat, OnObserverAIResponse);
        scrollRect.ScrollTo(0f, 1f);
    }

    private void OnObserverAIResponse(object data)
    {
        string response = data as string;
        _itemThinkingAIFirst.ShowTextAnim(response);
        scrollRect.ScrollTo(0f, 1f);
    }

    private void OnDisable()
    {
        foreach (var item in listItems)
        {
            Destroy(item.gameObject);
        }
        listItems.Clear();
        Observer.Instance.RemoveObserver(ObserverKey.OnUpdateNewChat, OnObserverAIResponse);
    }
    public void OnSendQuestion(string question)
    {
        var itemMe = SpawnItem(new ConverstationData() { str = question, role = ERole.Me });
        itemMe.ShowTextAnim(question);
        listItems.Add(itemMe);
        var itemAI = SpawnItem(new ConverstationData() { str = "...", role = ERole.AI });
        itemAI.SetThinking();
        listItems.Add(itemAI);

        question = LocalizationManager.Instance.GetPrompt("prompts.level2_2_conversation", new System.Collections.Generic.Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"subjectName", MentorSubjectExtension.GetSubjectName(Level2Manager.Instance.Data.subject)},
                {"question", question}
            });
        Level2Manager.Instance.Send(0, question, (response) =>
        {
            itemAI.ShowTextAnim(response);
        });
    }

    private Level2_ConversationItem SpawnItem(ConverstationData data)
    {
        Level2_ConversationItem newItem = null;
        if (data.role == ERole.Me)
        {
            newItem = Instantiate(itemPrefabMe, contentTransform);
        }
        else
        {
            newItem = Instantiate(itemPrefabAI, contentTransform);
        }
        newItem.ShowText(data.str);
        return newItem;
    }
}
