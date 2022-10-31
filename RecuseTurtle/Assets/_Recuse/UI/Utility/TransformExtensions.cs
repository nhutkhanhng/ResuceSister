using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Extensions
{
    public static class TransformExtensions
    {
        public static void LookAt2D(this Transform self, Transform target, Vector2 forward)
        {
            LookAt2D(self, target.position, forward);
        }
        public static void LookAt2D(this Transform self, Vector3 target, Vector2 forward)
        {
            var forwardDiff = GetForwardDiffPoint(forward);
            Vector3 direction = target - self.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            self.rotation = Quaternion.AngleAxis(angle - forwardDiff, Vector3.forward);
        }
        static private float GetForwardDiffPoint(Vector2 forward)
        {
            if (Equals(forward, Vector2.up)) return 90;
            if (Equals(forward, Vector2.right)) return 0;
            return 0;
        }
        public static float LookAtAngle2D(this Transform self, Vector3 target, Vector2 forward)
        {
            var forwardDiff = GetForwardDiffPoint(forward);
            Vector3 direction = target - self.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle - forwardDiff, Vector3.forward);
            return q.eulerAngles.z;
        }
        public static Transform GetClosestEnemy(this Transform self, Transform[] enemies)
        {
            Transform tMin = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = self.position;
            foreach (Transform t in enemies)
            {
                float dist = Vector3.Distance(t.position, currentPos);
                if (dist < minDist)
                {
                    tMin = t;
                    minDist = dist;
                }
            }
            return tMin;
        }
        public static GameObject[] GetObjectListInRange(this Transform self, Transform[] objects, float range)
        {
            List<GameObject> values = new List<GameObject>();
            foreach (Transform t in objects)
            {
                float dist = Vector2.Distance(self.position, t.position);
                if (dist <= range)
                    values.Add(t.gameObject);
            }
            return values.ToArray();
        }
        public static Vector2 MousePositionToCanvasPosition(RectTransform canvas, Vector2 mousePosition)
        {
            var temp = Camera.main.ScreenToViewportPoint(mousePosition);
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;
            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        }
        public static Vector2 CanvasToCanvasPosition(Vector2 position, Camera camera, RectTransform targetCanvas)
        {
            var temp = camera.WorldToViewportPoint(position);
            temp.x *= targetCanvas.sizeDelta.x;
            temp.y *= targetCanvas.sizeDelta.y;
            temp.x -= targetCanvas.sizeDelta.x * targetCanvas.pivot.x;
            temp.y -= targetCanvas.sizeDelta.y * targetCanvas.pivot.y;

            return temp;

        }
        public static Vector2 PositionToViewportPoint(Vector3 position, Camera camera)
        {
            return camera.WorldToViewportPoint(position);

        }
        public static Vector2 ViewportPointToPosition(Vector2 viewportPoint, RectTransform canvas)
        {
            var temp = viewportPoint;
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;
            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        }
        public static Vector3 WorldToCameraPos(Vector3 pos, Camera cam)
        {
            var temp = Camera.main.WorldToViewportPoint(pos);
            temp = cam.ViewportToWorldPoint(temp);
            return temp;
        }
        public static Vector2 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 temp = camera.WorldToViewportPoint(position);
#if UNITY_EDITOR
            Debug.LogWarning(temp);
#endif
            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;
            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.
            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;
            return temp;
        }
        public static Transform[] GetChilds(this Transform self, string name)
        {
            List<Transform> childs = new List<Transform>();
            foreach (Transform t in self.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, name))
                {
                    childs.Add(t);
                }
            }
            return childs.ToArray();
        }

        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}