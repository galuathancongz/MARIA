using UnityEngine;

namespace Luzart
{
    public class Level3_SetTopic_LO_DC : MonoBehaviour
    {
        [SerializeField] private int topicIndex;

        public void OnClick()
        {
            var data = Level3Manager.Instance.Data;
            data.topicIndex = topicIndex;
            // subject đã set trước đó ở scene chọn subject
            // learningObjective + designConstraint tự derive từ DesignChallengeTable
        }
    }
}
