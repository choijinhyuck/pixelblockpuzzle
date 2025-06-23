# Pixel Block Puzzle 게임 기획서

## 1. 게임 개요 및 장르 배경
- **장르**: 퍼즐 (블록 퍼즐, 테트리스류)
- **플랫폼**: 모바일(Android)
- **출시 플랫폼**: [원스토어](https://m.onestore.co.kr/ko-kr/apps/appsDetail.omp?prodId=0000781428)
- **엔진/툴**: Unity, Visual Studio Code, Git, Aseprite, Bosca Ceoil
- **개발 인원**: 1인 개발
- **타겟 유저**: 캐주얼 퍼즐 게임을 선호하는 전 연령층
- **플레이 영상**: [Youtube 영상](https://www.youtube.com/watch?v=osO16h3m1jg)

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
<img src="Play_Images\Main.png" width="200" alt="Main"/>
<br/>
<sub>[게임 플레이 화면]</sub>
</div>

### 1.1. 장르 배경
블록 퍼즐 게임은 1980년대 테트리스의 성공 이후 전 세계적으로 사랑받는 장르임. 직관적인 규칙과 반복 플레이의 재미, 점수 경쟁 요소로 인해 남녀노소 누구나 쉽게 접근할 수 있음. 최근에는 모바일 환경에 최적화된 조작과 짧은 플레이 타임, 다양한 스킨/미션 등으로 진화하고 있음.

### 1.2. 차별화 기획 의도 및 핵심 특징

- **블록 형태의 다양성**: 기존 블록 퍼즐 게임과 달리, 본 게임은 비슷한 장르의 게임에서 보기 힘든 독특한 블록 형태(총 54종)를 적극적으로 도입하여 반복적인 플레이에서도 새로운 전략과 재미를 제공함.
- **픽셀 블록의 감성적 디자인**: 각 블록의 단위는 귀여운 얼굴 표정을 가진 픽셀아트로 디자인함. 단순한 도형이 아닌 감정이 담긴 캐릭터처럼 느껴지도록 하여 반복 플레이의 지루함을 줄이고 친근함을 더함.
- **블록 제거 이펙트의 쾌감 강화**: 블록이 줄을 완성해 터질 때, 각 픽셀 블록이 실제로 부풀어 오르며 터지는 듯한 애니메이션 이펙트를 적용하여 시각적 쾌감과 타격감을 극대화
- **차별화된 시각/감성 경험**: 픽셀 표정, 색상, 이펙트 등에서 기존 블록 퍼즐과 차별화된 감성적 경험을 제공하는 것이 본 게임의 핵심 목표

## 2. 주요 시스템 및 구성 요소

### 2.1. 게임 플레이

- **목표**: 다양한 모양의 블록을 8x8 그리드에 배치하여 가로/세로 줄을 완성하면 해당 줄이 사라짐. 최대한 많은 점수를 획득하는 것이 목표
- **조작 방식**: 무작위로 주어진 3개의 블록 중 하나를 선택해 드래그 앤 드롭으로 그리드에 이동시켜 배치
- **게임 오버**: 블록을 더 이상 배치할 공간이 없을 때 게임 종료

### 2.1.1. 게임 오버 판정 로직

#### 게임 오버 판정
하단에 제시된 블록 중 단 하나라도 그리드 내에 배치 가능한 위치가 있으면 게임은 계속 진행됨. 반대로, 제시된 블록 모두에서 배치할 수 있는 위치가 단 하나도 없다면 즉시 게임 오버됨. 이 판정은 매 블록을 배치할 때마다 반복적으로 수행된다.

#### 배치 가능성 판정 과정
각 블록에 대해 그리드의 모든 위치를 검사하여, 해당 블록이 들어갈 수 있는 위치가 있는지 확인함. 각 블록은 자신의 크기(폭, 높이)만큼만 검사하며, 배치가 불가능한 조건(그리드 범위 초과, 이미 채워진 칸 등)을 발견하면 즉시 검사를 중단(return false)하도록 함.

```csharp
// 블록 배치 가능 여부 확인 예시 (실제 구현 코드)
public bool CanPlaceBlock(int x, int y, BlockShape shape)
{
    if (shape == null) return false;
    if (x < 0 || y < 0 || x + shape.width > gridWidth || y + shape.height > gridHeight)
        return false;

    for (int i = 0; i < shape.width; i++)
    {
        for (int j = 0; j < shape.height; j++)
        {
            if (shape.shape[j, i] == 1)
            {
                if (!IsIndexValid(x + i, y + j) || grid[x + i, y + j] != null)
                    return false;
            }
        }
    }
    return true;
}
```

#### 설계 의의 및 특징
불필요한 반복을 최소화하고, 배치 불가 조건을 빠르게 감지하여 즉시 중단하는 구조로 실시간 판정에 적합함. 매 블록 배치 시마다 이 과정을 반복하여, 플레이어가 즉각적으로 배치 가능/불가 및 게임 오버 여부를 인지할 수 있음. 모바일 환경에서도 쾌적한 조작감과 빠른 피드백을 제공함.

### 2.1.2. 콤보 및 점수 시스템

#### 콤보 시스템

- **콤보 시작**: 라인 제거를 연속 2번 성공하면 1콤보가 시작
- **콤보 유지**: 콤보가 시작된 이후, 4번의 블록 배치 시도 내에 또 한 번 라인 제거에 성공하면 콤보가 1씩 증가
    - 콤보가 쌓인 상태에서, 4번의 블록 배치 내에 라인 제거에 실패하면 콤보는 0으로 리셋

#### 점수 계산 공식

- **블록 배치 점수**: 블록을 그리드에 배치할 때, 해당 블록을 구성하는 유닛(칸)의 개수만큼 점수를 획득
    - 예시: 4칸짜리 블록을 배치하면 +4점
- **라인 제거 점수**: 한 번에 완성되어 사라지는 줄(가로/세로)마다 10점씩 추가로 획득
    - 예시: 한 번에 3줄을 지우면 10 x 3 = 30점
- **콤보 보너스**: 콤보 상태에서 라인 제거에 성공하면, 콤보 수치에 비례하여 추가 점수를 획득
    - 공식: `콤보 보너스 점수 = 10 x 콤보 수치 x 제거한 라인 수`
    - 예시: 5콤보 상태에서 2줄을 지우면 10 x 5 x 2 = 100점 추가

#### 점수 계산 예시

- 5칸짜리 블록을 배치하여 한 번에 3줄을 제거하고, 그 순간 7콤보가 되는 상황:
    - 블록 배치 점수: 5점
    - 라인 제거 점수: 10 x 3 = 30점
    - 콤보 보너스: 10 x 7 x 3 = 210점
    - **총합: 5 + 30 + 210 = 245점**

#### 콤보 유지의 중요성

- 콤보가 끊기지 않고 계속 쌓이면, 한 번에 획득할 수 있는 점수가 기하급수적으로 증가
- 따라서 고득점을 노리기 위해서는 콤보를 유지하는 전략이 매우 중요
- 콤보가 리셋되지 않도록, 연속적으로 라인 제거를 성공시키는 것이 핵심 플레이 전략

### 2.2. 블록
- **다양한 형태의 블록**: 1~5칸 크기의 직선, 정사각형, L자, T자, 십자 등 다양한 모양 제공

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
<img src="export/blue_block_new.png" width="32" alt="Blue Block" />
<img src="export/green_block_new.png" width="32" alt="Green Block" />
<img src="export/mint_block_new.png" width="32" alt="Mint Block" />
<img src="export/navy_block_new.png" width="32" alt="Navy Block" />
<img src="export/orange_block_new.png" width="32" alt="Orange Block" />
<img src="export/pink_block_new.png" width="32" alt="Pink Block" />
<img src="export/purple_block_new.png" width="32" alt="Purple Block" />
<img src="export/red_block_new.png" width="32" alt="Red Block" />
<img src="export/sky_block_new.png" width="32" alt="Sky Block" />
<img src="export/yellow_block_new.png" width="32" alt="Yellow Block" />
<br/>
<sub>[블록 색상별 예시]</sub>
</div>

- **10가지 색상 정책 및 색상 중복 방지**: 각 픽셀 블록은 10가지의 다양한 색상으로 구성되며, 하단에 제시되는 3개의 블록은 항상 서로 다른 색상으로 등장하도록 설계하여 시각적 구별성과 게임의 완성도를 높임.

아래 표는 `BlockManager.cs`에 실제 정의된 54개 블록의 도식, 배열, 크기, 설명을 모두 정리한 표임. 각 블록은 실제 게임에서 사용되는 형태와 동일하게 2차원 배열로 표현되며, 도식은 1=■, 0=공백으로 시각화함.

| 번호 | 도식 | 배열 | 크기 (WxH) | 블록 설명 |
|:---:|:---:|:---:|:---:|:---|
| 1 | <pre>■</pre> | { {1} } | 1x1 | 단일 블록 |
| 2 | <pre>■■</pre> | { {1,1} } | 2x1 | 가로 2칸 |
| 3 | <pre>■<br>■</pre> | { {1}, {1} } | 1x2 | 세로 2칸 |
| 4 | <pre>■□<br>□■</pre> | { {1,0}, {0,1} } | 2x2 | 대각선 2칸 (↘) |
| 5 | <pre>□■<br>■□</pre> | { {0,1}, {1,0} } | 2x2 | 대각선 2칸 (↙) |
| 6 | <pre>■■■</pre> | { {1,1,1} } | 3x1 | 가로 3칸 |
| 7 | <pre>■<br>■<br>■</pre> | { {1}, {1}, {1} } | 1x3 | 세로 3칸 |
| 8 | <pre>■■<br>■□</pre> | { {1,1}, {1,0} } | 2x2 | ㄴ자 (좌상) |
| 9 | <pre>■■<br>□■</pre> | { {1,1}, {0,1} } | 2x2 | ㄱ자 (우상) |
| 10 | <pre>□■<br>■■</pre> | { {0,1}, {1,1} } | 2x2 | ㄱ자 (좌하) |
| 11 | <pre>■□<br>■■</pre> | { {1,0}, {1,1} } | 2x2 | ㄴ자 (우하) |
| 12 | <pre>■□□<br>□■□<br>□□■</pre> | { {1,0,0}, {0,1,0}, {0,0,1} } | 3x3 | 대각선 3칸 (↘) |
| 13 | <pre>□□■<br>□■□<br>■□□</pre> | { {0,0,1}, {0,1,0}, {1,0,0} } | 3x3 | 대각선 3칸 (↙) |
| 14 | <pre>■■<br>■■</pre> | { {1,1}, {1,1} } | 2x2 | 정사각형 4칸 |
| 15 | <pre>■■■■</pre> | { {1,1,1,1} } | 4x1 | 가로 4칸 |
| 16 | <pre>■<br>■<br>■<br>■</pre> | { {1}, {1}, {1}, {1} } | 1x4 | 세로 4칸 |
| 17 | <pre>■□<br>■□<br>■■</pre> | { {1,0}, {1,0}, {1,1} } | 2x3 | ㄴ자 (좌상, 3칸) |
| 18 | <pre>■■<br>■□<br>■□</pre> | { {1,1}, {1,0}, {1,0} } | 2x3 | ㄴ자 (좌하, 3칸) |
| 19 | <pre>□■<br>□■<br>■■</pre> | { {0,1}, {0,1}, {1,1} } | 2x3 | ㄱ자 (우상, 3칸) |
| 20 | <pre>■■<br>□■<br>□■</pre> | { {1,1}, {0,1}, {0,1} } | 2x3 | ㄱ자 (우하, 3칸) |
| 21 | <pre>■■■<br>■□□</pre> | { {1,1,1}, {1,0,0} } | 3x2 | ㄴ자 (좌상, 4칸) |
| 22 | <pre>■■■<br>□□■</pre> | { {1,1,1}, {0,0,1} } | 3x2 | ㄱ자 (우상, 4칸) |
| 23 | <pre>■□□<br>■■■</pre> | { {1,0,0}, {1,1,1} } | 3x2 | ㄴ자 (좌하, 4칸) |
| 24 | <pre>□□■<br>■■■</pre> | { {0,0,1}, {1,1,1} } | 3x2 | ㄱ자 (우하, 4칸) |
| 25 | <pre>■■■<br>□■□</pre> | { {1,1,1}, {0,1,0} } | 3x2 | ㅗ자 |
| 26 | <pre>□■□<br>■■■</pre> | { {0,1,0}, {1,1,1} } | 3x2 | ㅜ자 |
| 27 | <pre>■□<br>■■<br>■□</pre> | { {1,0}, {1,1}, {1,0} } | 2x3 | ㅓ자 |
| 28 | <pre>□■<br>■■<br>□■</pre> | { {0,1}, {1,1}, {0,1} } | 2x3 | ㅏ자 |
| 29 | <pre>■■□<br>□■■</pre> | { {1,1,0}, {0,1,1} } | 3x2 | S자 (좌상) |
| 30 | <pre>□■■<br>■■□</pre> | { {0,1,1}, {1,1,0} } | 3x2 | S자 (우상) |
| 31 | <pre>□■<br>■■<br>■□</pre> | { {0,1}, {1,1}, {1,0} } | 2x3 | Z자 (좌상) |
| 32 | <pre>■□<br>■■<br>□■</pre> | { {1,0}, {1,1}, {0,1} } | 2x3 | Z자 (우상) |
| 33 | <pre>■■■■■</pre> | { {1,1,1,1,1} } | 5x1 | 가로 5칸 |
| 34 | <pre>■<br>■<br>■<br>■<br>■</pre> | { {1}, {1}, {1}, {1}, {1} } | 1x5 | 세로 5칸 |
| 35 | <pre>■■■■<br>■□□□</pre> | { {1,1,1,1}, {1,0,0,0} } | 4x2 | ㄴ자 (좌상, 5칸) |
| 36 | <pre>■■■■<br>□□□■</pre> | { {1,1,1,1}, {0,0,0,1} } | 4x2 | ㄱ자 (우상, 5칸) |
| 37 | <pre>■□□□<br>■■■■</pre> | { {1,0,0,0}, {1,1,1,1} } | 4x2 | ㄴ자 (좌하, 5칸) |
| 38 | <pre>□□□■<br>■■■■</pre> | { {0,0,0,1}, {1,1,1,1} } | 4x2 | ㄱ자 (우하, 5칸) |
| 39 | <pre>■■■<br>■□□<br>■□□</pre> | { {1,1,1}, {1,0,0}, {1,0,0} } | 3x3 | ㄴ자 (좌상, 6칸) |
| 40 | <pre>■■■<br>□□■<br>□□■</pre> | { {1,1,1}, {0,0,1}, {0,0,1} } | 3x3 | ㄱ자 (우상, 6칸) |
| 41 | <pre>■□□<br>■□□<br>■■■</pre> | { {1,0,0}, {1,0,0}, {1,1,1} } | 3x3 | ㄴ자 (좌하, 6칸) |
| 42 | <pre>□□■<br>□□■<br>■■■</pre> | { {0,0,1}, {0,0,1}, {1,1,1} } | 3x3 | ㄱ자 (우하, 6칸) |
| 43 | <pre>■■■<br>□■□<br>□■□</pre> | { {1,1,1}, {0,1,0}, {0,1,0} } | 3x3 | ㅗ자 (5블록) |
| 44 | <pre>□■□<br>□■□<br>■■■</pre> | { {0,1,0}, {0,1,0}, {1,1,1} } | 3x3 | ㅜ자 (5블록) |
| 45 | <pre>■□□<br>■■■<br>■□□</pre> | { {1,0,0}, {1,1,1}, {1,0,0} } | 3x3 | ㅓ자 (5블록) |
| 46 | <pre>□□■<br>■■■<br>□□■</pre> | { {0,0,1}, {1,1,1}, {0,0,1} } | 3x3 | ㅏ자 (5블록) |
| 47 | <pre>□■□<br>■■■<br>□■□</pre> | { {0,1,0}, {1,1,1}, {0,1,0} } | 3x3 | 십자(+) |
| 48 | <pre>■□■<br>■■■</pre> | { {1,0,1}, {1,1,1} } | 3x2 | ㅗ자 변형 |
| 49 | <pre>■■■<br>■□■</pre> | { {1,1,1}, {1,0,1} } | 3x2 | ㅜ자 변형 |
| 50 | <pre>■■<br>■□<br>■■</pre> | { {1,1}, {1,0}, {1,1} } | 2x3 | ㅓ자 변형 |
| 51 | <pre>■■<br>□■<br>■■</pre> | { {1,1}, {0,1}, {1,1} } | 2x3 | ㅏ자 변형 |
| 52 | <pre>■■■<br>■■■</pre> | { {1,1,1}, {1,1,1} } | 3x2 | 6블록 정사각형 |
| 53 | <pre>■■<br>■■<br>■■</pre> | { {1,1}, {1,1}, {1,1} } | 2x3 | 6블록 직사각형 |
| 54 | <pre>■■■<br>■■■<br>■■■</pre> | { {1,1,1}, {1,1,1}, {1,1,1} } | 3x3 | 9블록 정사각형 |

### 2.2.1. 블록 스폰(Spawn) 및 선택 알고리즘 예시

```csharp
// 블록 3개를 하나씩 선택하는 재귀 알고리즘 (BlockManager.cs)
private bool SelectBlocksRecursive(bool[,] grid, int blockIndex, List<BlockShape> selected)
{
    if (blockIndex >= 3)
        return true;
    
    // 현재 그리드 상태에서 배치 가능한 블록 후보 추출
    var validBlocks = possibleBlockShapes.Where(block => HasValidPlacementInTest(grid, block)).ToList();
    List<BlockShape> remainingBlocks = new List<BlockShape>(validBlocks);
    
    while (remainingBlocks.Count > 0)
    {
        // 가중치 기반 무작위 선택
        BlockShape selectedBlock = SelectWeightedRandomBlock(remainingBlocks);
        remainingBlocks.Remove(selectedBlock);
        
        bool[,] gridCopy = CloneGrid(grid);
        if (TryPlaceBlockAnywhere(gridCopy, selectedBlock))
        {
            CheckAndClearLines(gridCopy);
            selected.Add(selectedBlock);
            if (SelectBlocksRecursive(gridCopy, blockIndex + 1, selected))
                return true;
            selected.RemoveAt(selected.Count - 1);
        }
    }
    return false;
}

// 가중치 기반 무작위 블록 선택 (BlockManager.cs)
private BlockShape SelectWeightedRandomBlock(List<BlockShape> validBlocks)
{
    float totalWeight = 0;
    foreach (var block in validBlocks)
    {
        totalWeight += Mathf.Pow(random_weight, block.count - 1);
    }

    float randomValue = UnityEngine.Random.Range(0f, totalWeight);
    float currentSum = 0;

    foreach (var block in validBlocks)
    {
        currentSum += Mathf.Pow(random_weight, block.count - 1);
        if (randomValue <= currentSum)
        {
            return block;
        }
    }
    return validBlocks[validBlocks.Count - 1];
}
```

- 위 알고리즘은 실제 `BlockManager.cs`에서 3개의 블록을 하나씩, 그리드 상황에 따라 배치 가능성을 시뮬레이션하며 선택하는 구조임
- 각 블록은 가중치(블록 크기 등)에 따라 무작위로 선택되며, 반드시 배치 가능한 조합만 생성되도록 설계되어 있음 (게임 오버가 되지 않는 블록 배치 경우의 수가 최소 한 개는 존재)
- **가중치 기반 무작위 선택을 사용하는 이유**: 더 많은 블록 단위(큰 블록, 많은 칸을 차지하는 블록)로 구성된 블록이 더 높은 확률로 등장하도록 하여, 게임의 속도감, 전략성, 다양성을 높이고, 단순한 1~2칸짜리 블록만 반복적으로 등장하는 것을 방지하기 위함

### 2.3. 이펙트
- **블록 이펙트**: 블록 파괴 시 블록이 터지는 듯한 애니메이션 이펙트 적용
- **점수 이펙트**: 라인 완성으로 점수 획득 시 점수 UI 생성 및 라인 제거 애니메이션 이펙트 적용
- **콤보 이펙트**: 연속으로 라인을 지울 때 콤보 수치 및 보너스 점수 획득 효과 적용

### 2.4. UI/UX

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
    <img src="Play_Images/Gameover.png" width="200" alt="게임 오버"/>
    <br/>
    <sub>[게임 오버]</sub>
</div>

- **게임 화면**: 8x8 그리드, 블록 선택 영역, 점수 표시, 설정 버튼 포함
- **설정 화면**: 배경음악, 효과음, 진동 On/Off 및 순위 보기 버튼 제공
- **게임 오버 화면**: 재시작, 부활(리워드 광고 시청) 버튼 제공

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
    <img src="Play_Images/Line_UX.png" width="200" alt="Line 완성"/>
    <br/>
    <sub>[Line 완성]</sub>
</div>

- **UX 설계**:
    - 직관적인 드래그 앤 드롭 지원
    - 블록을 놓을 수 있는 상황일 때, 해당 위치의 그리드에 shadow(그림자)를 표시하여 플레이어가 즉시 배치 가능 여부를 시각적으로 확실히 인지할 수 있도록 구현
    - 블록이 터질 때(줄이 완성되어 사라질 때) 진동(Vibration) 효과를 제공하여 타격감과 피드백을 강화
    - 콤보가 쌓일 때, 콤보 수치를 화면 중앙에 크게 표시하여 플레이어가 연속 성공을 즉각적으로 인지할 수 있도록 설계
    - 블록 배치 가능/불가 시 색상 변화 등 시각적 피드백 제공함
    - 줄(line) 완성 가능한 경우, 해당 줄의 블록들은 점멸 애니메이션이 적용되어 즉시 시각적 피드백을 제공
    - 하단에 제시된 블록을 터치하는 즉시, 해당 블록이 그리드 쪽으로 이동하여 손가락 움직임에 따라 이동하도록 구현함. 이를 통해 사용자는 블록을 누르고 끌어올 필요 없이, 터치와 동시에 바로 배치 동작을 시작할 수 있어 손가락의 이동 거리가 줄고, 반복 플레이 시 피로도가 크게 감소함.

## 3. 확장 요소 및 기타 시스템

### 3.1. 랭킹 시스템(Leaderboard): 

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
<img src="Play_Images/ui_leaderboard.jpg" width="200" alt="리더보드"/>
<br/>
<sub>[리더보드]</sub>
</div>

- 플레이어의 최고 점수를 서버에 저장하고, 상위 랭커를 실시간으로 조회할 수 있는 시스템이 구현되어 있음.
- Unity Services의 Leaderboards 패키지를 활용하여, 익명 인증 및 점수 제출, 닉네임 입력, 실시간 랭킹 조회, 내 순위 표시, 100위까지의 랭커 리스트, 닉네임 검증(한글/영문/숫자/특수문자, 금지어, 바이트 제한) 등 다양한 기능을 제공함.
- UI는 `LeaderboardEntryUI`, `LeaderboardPanel` 등으로 구성되어 있으며, 닉네임 입력 패널, 닉네임 입력에 대한 피드백 메시지, 내 순위 강조 등 UX가 강화되어 있음.

    ```csharp
    // 닉네임 검증 (외부 금지어 파일 대조 포함, 실제 코드 기반)
    private bool IsNicknameValid(string nickname)
    {
        // 1. 허용 문자(영문, 숫자, 밑줄, 공백, 한글)
        string pattern = @"^[a-zA-Z0-9_ \uAC00-\uD7A3]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(nickname, pattern)) return false;
        // 2. 금지어(외부 파일) 대조
        foreach (string bad in bannedWords)
        {
            if (string.IsNullOrWhiteSpace(bad)) continue;
            if (nickname.IndexOf(bad, StringComparison.OrdinalIgnoreCase) >= 0) return false;
        }
        // 3. 바이트 수 제한(UTF-8 기준 20바이트)
        if (System.Text.Encoding.UTF8.GetByteCount(nickname) > 20) return false;
        return true;
    }

    // 점수 제출 (비동기, 닉네임 검증 포함, 실제 구조 기반)
    public async void SubmitScore(int score, string nickname)
    {
        if (!IsNicknameValid(nickname))
        {
            Debug.LogWarning("닉네임이 유효하지 않음");
            return;
        }
        try
        {
            var options = new AddPlayerScoreOptions { Metadata = new Dictionary<string, string> { { "nickname", nickname } } };
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
            Debug.Log($"Score submitted: {score} with nickname: {nickname}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to submit score: " + ex.Message);
        }
    }

    // 랭킹 조회 (비동기, 실제 구조 기반)
    public async void GetLeaderboard()
    {
        if (isFetchingLeaderboard) return;
        isFetchingLeaderboard = true;
        try
        {
            var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 100, IncludeMetadata = true });
            // ...데이터 파싱 및 UI 갱신 로직...
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve leaderboard: " + ex.Message);
        }
        finally
        {
            isFetchingLeaderboard = false;
        }
    }
    ```
- **효율성**: 중복 호출 방지, 비동기 처리, 금지어 로컬 파일 캐싱 등으로 최적화되어 있음.
- **확장성**: Firebase, PlayFab 등 외부 서비스로의 확장도 고려할 수 있음.

### 3.2. 광고 시스템(Unity Ads)

본 게임은 **보상형 광고(Rewarded Ads)**와 **배너 광고(Banner Ads)** 두 가지 광고 방식을 도입하여, 수익성과 유저 경험의 균형을 추구함.

#### 3.2.1. 보상형 광고(Rewarded Ads)

- **노출 시점**: 게임 오버 시 1회에 한해 시청 가능. 동영상 광고를 끝까지 시청하면 1회 부활(게임 재개) 기회를 제공
- **제한**: 2번째 게임 오버부터는 더 이상 보상형 광고(부활)가 제공되지 않음
- **효율성**: 광고 로딩 상태 체크, 중복 노출 방지, 네트워크 예외 처리 등으로 안정성을 높임
- **수익화**: 보상형 광고는 유저가 자발적으로 시청할 때만 보상을 지급하여, 광고 시청률과 클릭률을 높이고, 유저 이탈을 최소화함

```csharp
// RewardedAdsButton.cs
public void ShowAd()
{
    _showAdButton.interactable = false;
    Advertisement.Show(_adUnitId, this);
}

public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
{
    if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
    {
        Debug.Log("Unity Ads Rewarded Ad Completed");
        // 실제 보상 지급: 1회 부활
        GameManager.Instance.ReviveGame();
    }
}

// GameManager.cs
public void ReviveGame()
{
    reviveCount++;
    isGameOver = false;
    gridManager.ClearGameOverBlocks();
    gridManager.ReactivateGridBlocks();
    blockManager.GenerateNewBlocks();
    uiManager.HideGameOverPanel();
    soundManager.PlayReviveSound();
    soundManager.PlayBGM();
    inputHandler.EnableDrag();
}
```

#### 3.2.2. 배너 광고(Banner Ads)

<div align="center" style="margin-top: 24px; margin-bottom: 24px">
    <img src="Play_Images/banner_ads.jpg" width="200" alt="배너 광고"/>
    <br/>
    <sub>[배너 광고]</sub>
</div>

- **노출 위치**: 게임 플레이 도중 화면의 가장 하단에 지속적으로 노출
- **목적**: 플레이 내내 자연스럽게 광고가 노출되어, 반복적인 노출을 통한 안정적인 광고 수익을 확보
- **UX 배려**: 더 높은 수익을 위해 클릭을 유도할 수도 있지만, 이는 유저 경험을 해칠 수 있으므로, 플레이에 방해가 되지 않도록 손에 잘 거슬리지 않는 하단에 배치함

## 4. 에셋

### 4.1. 아트 에셋

모든 픽셀 아트(블록, 그리드, UI 등)는 **_`Aseprite`_** 를 활용하여 직접 제작함. 각 픽셀 블록의 표정 및 움직임, 색상, 이펙트 등도 Aseprite로 세밀하게 작업하여 게임의 감성적 완성도를 높임.

- **블록 스프라이트**: 다양한 색상/모양의 블록 이미지로 구성
- **UI 이미지**: 버튼, 배경, 아이콘 등 제작
- **이펙트**: 줄 삭제, 콤보 등 시각 효과 구현

### 4.2. 사운드 에셋

게임의 배경음악(BGM)과 효과음(Sound Effect)은 **_`Bosca Ceoil`_**을 활용하여 직접 제작함. 게임의 분위기와 플레이 템포에 맞는 사운드를 자체적으로 작곡 및 편집하여, 상용 음원 없이 독창적인 사운드 경험을 제공함.

- **효과음**: 블록 배치, 줄 삭제, 게임 오버 등 제작
- **배경음악**: 게임 진행 중 재생하는 루프 BGM 작곡