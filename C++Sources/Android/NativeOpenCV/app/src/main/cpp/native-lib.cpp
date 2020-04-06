#include <jni.h>
#include <string>
#include "include/opencv2/opencv.hpp"

using namespace cv;

extern "C" JNIEXPORT jstring JNICALL
Java_com_example_nativeopencv_MainActivity_stringFromJNI(
        JNIEnv* env,
        jobject /* this */) {
    std::string hello = "Hello from C++";
    return env->NewStringUTF(hello.c_str());
}

struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};


extern "C" {
    int GenerateNumber()
    {
        return rand() % 10;
    }

    void ProcessImage(Color32** rawImage, int width, int height)
    {
    // create an opencv object sharing the same data space
    Mat image(height, width, CV_8UC4, *rawImage);

    // start with flip (in both directions) if your image looks inverted
    flip(image, image, -1);

    // start processing the image
    // ************************************************

    Mat edges;
    Canny(image, edges, 50, 200);
    dilate(edges, edges, (5, 5));
    cvtColor(edges, edges, COLOR_GRAY2RGBA);
    normalize(edges, edges, 0, 1, NORM_MINMAX);
    multiply(image, edges, image);

    // end processing the image
    // ************************************************

    // flip again (just vertically) to get the right orientation
    flip(image, image, 0);
}
}
