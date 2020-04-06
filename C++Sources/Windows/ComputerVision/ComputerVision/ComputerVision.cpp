#include "opencv2/opencv.hpp"

struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};

extern "C"
{
    using namespace cv;
    using namespace std;

    // helper function:
    // finds a cosine of angle between vectors
    // from pt0->pt1 and from pt0->pt2
    /*static double angle(Point pt1, Point pt2, Point pt0)
    {
        double dx1 = pt1.x - pt0.x;
        double dy1 = pt1.y - pt0.y;
        double dx2 = pt2.x - pt0.x;
        double dy2 = pt2.y - pt0.y;
        return (dx1 * dx2 + dy1 * dy2) / sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2) + 1e-10);
    }*/


    // Corner demo https://docs.opencv.org/3.4/d2/d1d/samples_2cpp_2lkdemo_8cpp-example.html#a20
    __declspec(dllexport) bool ProcessImage(void *result, Color32** rawImage, int width, int height)
    {
        float* ptr = (float*)result;

        Mat gray, edges;
        const int MAX_COUNT = 50;

        // create an opencv object sharing the same data space
        Mat image(height, width, CV_8UC4, *rawImage);
        
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
            if (fabs(contourArea(contours[i])) < 50 || !isContourConvex(approx))
                continue;

            if (approx.size() == 4) {
                ptr[0] = contours[i][0].x;
                ptr[1] = contours[i][0].y;
                ptr[2] = contours[i][1].x;
                ptr[3] = contours[i][1].y;
                ptr[4] = contours[i][2].x;
                ptr[5] = contours[i][2].y;
                ptr[6] = contours[i][3].x;
                ptr[7] = contours[i][3].y;

                cvtColor(edges, image, COLOR_GRAY2RGBA);

                line(image, contours[i][0], contours[i][1], Scalar(0, 0, 255), 3);
                line(image, contours[i][1], contours[i][2], Scalar(0, 0, 255), 3);
                line(image, contours[i][2], contours[i][3], Scalar(0, 0, 255), 3);
                line(image, contours[i][3], contours[i][0], Scalar(0, 0, 255), 3);

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
