# Omok_Server (온라인 오목 게임 서버)

## 📖 프로젝트 소개

ASP.NET Core Web API와 SignalR을 사용하여 개발한 온라인 오목 게임 서버입니다. RESTful API를 통해 사용자 인증 및 데이터 관리를 처리하고, SignalR을 통해 실시간 멀티플레이어 게임 로직을 구현하는 것을 목표로 합니다.

## ✨ 주요 기능 (현재까지 구현 완료)

- **사용자 시스템 (User System)**
  - 회원가입 (BCrypt를 이용한 비밀번호 해싱)
  - 로그인 (JWT 발급)

- **인증 및 인가 (Authentication & Authorization)**
  - JWT Bearer 토큰을 이용한 API 요청 인증
  - `[Authorize]` 어트리뷰트를 통한 엔드포인트 접근 제어

- **게임 데이터 관리 (Game Data Management)**
  - 경기 결과 저장
  - 사용자별 전적 조회

- **아키텍처 (Architecture)**
  - Repository 패턴 및 Service 패턴을 적용한 계층형 아키텍처 구축
  - DI(의존성 주입)를 통한 유연한 코드 구조 설계

## 🛠️ 사용 기술 (Tech Stack)

- **Backend:** ASP.NET Core 8
- **Database:** MySQL
- **Data Access:** SqlKata (Query Builder)
- **Authentication:** JWT (JSON Web Token)
- **Security:** BCrypt.Net-Next (Password Hashing)
- **Logging:** ZLogger
- **Real-time Communication:** SignalR (예정)

## 🚀 앞으로의 계획 (TODO)

- [X] DTO 패턴을 적용하여 전체 구조 리팩토링
- [ ] Refresh Token을 이용한 JWT 인증 시스템 고도화
- [X] SignalR 허브(Hub) 추가 및 실시간 통신 기반 마련
- [X] 인메모리 대기열을 이용한 매치메이킹 시스템 구현
- [X] `GameRoom` 상태 관리 로직 구현 (게임 시작, 수 두기, 게임 종료)
