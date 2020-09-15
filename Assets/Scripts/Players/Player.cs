using UnityEngine;

namespace Players
{
    public class Player : MonoBehaviour
    {
        public GameObject head;
        public string Nickname { get; set; }
        public uint ID { get; set; }
        public string Team { get; set; }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetPositionRotation(Vector3 position, Quaternion rotation, Quaternion headRotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            head.transform.rotation = headRotation;
        }
    }
}