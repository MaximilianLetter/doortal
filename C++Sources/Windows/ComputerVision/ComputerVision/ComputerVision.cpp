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

    // Corner demo https://docs.opencv.org/3.4/d2/d1d/samples_2cpp_2lkdemo_8cpp-example.html#a20
    __declspec(dllexport) void ProcessImage(void *result, Color32** rawImage, int width, int height)
    {
        float* ptr = (float*)result;

        Mat gray;
        Point2f point;
        const int MAX_COUNT = 50;
        vector<Point> points;

        // create an opencv object sharing the same data space
        Mat image(height, width, CV_8UC4, *rawImage);
        
        cvtColor(image, gray, COLOR_RGBA2GRAY);

        goodFeaturesToTrack(gray, points, MAX_COUNT, 0.01, 10, Mat(), 3, 3, 0, 0.04);

        if (!points.empty())
        {
            size_t i;
            for (i = 0; i < points.size(); i++)
            {
                circle(gray, points[i], 11, Scalar(0, 255, 0), -1, 8);
            }

            // TODO, collect x and y that belong to the same point
            
            // return the detected points as array
            /*for (int i = 0; i < 4; i++)
            {
                ptr[i] = points[i].x;
                ptr[i+1] = points[i].y;
            }*/

            /*ptr[0] = points[0].x;
            ptr[1] = points[0].y;
            ptr[2] = points[1].x;
            ptr[3] = points[1].y;
            ptr[4] = points[2].x;
            ptr[5] = points[2].y;
            ptr[6] = points[3].x;
            ptr[7] = points[3].y;*/

            ptr[0] = 50;
            ptr[1] = 50;

            ptr[2] = 100;
            ptr[3] = 50;

            ptr[4] = 100;
            ptr[5] = 100;

            ptr[6] = 50;
            ptr[7] = 100;
        }

        // format has to be the same
        cvtColor(gray, image, COLOR_GRAY2RGBA);
    }
}
