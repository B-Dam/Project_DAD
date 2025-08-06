public interface ISaveable
{
    // 고유 식별자 (씬 전환 시에도 변하지 않아야 함)
    string UniqueID { get; }

    // 이 객체의 상태를 직렬화해서 반환
    object CaptureState();

    // 저장된 상태를 받아서 복원
    void RestoreState(object state);
}