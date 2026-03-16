using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepActionClick : StepAction
{
    public enum Mode
    {
        Button = 0,
        Mouse = 1
    } 
    public Mode mode = Mode.Button;
    public void OnClickOnDone()
    {
        CallOnComplete();
    }
    private void Update()
    {
        if(mode == Mode.Mouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && IsPointerOverButton())
                {
                    Debug.Log("Đang bấm vào UI - Bỏ qua xử lý logic game");
                    return;
                }
                OnClickOnDone();
            }
        }   
    }
    private bool IsPointerOverButton()
    {
        // Tạo dữ liệu con trỏ tại vị trí chuột hiện tại
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Danh sách chứa các kết quả trả về từ Raycast
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Kiểm tra xem đối tượng hoặc cha của nó có chứa component Button không
            // Dùng GetComponentInParent vì Raycast thường dính vào phần Text hoặc Image con của Button
            if (result.gameObject.GetComponentInParent<Button>() != null)
            {
                return true;
            }
        }

        return false;
    }
}
