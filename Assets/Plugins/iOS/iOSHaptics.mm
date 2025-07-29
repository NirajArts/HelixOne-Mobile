#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

// iOS Haptic Feedback Plugin for Unity
extern "C" {
    void iOSHapticFeedback(int type) {
        if (@available(iOS 10.0, *)) {
            switch (type) {
                case 0: // Light
                {
                    UIImpactFeedbackGenerator *lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                    [lightGenerator impactOccurred];
                    break;
                }
                case 1: // Medium
                {
                    UIImpactFeedbackGenerator *mediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                    [mediumGenerator impactOccurred];
                    break;
                }
                case 2: // Heavy
                {
                    UIImpactFeedbackGenerator *heavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                    [heavyGenerator impactOccurred];
                    break;
                }
                default:
                {
                    UIImpactFeedbackGenerator *mediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                    [mediumGenerator impactOccurred];
                    break;
                }
            }
        } else {
            // Fallback for iOS versions prior to 10.0
            // Use AudioServicesPlaySystemSound for basic vibration
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
}
