using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这部分代码实现上位于Utils类模块之外
public enum BoundsTest
{
    center,     //游戏对象的中心是否位于屏幕中
    onScreen,   //游戏对象是否完全位于屏幕之中
    offScreen   //游戏对象是否完全位于屏幕之外
}

public class Utils : MonoBehaviour {
    //Bounds函数
    //接受两个Bounds类型变量，并返回包含这两个Bounds的新Bounds
    public static Bounds BoundsUnion(Bounds b0,Bounds b1)
    {
        //如果其中一个Bounds的size为0，则忽略它
        if (b0.size==Vector3.zero&&b1.size!=Vector3.zero)
        {
            return (b1);
        }
        else if (b0.size!=Vector3.zero&&b1.size==Vector3.zero)
        {
            return (b0);
        }
        else if (b0.size == Vector3.zero && b1.size == Vector3.zero)
        {
            return (b0);
        }
        //扩展b0,使其可以包含b1.min和b1.max
        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return (b0);
    }

    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        //创建一个空白Bounds变量b
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        //如果游戏对象具有渲染器组件
        if (go.GetComponent<Renderer>()!=null)
        {
            //扩展b使其包含渲染器的边界框
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        }
        //如果游戏对象具有碰撞器组件
        if (go.GetComponent<Collider>()!=null)
        {
            //扩展b使其包含碰撞器的边界框
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        }
        //递归遍历游戏对象Transform组件的每个子对象
        foreach (Transform t in go.transform)
        {
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }
        return (b);
    }
    
    //创建一个静态只读全局属性camBounds
    static public Bounds camBounds
    {
        get
        {
            //如果未设置 _camBounds 变量
            if (_camBounds.size==Vector3.zero)
            {
                //使用默认摄像机设置调用SetCameraBounds()
                SetCameraBounds();
            }
            return (_camBounds);
        }
    }
    //这是一个局部静态字段，在camBounds属性定义中使用
    static private Bounds _camBounds;

    //本函数用于camBounds属性，可设置_camBounds变量值，也可直接调用
    public static void SetCameraBounds(Camera cam = null)
    {
        //如果未传入任何摄像机作为参数，则使用主摄像机
        if (cam == null) cam = Camera.main;
        //这里对摄像机做一些重要假设：
        //1.摄像机为正投影摄像机
        //2.摄像机的旋转为R:[0,0,0]
        //根据屏幕左上角和右上角坐标创建两个三维向量
        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);
        //将两个坐标转化为世界坐标
        Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
        Vector3 boundBRF = cam.ScreenToWorldPoint(bottomRight);
        //将两个三维向量的z坐标值分别设置为摄像机远切平面和进切平面的z坐标
        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;
        //查找边界框的中心
        Vector3 center = (boundTLN + boundBRF) / 2f;
        _camBounds = new Bounds(center, Vector3.zero);
        //扩展_camBounds，使其具有尺寸
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }

    //检查边界框bnd是否位于镜头边界框camBounds之内
    public static Vector3 ScreenBoundsCheck(Bounds bnd,BoundsTest test = BoundsTest.center)
    {
        return (BoundsInBoundsCheck(camBounds, bnd, test));
    }

    //检查边界框lilB是否位于边界框bigB之内
    public static Vector3 BoundsInBoundsCheck(Bounds bigB,Bounds lilB,BoundsTest test = BoundsTest.onScreen)
    {
        //根据所选的BoundsTest，本函数的行为也会有所不同
        //获取边界框lilB的中心
        Vector3 pos = lilB.center;
        Vector3 off = Vector3.zero;
        switch (test)
        {
            //当test参数值为center时，函数将确定将lilB的中心平移到bigB之内，
            //需要平移的方向和距离，用三维向量off表示
            case BoundsTest.center:
                if (bigB.Contains(pos))
                {
                    return (Vector3.zero);
                }

                if (pos.x>bigB.max.x)
                {
                    off.x = pos.x - bigB.max.x;
                }
                else if (pos.x<bigB.min.x)
                {
                    off.x = pos.x - bigB.min.x;
                }

                if (pos.y > bigB.max.y)
                {
                    off.y = pos.y - bigB.max.y;
                }
                else if (pos.y < bigB.min.y)
                {
                    off.y = pos.y - bigB.min.y;
                }

                if (pos.z > bigB.max.z)
                {
                    off.z = pos.z - bigB.max.z;
                }
                else if (pos.z < bigB.min.z)
                {
                    off.z = pos.z - bigB.min.z;
                }
                return (off);
            //当test参数值为onScreen时，函数将确定将lilB整体平移到，
            //需要平移的方向和距离，用三维向量off表示
            case BoundsTest.onScreen:
                if (bigB.Contains(lilB.min)&&bigB.Contains(lilB.max))
                {
                    return (Vector3.zero);
                }

                if (lilB.max.x > bigB.max.x)
                {
                    off.x = lilB.max.x - bigB.max.x;
                }
                else if (lilB.min.x < bigB.min.x)
                {
                    off.x = lilB.min.x - bigB.min.x;
                }

                if (lilB.max.y > bigB.max.y)
                {
                    off.y = lilB.max.y - bigB.max.y;
                }
                else if (lilB.min.y < bigB.min.y)
                {
                    off.y = lilB.min.y - bigB.min.y;
                }

                if (lilB.max.z > bigB.max.z)
                {
                    off.z = lilB.max.z - bigB.max.z;
                }
                else if (lilB.min.z < bigB.min.z)
                {
                    off.z = lilB.min.z - bigB.min.z;
                }
                return (off);
            //当test参数值为offScreen时，函数将确定要将lilB的任意一部分平移到bigB之内
            //需要平移的方向和距离，用三维向量off表示    
            case BoundsTest.offScreen:
                bool cMin = bigB.Contains(lilB.min);
                bool cMax = bigB.Contains(lilB.max);
                if (cMin||cMax)
                {
                    return (Vector3.zero);
                }

                if (lilB.min.x>bigB.max.x)
                {
                    off.x = lilB.min.x - bigB.max.x;
                }
                else if (lilB.max.x<bigB.min.x)
                {
                    off.x = lilB.max.x - bigB.min.x;
                }

                if (lilB.min.y > bigB.max.y)
                {
                    off.y = lilB.min.y - bigB.max.y;
                }
                else if (lilB.max.y < bigB.min.y)
                {
                    off.y = lilB.max.y - bigB.min.y;
                }

                if (lilB.min.z > bigB.max.z)
                {
                    off.z = lilB.min.z - bigB.max.z;
                }
                else if (lilB.max.z < bigB.min.z)
                {
                    off.z = lilB.max.z - bigB.min.z;
                }
                return (off);
        }
        return (Vector3.zero);
    }

    /// <summary>
    /// 变换函数
    /// 本函数将采用递归方法查找transform.parent树
    /// 直至找到具有自定义标签的对象tag!="Untagged"或者没有父对象为止
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static GameObject FindTaggedParent(GameObject go)
    {
        //如果当前游戏对象具有自定义标签
        if (go.tag!="Untagged")
        {
            //当返回当前游戏对象
            return (go);
        }
        //如果当前的transform没有父对象
        if (go.transform.parent==null)
        {
            //我们到达了对象树的最顶层仍未找到所需的对象
            //所以返回null
            return (null);
        }
        //否则，继续使用递归方法沿对象树向上查找
        return (FindTaggedParent(go.transform.parent.gameObject));
    }

    //这个版本的FindTaggedParent()函数以Transform为参数
    public static GameObject FindTaggedParent(Transform t)
    {
        return (FindTaggedParent(t.gameObject));
    }

    #region 材质函数
    //用一个List返回游戏对象或其子对象的所有材质
    static public Material[] GetAllMaterials(GameObject go)
    {
        List<Material> mats = new List<Material>();
        if (go.GetComponent<Renderer>()!=null)
        {
            mats.Add(go.GetComponent<Renderer>().material);
        }
        foreach (Transform t in go.transform)
        {
            mats.AddRange(GetAllMaterials(t.gameObject));
        }
        return (mats.ToArray());
    }
    #endregion
}
