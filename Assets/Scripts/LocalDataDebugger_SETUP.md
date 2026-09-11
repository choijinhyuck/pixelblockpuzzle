# LocalDataDebugger 설정 가이드

## 개요
`LocalDataDebugger`는 로컬 테스트 중에 PlayerPrefs의 데이터(BestScore, 닉네임 등)를 쉽게 수정할 수 있는 디버그 도구입니다.

## 설정 방법

### 1. Canvas 준비
- 기존 Canvas가 없다면 Canvas를 생성합니다 (Right-click in Hierarchy → UI → Canvas)

### 2. Debug Panel 생성
Canvas 하위에 다음 구조로 UI를 생성합니다:

```
Canvas
├── DebugPanel (Panel)
│   ├── Title (TextMeshProUGUI) - "Local Data Debugger"
│   ├── BestScore Section
│   │   ├── BestScoreDisplay (TextMeshProUGUI)
│   │   ├── BestScoreInputField (InputField)
│   │   └── SetBestScoreButton (Button)
│   ├── Nickname Section
│   │   ├── NicknameDisplay (TextMeshProUGUI)
│   │   ├── NicknameInputField (InputField)
│   │   └── SetNicknameButton (Button)
│   ├── PlayerID Display (TextMeshProUGUI)
│   ├── Button Row
│   │   ├── RefreshDataButton (Button)
│   │   ├── SetTestDataButton (Button)
│   │   └── ResetAllButton (Button)
│   └── AdditionalInfoText (TextMeshProUGUI)
└── ToggleDebugButton (Button) - Canvas 밖에 배치 가능
```

### 3. LocalDataDebugger 스크립트 할당
1. Canvas에 `LocalDataDebugger` 스크립트를 Add Component합니다
2. Inspector에서 다음 항목들을 할당합니다:

| 항목 | 설명 |
|-----|------|
| Debug Panel | DebugPanel GameObject |
| Toggle Debug Button | 디버그 패널을 토글하는 버튼 |
| Best Score Input Field | BestScore 입력 필드 |
| Set Best Score Button | BestScore 설정 버튼 |
| Best Score Display Text | 현재 BestScore 표시 텍스트 |
| Nickname Input Field | 닉네임 입력 필드 |
| Set Nickname Button | 닉네임 설정 버튼 |
| Nickname Display Text | 현재 닉네임 표시 텍스트 |
| Player Id Display Text | Player ID 표시 텍스트 |
| Reset All Button | 모든 데이터 초기화 버튼 |
| Set Test Data Button | 테스트 데이터 설정 버튼 |
| Refresh Data Button | 데이터 새로고침 버튼 |
| Additional Info Text | 추가 정보 표시 텍스트 |

### 4. 버튼 클릭 이벤트 설정 (Optional)
각 버튼의 OnClick 이벤트를 자동으로 연결하고 싶다면, LocalDataDebugger의 InitializeUI() 메서드가 이미 처리합니다.

## 기능

### BestScore 변경
1. "Best Score Input Field"에 원하는 점수를 입력
2. "Set BestScore Button"을 클릭
3. 현재 BestScore가 업데이트됩니다

### 닉네임 변경
1. "Nickname Input Field"에 닉네임을 입력
2. "Set Nickname Button"을 클릭
3. 현재 닉네임이 업데이트됩니다

### 테스트 데이터 설정
"Set Test Data Button"을 클릭하면 다음 샘플 데이터가 설정됩니다:
- BestScore: 50000
- Nickname: TestPlayer

### 모든 데이터 초기화
"Reset All Button"을 클릭하면 모든 로컬 데이터가 삭제됩니다.

### 데이터 새로고침
"Refresh Data Button"을 클릭하면 화면의 표시 데이터가 업데이트됩니다.

## 빌드 설정

- **에디터 & DEBUG 빌드**: 디버그 패널 활성화
- **릴리스 빌드**: 디버그 패널 자동 비활성화 (예처리 지시문: `#if UNITY_EDITOR || DEBUG`)

릴리스 빌드에서 비활성화하려면 빌드할 때 DEBUG 플래그를 제거하면 됩니다.

## 프로그래밍 방식 사용

```csharp
LocalDataDebugger debugger = GetComponent<LocalDataDebugger>();

// 특정 PlayerPrefs 값 설정
debugger.SetPlayerPrefsInt("BestScore", 100000);
debugger.SetPlayerPrefsString("DebugNickname", "MyTestNickname");

// 특정 PlayerPrefs 값 조회
int score = debugger.GetPlayerPrefsInt("BestScore", 0);
string nickname = debugger.GetPlayerPrefsString("DebugNickname", "");

// 디버그 패널 토글
debugger.ToggleDebug();
```

## 팁

- 게임 중에 `Tab` 키를 눌러 디버그 패널을 토글하도록 별도 KeyCode 리스너를 추가할 수 있습니다
- 리더보드 검증을 위해 BestScore를 서버의 점수보다 높게 설정해서 동기화 테스트를 할 수 있습니다
- 닉네임을 다양하게 설정해서 필터링 및 검증 로직을 테스트할 수 있습니다
