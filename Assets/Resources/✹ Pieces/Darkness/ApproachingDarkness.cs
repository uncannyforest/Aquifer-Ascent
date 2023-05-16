public class ApproachingDarkness : BooleanScript {
	public InDarkness darknessCheckHead;
	public InDarkness darknessCheckFeet;
    public bool shouldHinderMovement = true;

    public bool IsApproachingDarkness {
        get => darknessCheckHead.IsInDarkness && darknessCheckFeet.IsInDarkness;
    }

    public bool IsMovementProhibited {
        get => shouldHinderMovement && IsApproachingDarkness;
    }

    override public bool IsActive {
        get => IsMovementProhibited;
    }
}
