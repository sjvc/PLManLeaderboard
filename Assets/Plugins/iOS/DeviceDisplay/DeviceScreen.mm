#import <UIKit/UIKit.h>
extern "C"
{
    int _iOSDeviceDisplayScaleFactor(){
        return [[UIScreen mainScreen] scale];
    }
}
