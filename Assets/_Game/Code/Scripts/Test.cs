using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VinhLB.Utilities;

public class Test : MonoBehaviour
{
    public RectTransform targetTF;
    public Transform gameUITF;
    
    public float pickUpOffset = 1f;
    public float uiOffset = -100f;
    public Vector3 rotation;
    public float scale = 0.5f;

    private void Start()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return null;
        
        transform.SetParent(targetTF, true);
        gameObject.layer = VLBLayer.UI;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + pickUpOffset, 0.5f)
            .SetEase(Ease.OutSine));
        Vector3 targetPosition = new Vector3(0, 0, uiOffset);
        sequence.Append(transform.DOLocalMove(targetPosition, 1f)
            .SetEase(Ease.OutSine));
        Vector3 targetRotation = rotation;
        sequence.Join(transform.DOLocalRotate(targetRotation, 1f)
            .SetEase(Ease.OutSine));
        Vector3 targetScale = transform.localScale * scale;
        sequence.Join(transform.DOScale(targetScale, 1f)
            .SetEase(Ease.OutSine));
    }
}