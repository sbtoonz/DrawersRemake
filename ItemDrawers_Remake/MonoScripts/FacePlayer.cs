using System;
using System.Collections;
using ItemDrawers_Remake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FacePlayer : MonoBehaviour
{
  public RectTransform objectToRotate;
  public Image image;
  public TextMeshProUGUI text;
  public SphereCollider SphereCollider;
  public Color m_DisabledColor;
  public Color m_EnableColor;
  public Piece? _Piece;

  private static bool enableRan = false;
  private void OnEnable()
  {
    if(enableRan)return;
    if (SphereCollider == null)
      SphereCollider = gameObject.GetComponent<SphereCollider>();
    if (text == null)
      text = gameObject.transform.parent.transform.Find("Canvas/Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();
    text.CrossFadeAlpha(0f, 1.5f, false);
    enableRan = true;
  }
  
  private IEnumerator RotateToPlayer(float time)
  {
    while (true)
    {
      if(time == 0) break;
      if (GameCamera.instance != null)
      {
        yield return new WaitForSeconds(time);
        Quaternion rotation = GameCamera.instance.m_camera.transform.rotation;
        objectToRotate.transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        rotation = new Quaternion();
      }
      else
        break;
    }
  }
  
  private IEnumerator LerpColorEnabled()
  {
    float progress = 0.0f;
    float increment = 0.0133333327f;
    while ((double) progress < 1.0)
    { 
      image.color = Color.Lerp(ItemDrawersMod._disabledColorOpacity.Value, ItemDrawersMod._enabledColorOpacity.Value, progress);
      progress += increment;
      yield return (object) new WaitForSeconds(0.02f);
      if ((double) Math.Abs(progress - 1f) < 1.0 / 1000.0)
        break;
    }
  }

  private IEnumerator LerpColorDisabled()
  {
    float progress = 0.0f;
    float increment = 0.0133333327f;
    while ((double) progress < 1.0)
    {
      image.color = Color.Lerp(ItemDrawersMod._enabledColorOpacity.Value, ItemDrawersMod._disabledColorOpacity.Value, progress);
      progress += increment;
      yield return (object) new WaitForSeconds(0.02f);
      if ((double) Math.Abs(progress - 1f) < 1.0 / 1000.0)
        break;
    }
  }
  private void OnTriggerEnter(Collider other)
  {
    if (!(other.gameObject.GetComponent<Player>() != null))
      return;
    text.CrossFadeAlpha(1, 1.5f, false);
    StartCoroutine(LerpColorEnabled());
    if(ItemDrawersMod._rotateAtPlayer.Value == false)return;
    StartCoroutine(RotateToPlayer(0.25f));
  }

  private void OnTriggerExit(Collider other)
  {
    if (!(other.gameObject.GetComponent<Player>() != null))
      return;
    text.CrossFadeAlpha(0, 1.5f, false);
    StartCoroutine(LerpColorDisabled());
    if(ItemDrawersMod._rotateAtPlayer.Value == false)return;
    StopCoroutine(RotateToPlayer(0f));
  }
}
