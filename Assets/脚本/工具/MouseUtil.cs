using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MouseUtil 
{
    private static Camera camera = Camera.main;
    public static Vector3 GetMousePositionInWorldSpace(float zValue=0f)
    {
        Plane dragPlane=new(camera.transform.forward,new Vector3(0,0,zValue));//创建一个用于跟踪位置的平面
        Ray ray=camera.ScreenPointToRay(Input.mousePosition);
        if(dragPlane.Raycast(ray,out float distance))//如果射线击中屏幕上的一点，就返回该点
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
