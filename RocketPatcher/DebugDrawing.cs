using UnityEngine;

namespace RocketPatcher
{
    internal static class DebugDrawing
    {
        public static void DrawPoint(Vector3 position, Color color, float duration = 10f)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.transform.position = position;
            pointObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            pointObj.GetComponent<Renderer>().material.color = color;
            Object.Destroy(pointObj.GetComponent<Collider>());

            Object.Destroy(pointObj, duration);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 1.0f)
        {
            GameObject lineObj = new("DebugLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.startColor = color;
            lr.endColor = color;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            Object.Destroy(lineObj, duration);
        }

        public static void DrawCylinder(Vector3 position, float height, float radius, Color color, float duration = 10f)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.position = position;
            cylinder.transform.localScale = new Vector3(radius * 2, height / 2, radius * 2);
            Object.Destroy(cylinder.GetComponent<Collider>());
            cylinder.GetComponent<Renderer>().material.color = color;

            Object.Destroy(cylinder, duration);
        }

        public static void DrawCylinder(Vector3 start, Vector3 end, float radius, Color color, float duration = 10f)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            Vector3 direction = (end - start).normalized;
            cylinder.transform.position = (start + end) / 2;
            cylinder.transform.localScale = new Vector3(radius * 2, (end - start).magnitude / 2, radius * 2);
            cylinder.transform.up = direction;
            Object.Destroy(cylinder.GetComponent<Collider>());
            cylinder.GetComponent<Renderer>().material.color = color;

            Object.Destroy(cylinder, duration);
        }

        public static void DrawCapsule(Vector3 position, float height, float radius, Color color, float duration = 10f)
        {
            DrawCylinder(position, height - radius * 2, radius, color, duration);
            GameObject topSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topSphere.transform.position = position + new Vector3(0, height / 2 - radius, 0);
            topSphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            topSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(topSphere.GetComponent<Collider>());
            topSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(topSphere, duration);

            GameObject bottomSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bottomSphere.transform.position = position - new Vector3(0, height / 2 - radius, 0);
            bottomSphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            bottomSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(bottomSphere.GetComponent<Collider>());
            bottomSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(bottomSphere, duration);
        }

        public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color, float duration = 10f)
        {
            DrawCylinder(start, end, radius, color, duration);
            GameObject startSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            startSphere.transform.position = start;
            startSphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            startSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(startSphere.GetComponent<Collider>());
            startSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(startSphere, duration);

            GameObject endSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            endSphere.transform.position = end;
            endSphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            endSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(endSphere.GetComponent<Collider>());
            endSphere.GetComponent<Renderer>().material.color = color;
            Object.Destroy(endSphere, duration);
        }
    }
}