# VR-Dinosaur-Museum Unity Project
- 총 파일 크기: 9.26GB (그러나 용량 제한으로 인해 Assets/Scripts에 해당하는 파일만 Github에 업로드하였습니다.)
- **Unity 버전: 2022.3.49f1 (Apple Silicon, LTS)**
- 반드시 유니티 버전이 매칭되어야 하고, Android로 빌드되어야 하고, 환경설정이 맞아야 하며, Package를 모두 다운받아야 함 (하단에 정리되어있음)
- 메인 카메라는 Meta Quest의 카메라를 대신해서 사용하고 있음, 그러나 MAC에서도 테스팅 가능
- Shader는 반드시 Built-In을 사용해야 하며, Shader 문제로 에셋의 뮤지엄의 텍스쳐가 핑크색으로 보이는 경우 Trouble-Shooting해야 함
    - 에셋 정보:
        [Historical Museum](https://assetstore.unity.com/packages/3d/environments/historic/historical-museum-251130)
- FastAPI 서버가 열려있어야만 UseCase가 작동을 함 (현재는 법인카드 지원이 끊겨서 서버가 닫혀있음)
- S3의 버킷 이름과 Access Key, Secret Key가 원래는 코드 내에 있었으나, 보안 상의 이유로 현재는 지워둠 (현재는 법인카드 지원이 끊겨서 S3가 닫혀있음)
- S3에 내가 원하는 PDF가 업로드 되어있어야 하며, 이를 Unity 내에서 명시해주어야 함 (하단에 사진)
<img width="1710" alt="Unity 내에서 PDF 이름을 명시하는 사진" src="https://github.com/user-attachments/assets/052e8029-b272-41b3-8d22-4678b7ce1283">

**전체 파일 링크:** https://drive.google.com/file/d/17sJgxee8NjPn755P1dEbaOOSZyU4oSo0/view?usp=sharing

## Unity의 디렉토리

### Assets

- **Files** - S3에서 다운받은 PDF, 3D 모델(fbx, glb)을 이 폴더에 저장
- Fonts
- **Images** - S3에서 다운받은 PDF를 이미지로 파싱하여 이 폴더에 저장
- **LeartesStudios** - 3D 박물관 유니티 에셋
- Materials
- Oculus
- Packages
- Plugins
- Resources
- **Scenes** - 공룡 뮤지엄 씬이 담겨있는 디렉토리
    - **DinosaurMuseumScene** - 최종발표에서 사용한 공룡 뮤지엄 씬
    - **TestScene** - 중간발표 때까지 테스트용으로 사용하던 씬
- **Scripts** - 모든 코드가 담겨있는 디렉토리
    - **Canvas Related** - 프론트엔드 관련 코드들
        - **ButtonDeactivator.cs** - 버튼 비활성화하는 코드
        - **DescribeModeController.cs** - Describe를 보여줄지 가릴지 결정하는 코드
        - **FollowPlayer.cs** - Describe가 플레이어를 따라다니도록 하는 코드
        - **HideSelf.cs** - Unity App을 처음에 실행 시, 숨겨야 하는 오브젝트는 숨기는 코드
        - **InputFieldController.cs** - 사용자가 입력중일 땐 다른 키들을 비활성화하는 코드
        - **InputManager.cs** - 입력 필드를 생성/제거하는 코드
        - **MouseMovement.cs** - 플레이어의 카메라 시점을 이동하는 코드
        - **OpenQuery.cs** - 입력 필드 관련 Canvas를 여는/닫는 코드, 지금은 사용 X
        - **PlayerMovement.cs** - 플레이어를 움직이는 코드
        - **ResetPlayerPosition.cs** - 플레이어를 처음 위치로 초기화하는 코드
        - **UrlImageLoader.cs** - URL 링크에 알맞는 이미지를 띄우는 코드 (Describe용)
    - **Demonstration** - 시연 영상 및 Voice SDK 관련 코드들
        - **TransparentSelf.cs** - 시연영상에 필요없는 요소를 가리는 코드, 지금은 사용 X
        - **WitActivation.cs** - 마이크를 활성화하여 VoiceSDK로 연결해주는 코드
    - **S3 Related** - S3에서 파일을 다운로드/업로드하는 코드들
        - **ImageLoader.cs** - S3에 담긴 이미지를 띄우는 코드, 지금은 사용 X
        - **ModelLoader.cs** - 3D 모델(fbx, glb)을 띄우는 코드
        - **PDFLoader.cs** - PDF를 이미지로 파싱 & eBook 관련 기능이 구현된 코드
        - **PDFOpener.cs** - PDF의 이미지를 Unity에 렌더링하는 코드 - 지금은 사용 X
        - **S3Downloader.cs** - S3에 담긴 PDF, 3D 모델을 다운받는 코드
        - **S3ImageLoader.cs** - S3에 담긴 PDF를 다운받는 코드, 지금은 사용 X
        - **UnityMainThreadDispatcher.cs** - Unity의 Main Thread 관련 코드
        - **UploadToS3.cs** - 지금 보고 있는 PDF 이미지를 S3에 업로드하는 코드
    - **UseCases** - Create3D, Describe 코드들
        - **Create3D.cs** - Create3D Intent와 관련한 모든 메소드가 담긴 코드
        - **Describe.cs** -  Describe Intent와 관련한 모든 메소드가 담긴 코드
- TextMesh Pro

## Unity 환경설정 / 설치된 Package들

### Build Settings (하단에 사진)
<img width="636" alt="Build Settings를 Android로 설정한 사진" src="https://github.com/user-attachments/assets/78bff230-ac76-4cdf-afa8-7c166387ab00">

### Project Settings > Player (하단에 사진)
<img width="1399" alt="Project Settings > Player 사진" src="https://github.com/user-attachments/assets/a5e704bc-9080-40db-8190-c02944a496d3">

### 다운받은 / 설치한 Unity Package 리스트 (하단에 사진 2장)
<img width="424" alt="설치한 Unity Package 사진 (1)" src="https://github.com/user-attachments/assets/dcad497f-74ff-4c3a-a53a-2bdd68fc0cb7">
<img width="427" alt="설치한 Unity Package 사진 (2)" src="https://github.com/user-attachments/assets/19c82960-178a-486a-a023-1143ed1e0b7d">

## MAC 내에서 작동시키는 법 (Key Mapping)

**`W` 키, `A` 키, `S` 키, `D` 키** = 플레이어 이동

**마우스 움직임** = 카메라 시점 이동

**`왼쪽 Option` 키** = 마우스 커서 보이기/숨기기

**`SPACE` 키** = 점프

**`[` 키, `]` 키** = PDF 페이지 앞뒤로 넘기기

**`E` 키** = Description을 보이기/숨기기 (그 안의 내용은 Describe Intent가 들어온 경우에만 생성됨)

**`R` 키** = 플레이어 위치 처음으로 초기화 (벽 등에 끼어서 안 움직일 때)

**`M` 키** = 마이크 활성화
