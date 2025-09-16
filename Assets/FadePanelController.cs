using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadePanelController : MonoBehaviour
{
    public float fadeDuration = 1.5f;   // 페이드 아웃에 걸리는 시간 (초)

    void Start()
    {
        StartCoroutine(FadeOutPanel());
    }

    IEnumerator FadeOutPanel()
    {
        Image panelImage = GetComponent<Image>();

        if (panelImage == null)
        {
            Debug.LogError("PanelFader: 이 GameObject에 Image 컴포넌트가 없습니다. Panel에 스크립트를 부착했는지 확인해주세요.");
            yield break; // 코루틴 종료
        }

        // 패널의 현재 색상을 가져옵니다.
        Color currentColor = panelImage.color;
        // 시작 투명도 (완전히 불투명)
        float startAlpha = 1f;
        // 목표 투명도 (완전히 투명)
        float targetAlpha = 0f;
        // 경과 시간
        float elapsedTime = 0f;

        // 페이드 아웃 시작 시 패널을 활성화합니다. (혹시 비활성화되어 있을 경우)
        panelImage.gameObject.SetActive(true);
        // 패널의 투명도를 시작 값으로 설정합니다.
        currentColor.a = startAlpha;
        panelImage.color = currentColor;

        // 지정된 시간 동안 반복하여 패널의 투명도를 조절합니다.
        while (elapsedTime < fadeDuration)
        {
            // 시간의 흐름에 따라 투명도(알파 값)를 1에서 0으로 부드럽게 변경합니다.
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

            // 패널의 현재 색상에서 투명도만 변경하여 적용합니다.
            currentColor.a = currentAlpha;
            panelImage.color = currentColor;

            elapsedTime += Time.deltaTime; // 다음 프레임으로 진행
            yield return null; // 다음 프레임까지 대기
        }

        // 페이드 아웃이 완료된 후, 패널의 최종 투명도를 완전히 투명하게 설정합니다.
        currentColor.a = targetAlpha;
        panelImage.color = currentColor;

        // 페이드 아웃이 완전히 끝나면 패널 GameObject를 비활성화하여 화면에서 완전히 사라지게 합니다.
        // (선택 사항: 완전히 사라진 후에는 더 이상 필요 없으므로)
        panelImage.gameObject.SetActive(false);
    }
}
