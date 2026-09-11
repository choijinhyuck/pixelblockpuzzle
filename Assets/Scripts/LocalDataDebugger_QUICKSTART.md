# LocalDataDebugger 빠른 시작

## 1분 안에 설정하기

### 시나리오 1: UI 없이 빠르게 테스트
```csharp
// GameManager 또는 임의의 MonoBehaviour에서
void Start()
{
    LocalDataDebugger debugger = GetComponent<LocalDataDebugger>();
    
    // 테스트 데이터 한번에 설정
    debugger.SetPlayerPrefsInt("BestScore", 100000);
    debugger.SetPlayerPrefsString("DebugNickname", "TestPlayer123");
}
```

### 시나리오 2: 풀 UI 설정 (권장)
1. Canvas 생성 (없으면)
2. LocalDataDebugger 스크립트를 Canvas에 추가
3. LocalDataDebugger_SETUP.md의 UI 구조 참고해서 UI 생성
4. 인스펙터에서 각 필드 할당

## 주요 기능

| 기능 | 방법 |
|-----|-----|
| 디버그 패널 열기/닫기 | **F12 키** 또는 UI 버튼 클릭 |
| BestScore 변경 | 입력 필드에 값 입력 → 버튼 클릭 |
| 닉네임 변경 | 입력 필드에 닉네임 입력 → 버튼 클릭 |
| 테스트 데이터 설정 | "Set Test Data" 버튼 (BestScore: 50000) |
| 모든 데이터 초기화 | "Reset All" 버튼 |
| 현재 플레이어 ID 확인 | 패널에 자동 표시 |

## 테스트 시나리오 예제

### 1. 로컬 BEST가 서버보다 높은 경우 테스트
```
1. LocalDataDebugger 패널 열기 (F12)
2. BestScore Input Field에 "500000" 입력
3. "Set BestScore" 버튼 클릭
4. 게임 재시작 또는 SyncLocalBestWithServer() 호출
5. 서버에 점수가 업데이트되는지 확인
```

### 2. 닉네임 필터링 테스트
```
1. Nickname Input Field에 테스트할 닉네임 입력
2. "Set Nickname" 버튼 클릭
3. 게임 내에서 닉네임 유효성 검사 동작 확인
```

### 3. 초기화 및 새 게임 시작
```
1. "Reset All" 버튼 클릭하여 모든 로컬 데이터 초기화
2. 새 게임 시작 (BestScore 0부터 시작)
3. 로컬 BEST 동기화 테스트
```

## 키보드 단축키

- **F12**: 디버그 패널 토글 (기본값, Inspector에서 변경 가능)

## 빌드 시 자동으로 비활성화됨

- 에디터에서는 항상 활성화
- DEBUG 플래그가 있는 빌드에서는 활성화
- 릴리스 빌드에서는 자동으로 비활성화

## 더 자세한 정보
[LocalDataDebugger_SETUP.md](LocalDataDebugger_SETUP.md) 참고
