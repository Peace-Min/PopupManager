# Popup Manager Refactoring Project

이 프로젝트는 기존 WPF 팝업 관리 예제에서 **의존성 주입(DI) 컨테이너 직접 구현**, **팝업 관리 구조 개선**, **UI 카테고리화**를 목표로 리팩토링되었습니다.

## 주요 변경 사항

### 1. 커스텀 의존성 주입(DI) 컨테이너 구현
*   **기존**: `Microsoft.Extensions.DependencyInjection` 라이브러리 사용.
*   **변경**: `Services/ServiceProvider.cs`를 직접 구현하여 외부 라이브러리 의존성을 제거했습니다.
    *   **Singleton**: 애플리케이션 수명 동안 단일 인스턴스 유지 (`DialogService`, `PopupManager`, `MainViewModel`).
    *   **Transient**: 요청 시마다 새로운 인스턴스 생성 (`SubViewModel1~4`).
    *   **Auto-wiring**: 생성자 주입을 통해 의존성을 자동으로 해결합니다.

### 2. DialogService 및 PopupManager 구조 개선
*   **창 제어 위임**: `PopupManager`가 직접 `Window` 객체를 제어하지 않고, `DialogService`의 `Hide`, `Close`, `Show` 메서드를 호출하여 제어하도록 변경했습니다 (Facade 패턴 적용).
*   **ViewModel 의존성 제거**: `ViewModelBase`에서 `Window` 객체에 대한 직접적인 참조를 제거하고, `DevExpress.Mvvm.BindableBase`를 상속받아 순수한 ViewModel 형태를 유지했습니다.

### 3. 뷰(View) 및 뷰모델(ViewModel) 카테고리화
*   **구조 분리**: `ViewModels/Left`, `ViewModels/Right` 및 `Views/Left`, `Views/Right` 폴더로 파일을 구조화했습니다.
*   **동적 그룹화**: `IGroupableViewModel` 인터페이스를 도입하여, ViewModel이 스스로 그룹("Left", "Right")을 정의하고 `MainViewModel`에서 이를 동적으로 필터링하도록 했습니다.

### 4. UI 개선 (MainView)
*   **카테고리별 버튼**: 왼쪽/오른쪽 영역에 각각 팝업 열기 버튼을 배치했습니다.
*   **최소화 목록 메뉴화**: 기존의 단순 리스트 대신, `Menu` 컨트롤을 사용하여 "팝업 (n)" 형태의 버튼으로 최소화된 항목 수를 표시하고, 클릭 시 드롭다운 메뉴로 목록을 제공합니다.

## 프로젝트 구조

*   **Services/**: `ServiceProvider` (Custom DI), `DialogService`, `PopupManager` 등 핵심 서비스 로직.
*   **ViewModels/**: `MainViewModel` 및 `Left/Right` 폴더 내의 `SubViewModel`들.
*   **Views/**: `MainView` 및 `Left/Right` 폴더 내의 `SubView`들.
*   **Models/**: 데이터 전달을 위한 모델 및 `IGroupableViewModel` 인터페이스.
