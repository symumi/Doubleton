using BalartroLike.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITest : MonoBehaviour
{
    // 挂任意物体，进入 Play 后按 H 弹提示、按 C 弹确认框
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) UIRoot.Instance.Toast("灵力不足");
        if (Input.GetKeyDown(KeyCode.C)) UIRoot.Instance.Confirm("放弃本局", "当前进度将丢失，确定放弃？", () => Debug.Log("确认"));
    }
}
