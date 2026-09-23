namespace Data
{
	public class KillData
	{
		public PlayerId SourceId;
		public PlayerId TargetId;
		public KillReason Reason;

		public KillData(PlayerId sourceId, PlayerId targetId, KillReason reason)
		{
			SourceId = sourceId;
			TargetId = targetId;
			Reason = reason;
		}

		public override string ToString()
		{
			return $"[SOURCE] {SourceId} [TARGET] {TargetId} [REASON] {Reason}";
		}
	}
}
