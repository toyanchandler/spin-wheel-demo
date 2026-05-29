# Card reveal (cash out)

| File | Responsibility |
|------|----------------|
| `WheelRewardOpeningView` | Shows collected rewards after leave / cash out |
| `WheelRewardOpeningDeckRenderer` | Deck layout from HUD/outcome snapshots |
| `WheelRewardOpeningAnimator` | Opening motion |
| `WheelRewardCardView` | Single card presentation |

Triggered when phase is `CashedOut` and publisher raises outcome/HUD snapshots. See `ARCHITECTURE_AND_LOGIC.md` §17.
