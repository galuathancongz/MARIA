namespace Luzart
{
    using UnityEngine;

    /// <summary>
    /// Continuously rotates a RectTransform – used as the login loading spinner.
    /// </summary>
    public class UISimpleSpin : MonoBehaviour
    {
        [Tooltip("Degrees per second (clockwise = negative)")]
        public float speed = -280f;

        private void Update()
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }
}
