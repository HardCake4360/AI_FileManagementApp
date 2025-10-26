# Unity UI 변환 가이드

이 폴더는 Figma Make로 생성된 React 프로젝트를 Unity에서 재구성하기 위한 참고 자료입니다.

## 폴더 구성
- Images/: 추출된 SVG/PNG 리소스
- Colors.json: index.css에서 추출된 색상 팔레트
- LayoutReference/: React 컴포넌트 구조를 Unity Canvas 구조로 매핑할 때 참고

## Unity 구성 팁
1. Canvas Scaler → Scale With Screen Size, 기준 해상도는 390x844 권장
2. 각 React 컴포넌트를 Panel 또는 Group으로 변환
3. 버튼, 텍스트 등은 UGUI/Button, TMP_Text로 매핑
4. Colors.json의 색상 코드를 Unity Color로 적용
