#include <jni.h>
#include <string>
#include "include/opencv2/opencv.hpp"

using namespace cv;
using namespace std;

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

    bool ProcessImage(void *result, Color32** rawImage, int width, int height, bool rotation)
    {
        float* ptr = (float*)result;

        Mat gray, edges;
        const int MAX_COUNT = 50;

        // create an opencv object sharing the same data space
        Mat image(height, width, CV_8UC4, *rawImage);

        // image from Android device comes rotated
        if (rotation) {
            flip(image, image, 1);
            rotate(image, image, ROTATE_90_CLOCKWISE);
            resize(image, image, Size(image.cols,image.rows), 0, 0, INTER_LINEAR);
        }

        cvtColor(image, gray, COLOR_RGBA2GRAY);

        /*threshold(gray, mask, 0, 255, THRESH_BINARY_INV | THRESH_OTSU);
        canny()*/

        blur(gray, edges, Size(3, 3));
        Canny(edges, edges, 80, 255, 3);

        vector<vector<Point>> contours;
        findContours(edges, contours, RETR_EXTERNAL, CHAIN_APPROX_SIMPLE);

        vector<Point> approx;

        for (int i = 0; i < contours.size(); i++)
        {
            // Approximate contour with accuracy proportional
            // to the contour perimeter
            approxPolyDP(
                Mat(contours[i]),
                approx,
                arcLength(Mat(contours[i]), true) * 0.02,
                true
            );

            // Skip small or non-convex objects
            if (abs(contourArea(contours[i])) < 50 || !isContourConvex(approx))
                continue;

            if (approx.size() == 4) {
                /*ptr[0] = contours[i][0].x;
                ptr[1] = contours[i][0].y;
                ptr[2] = contours[i][1].x;
                ptr[3] = contours[i][1].y;
                ptr[4] = contours[i][2].x;
                ptr[5] = contours[i][2].y;
                ptr[6] = contours[i][3].x;
                ptr[7] = contours[i][3].y;*/

                ptr[0] = approx[0].x;
                ptr[1] = approx[0].y;
                ptr[2] = approx[1].x;
                ptr[3] = approx[1].y;
                ptr[4] = approx[2].x;
                ptr[5] = approx[2].y;
                ptr[6] = approx[3].x;
                ptr[7] = approx[3].y;

                cvtColor(edges, image, COLOR_GRAY2RGBA);

                line(image, contours[i][0], contours[i][1], Scalar(0, 0, 255), 10);
                line(image, contours[i][1], contours[i][2], Scalar(0, 0, 255), 10);
                line(image, contours[i][2], contours[i][3], Scalar(0, 0, 255), 10);
                line(image, contours[i][3], contours[i][0], Scalar(0, 0, 255), 10);

                return true;
            }
        }

        //if (!contours.empty()) {
        //    vector<Point> rect = contours[0];

        //    // it really is a rectangle
        //    if (rect.size() == 4) {
        //        ptr[0] = rect[0].x;
        //        ptr[1] = rect[0].y;
        //        ptr[2] = rect[1].x;
        //        ptr[3] = rect[1].y;
        //        ptr[4] = rect[2].x;
        //        ptr[5] = rect[2].y;
        //        ptr[6] = rect[3].x;
        //        ptr[7] = rect[3].y;

        //        cvtColor(edges, image, COLOR_GRAY2RGBA);

        //        line(image, rect[0], rect[1], Scalar(0, 0, 255), 10);
        //        line(image, rect[1], rect[2], Scalar(0, 0, 255), 10);
        //        line(image, rect[2], rect[3], Scalar(0, 0, 255), 10);
        //        line(image, rect[3], rect[0], Scalar(0, 0, 255), 10);

        //        return true;
        //    }
        //}

        // format has to be the same
        cvtColor(edges, image, COLOR_GRAY2RGBA);

        return false;
    }
}
