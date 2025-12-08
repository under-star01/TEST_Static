using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SettingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject sidebarPanel; // 슬라이드 패널(전체부모)
    [SerializeField] private Button settingButton; // 설정 버튼
    [SerializeField] private Button closeButton; // 닫기 버튼

    [Header("Volume Controls")]
    [SerializeField] private Slider volumeSlider; // 볼륨조절 슬라이더
    [SerializeField] private Slider sfxSlider; // SFX조절 슬라이더
    [SerializeField] private TextMeshProUGUI volumeValueText; // 값 표시
    [SerializeField] private TextMeshProUGUI sfxValueText;    // 값 표시

    private const string VOLUME_KEY = "MasterVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (settingButton != null)
        {
            // 설정 버튼을 클릭하면 - 설정열기
            settingButton.onClick.AddListener(OpenSettings);
        }

        if (closeButton != null)
        {
            // 닫기 버튼을 클릭하면 - 설정닫기
            closeButton.onClick.AddListener(CloseSettings);
        }

        // 슬라이더 이벤트 등록
        if (volumeSlider != null)
        {
            // 볼륨슬라이더 값이 바뀌면(움직이면) 볼륨 바꿈
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (sfxSlider != null)
        {
            // SFX슬라이더 값이 바뀌면(움직이면) 볼륨 바꿈
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        InitializeSettings(); // 초기세팅으로 돌림
    }

    private void InitializeSettings() // 초기설정
    {
        // 사이드바 초기 비활성화
        if (sidebarPanel != null)
        {
            // 사이드바 배경 끄기
            sidebarPanel.SetActive(false);
        }

        // 저장된 값 로드
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f); 
        float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume; // 슬라이더의 값 = 볼륨조절하도록
            UpdateVolumeText(savedVolume); // 적용위해 담기
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX; // 슬라이더 값 = SFX값
            UpdateSFXText(savedSFX); // 적용위해 담기
        }

        // 오디오 적용
        ApplyVolume(savedVolume);
        ApplySFX(savedSFX);
    }

    private void OpenSettings() // 설정창 켜기
    {
        if (sidebarPanel != null)
        {
            // 슬라이드패널 킴
            sidebarPanel.SetActive(true);
        }
        // 나 켜졌슈!
        Debug.Log("Settings opened");
    }

    private void CloseSettings() // 설정창 끄기
    {
        if (sidebarPanel != null)
        {
            // 슬라이드패널 끔
            sidebarPanel.SetActive(false);
        }
        // 나 꺼졌슈!
        Debug.Log("Settings closed");
    }

    private void OnVolumeChanged(float value) // 볼륨 변경됨
    {
        // 볼륨 반영 - 값
        ApplyVolume(value);
        // 볼륨 텍스트 반영 - 값
        UpdateVolumeText(value);
        // 볼륨 값을 출력해줌
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        // 변경값 저장함
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(float value)
    {
        // 위 과정 반복
        ApplySFX(value);
        UpdateSFXText(value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        // 전체 오디오 볼륨 조절
        AudioListener.volume = value;
    }

    private void ApplySFX(float value)
    {
        // SFX 볼륨 적용 (AudioManager 만들면 거기서 처리)
        // 예: AudioManager.Instance.SetSFXVolume(value);
        // 특정 AudioSource들의 볼륨 조절
        // 여기는 임시로 로그만 출력
        Debug.Log($"SFX Volume set to: {value}");
    }

    private void UpdateVolumeText(float value) // 메인볼륨 변경값 텍스트 출력
    {
        if (volumeValueText != null)
        {
            // 볼륨 값 %로 표기
            volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void UpdateSFXText(float value) // SFX 변경값 텍스트 출력
    {
        if (sfxValueText != null)
        {
            // 볼륨 값 %로 표기
            sfxValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void OnDestroy()
    {
        // 아까 받은 이벤트 해제
        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OpenSettings);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        }
    }
}
