public enum ProjectileRotationMode
{
    None,
    DefaultFixed,   // 초기 상태 그대로 회전 상태 유지함
    RandomRotation, // 랜덤 회전값 사용
    UseVelocityDir, // 속도값 설정 시 해당 방향을 바라보도록 함
}
