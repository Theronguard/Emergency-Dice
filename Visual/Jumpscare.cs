using LethalLib.Modules;
using System.Collections;
using UnityEngine;

namespace MysteryDice.Visual
{
    public class Jumpscare : MonoBehaviour
    {
        bool IsScaring = false;
        GameObject ImageGObj = null;
        Vector2 BaseSize = new Vector2(4f, 4f);
        void Start()
        {
            ImageGObj = transform.GetChild(0).gameObject;
            ImageGObj.SetActive(false);
        }

        public void Scare()
        {
            StartCoroutine(ScareTime());
        }
        void Update()
        {
            if (!IsScaring) return;

            ImageGObj.transform.localScale = new Vector3(BaseSize.x + UnityEngine.Random.Range(0f, 0.2f), BaseSize.y + UnityEngine.Random.Range(0f, 0.2f),1f);
        }

        IEnumerator ScareTime()
        {
            AudioSource.PlayClipAtPoint(MysteryDice.JumpscareSFX, GameNetworkManager.Instance.localPlayerController.transform.position,10f);
            AudioSource.PlayClipAtPoint(MysteryDice.JumpscareSFX, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(MysteryDice.JumpscareSFX, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(MysteryDice.JumpscareSFX, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            IsScaring = true;
            ImageGObj.SetActive(true);
            yield return new WaitForSeconds(1.3f);
            IsScaring = false;
            ImageGObj.SetActive(false);
        }
    }
}
