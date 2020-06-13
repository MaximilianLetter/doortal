#include <jni.h>
#include <string>

#include <opencv2/opencv.hpp>

#define _USE_MATH_DEFINES
#include <math.h>

extern "C" JNIEXPORT jstring JNICALL
Java_com_example_nativeopencv_MainActivity_stringFromJNI(
        JNIEnv* env,
        jobject /* this */) {
    std::string hello = "Hello from C++";
    return env->NewStringUTF(hello.c_str());
}

using namespace cv;
using namespace std;

// Resembles the Color32 format type of Unity
struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};

// Resembles the Vector2 data type of Unity
struct Vector2
{
    float x;
    float y;
};

// Declare all used constants

// Resolution constant
const int RES = 480;

// ROI constants
const float ROI_WIDTH = 0.8;
const float ROI_HEIGHT = 0.125;

// Contrast constant
const float CONTRAST = 1.2;

// Blur constants
const Size BLUR_KERNEL = Size(3, 3);
const float BLUR_SIGMA = 2.5;

// Canny constants
const int CANNY_LOWER = 50;
const int CANNY_UPPER = 200;

// NOTE: these values need to be improved to ensure to always find the corners of a door
// Corner detection constants
const int CORNERS_MAX = 50;
const float CORNERS_BOT_QUALITY = 0.05;
const float CORNERS_TOP_QUALITY = 0.01;
const float CORNERS_MIN_DIST = 15.0;
const bool CORNERS_HARRIS = false;

// Vertical lines constants
const float LINE_MAX = 0.9;
const float LINE_MIN = 0.3;
const float LINE_ANGLE_MIN = 0.875; // RAD

// Rectangles constants
const float ANGLE_MAX = 0.175; // RAD
const float LENGTH_DIFF_MAX = 0.12;
const float ASPECT_RATIO_MIN = 0.3;
const float ASPECT_RATIO_MAX = 0.7;
const float LENGTH_HOR_DIFF_MAX = 1.2;
const float LENGTH_HOR_DIFF_MIN = 0.7;
const float RECTANGLE_THRESH = 10.0;
const float RECTANGLE_OPPOSITE_THRESH = 10.0;

// Comparison of rectangles to edges constants
const float RECT_THRESH = 0.85;
const float LINE_THRESH = 0.5;
const int LINE_WIDTH = 2;
const float BOT_LINE_BONUS = 0.25;

// Selection of best candidate constants
const float UPVOTE_FACTOR = 1.2;
const float DOOR_IN_DOOR_DIFF_THRESH = 18.0; // Divider of image height
const float COLOR_DIFF_THRESH = 50.0;
const float ANGLE_DEVIATION_THRESH = 10.0;
const float CLOSE_TO_INPUT_THRESH = 22.0;

// Declare all used functions
bool detect(Mat& image, Vector2 inputPoint, vector<Point2f>& result);
vector<vector<Point2f>> cornersToVertLines(vector<Point2f> cornersBot, vector<Point2f> cornersTop, int height);
vector<vector<Point2f>> vertLinesToRectangles(vector<vector<Point2f>> lines);
float compareRectangleToEdges(vector<Point2f> rect, Mat edges);
vector<Point2f> selectBestCandidate(vector<vector<Point2f>> candidates, vector<float> scores, Point inputPoint, Mat gray);

float getDistance(Point2f p1, Point2f p2);
float getOrientation(Point2f p1, Point2f p2);
float getCornerAngle(Point2f p1, Point2f p2, Point2f p3);

bool detect(Mat& image, Point inputPoint, vector<Point2f>& result)
{
    // Scale image down
    // NOTE: the image comes in downscaled
    int width = image.size().width;
    int height = image.size().height;
    float ratio = float(height) / float(width);
    // resize(image, image, Size(RES, int(RES * ratio)), 0.0, 0.0, INTER_AREA);*/
    // NOTE: different interpolation methods can be used

    // Convert to grayscale
    Mat gray;
    cvtColor(image, gray, COLOR_BGR2GRAY);

    // Increase contrast
    gray.convertTo(gray, -1, CONTRAST, 0);

    // Blur the image
    Mat blurred;
    GaussianBlur(gray, blurred, BLUR_KERNEL, BLUR_SIGMA);

    // Generate edges
    Mat edges;
    Canny(blurred, edges, CANNY_LOWER, CANNY_UPPER);

    // Find ROI's based on user input
    Mat maskBot, maskTop;

    // Bottom ROI
    int roiBotWidth = width * ROI_WIDTH;
    int roiBotHeight = height * ROI_HEIGHT;
    Point2f roiPoint = Point2f(inputPoint.x - roiBotWidth / 2, inputPoint.y - roiBotHeight / 2);
    Rect roiBot = Rect(roiPoint.x, roiPoint.y, roiBotWidth, roiBotHeight);

    // Cut overlapping parts off
    roiBot = roiBot & Rect(0, 0, width, height);

    maskBot = Mat::zeros(image.size(), CV_8U);
    maskBot(roiBot) = 1;

    // Top ROI
    int lowLineBot = roiBot.y + (roiBot.height / 2);
    int roiTopHeight = lowLineBot - (LINE_MIN * height);

    Point polygonPoints[4] = {
            Point(0, 0),
            Point(width, 0),
            Point(roiBot.x + roiBot.width, roiTopHeight),
            Point(roiBot.x, roiTopHeight)
    }; // NOTE: order matters

    maskTop = Mat::zeros(image.size(), CV_8U);
    fillConvexPoly(maskTop, polygonPoints, 4, cv::Scalar(255));

    // Find corners using the given masks
    vector<Point2f> cornersBot, cornersTop;

    goodFeaturesToTrack(blurred, cornersBot, CORNERS_MAX, CORNERS_BOT_QUALITY, CORNERS_MIN_DIST, maskBot, 3, CORNERS_HARRIS);
    goodFeaturesToTrack(blurred, cornersTop, CORNERS_MAX, CORNERS_TOP_QUALITY, CORNERS_MIN_DIST, maskTop, 3, CORNERS_HARRIS);

    // Connect corners to vertical lines
    vector<vector<Point2f>> lines = cornersToVertLines(cornersBot, cornersTop, int(RES * ratio));

    // Group corners based on found lines to rectangles
    vector<vector<Point2f>> rectangles = vertLinesToRectangles(lines);

    // NOTE: this could be done in vertLinesToRectangles aswell
    // Compare the found rectangles to the edge image
    vector<vector<Point2f>> candidates;
    vector<float> scores;
    for (int i = 0; i < rectangles.size(); i++)
    {
        float result = compareRectangleToEdges(rectangles[i], edges);

        if (result > RECT_THRESH)
        {
            candidates.push_back(rectangles[i]);
            scores.push_back(result);
        }
    }

    // Select the best candidate out of the given rectangles
    if (candidates.size())
    {
        vector<Point2f> door = selectBestCandidate(candidates, scores, inputPoint, gray);

        result = door;

        return true;
    }

    return false;
}

// Group corners to vertical lines that represent the door posts
vector<vector<Point2f>> cornersToVertLines(vector<Point2f> cornersBot, vector<Point2f> cornersTop, int height)
{
    vector<vector<Point2f>> lines;
    vector<bool> done;

    for (int i = 0; i < cornersBot.size(); i++)
    {
        for (int j = 0; j < cornersTop.size(); j++)
        {
            float orientation = getOrientation(cornersBot[i], cornersTop[j]);
            if (orientation < LINE_ANGLE_MIN)
            {
                continue;
            }

            vector<Point2f> line = { cornersTop[j], cornersBot[i] };
            lines.push_back(line);
        }
    }

    return lines;
}

// Group rectangles that represent door candidates out of vertical lines
vector<vector<Point2f>> vertLinesToRectangles(vector<vector<Point2f>> lines)
{
    vector<vector<Point2f>> rects;

    for (int i = 0; i < lines.size(); i++)
    {
        for (int j = 0; j < lines.size(); j++)
        {
            if (j <= i) continue;

            // Only build rectangle if the two lines are completely distinct
            if ((lines[i][0] == lines[j][0]) || (lines[i][0] == lines[j][1]) || (lines[i][1] == lines[j][0]) || (lines[i][1] == lines[j][1]))
            {
                continue;
            }

            // NOTE: both of these values was calculated before,
            // maybe store them for reusage
            // Check if length difference of lines is close
            float length1 = getDistance(lines[i][0], lines[i][1]);
            float length2 = getDistance(lines[j][0], lines[j][1]);
            float lengthDiff = abs(length1 - length2);
            float lengthAvg = (length1 + length2) / 2;

            if (lengthDiff > (lengthAvg * LENGTH_DIFF_MAX))
            {
                continue;
            }

            // Check if top distance is in range of the given aspect ratio
            float lengthMin = lengthAvg * ASPECT_RATIO_MIN;
            float lengthMax = lengthAvg * ASPECT_RATIO_MAX;

            float distanceTop = getDistance(lines[i][0], lines[j][0]);
            if (distanceTop < lengthMin || distanceTop > lengthMax)
            {
                continue;
            }

            // Check if bottom distance is similar to top distance
            float distanceBot = getDistance(lines[i][1], lines[j][1]);
            if (distanceBot > (distanceTop * LENGTH_HOR_DIFF_MAX)
                || distanceBot < (distanceTop * LENGTH_HOR_DIFF_MIN))
            {
                continue;
            }

            // Test orientation of top horizontal line
            float orientationTop = getOrientation(lines[i][0], lines[j][0]);
            if (orientationTop > ANGLE_MAX)
            {
                continue;
            }

            // Test orientation of bottom horizontal line
            float orientationBot = getOrientation(lines[i][0], lines[j][0]);
            if (orientationBot > ANGLE_MAX)
            {
                continue;
            }

            float angles[4];
            angles[0] = getCornerAngle(lines[i][1], lines[i][0], lines[j][0]);
            angles[1] = getCornerAngle(lines[i][0], lines[j][0], lines[j][1]);
            angles[2] = getCornerAngle(lines[j][0], lines[j][1], lines[i][1]);
            angles[3] = getCornerAngle(lines[j][1], lines[i][1], lines[i][0]);

            bool rectangular = true;

            for (int k = 0; k < 4; k++)
            {
                if (abs(90.0 - angles[k]) > RECTANGLE_THRESH)
                {
                    int kOpp = (k + 2) % 4;

                    if (abs(180.0 - (angles[k] + angles[kOpp]) > RECTANGLE_OPPOSITE_THRESH))
                    {
                        rectangular = false;
                        break;
                    }
                }
            }

            if (!rectangular) continue;


            // Sort in order: leftBot > leftTop > rightTop > rightBot
            vector<Point2f> group = { lines[i][1], lines[i][0], lines[j][0], lines[j][1] };
            rects.push_back(group);
        }
    }

    return rects;
}

// Compare a possible rectangle with the existing edges in the edge image
float compareRectangleToEdges(vector<Point2f> rect, Mat edges)
{
    float result = 0.0;
    float bottomBonus = 0.0;

    for (int i = 0; i < rect.size(); i++)
    {
        // Next point to connect
        int j = (i + 1) % 4;

        Mat mask = Mat::zeros(edges.size(), CV_8U);
        cv::line(mask, rect[i], rect[j], 1, LINE_WIDTH);

        // While this works, there might be a better option without copy
        Mat roi;
        edges.copyTo(roi, mask);

        float lineLength = getDistance(rect[i], rect[j]);
        float fillRatio = min(float(1.0), countNonZero(roi) / lineLength);

        if (i < 3)
        {
            if (fillRatio < LINE_THRESH)
            {
                return 0.0;
            }

            result += fillRatio;
        }
        else
        {
            // Bottom line
            bottomBonus = fillRatio * BOT_LINE_BONUS;
        }
    }

    // Get average fillRatio for all lines but bottom line
    result = (result / 3) + bottomBonus;

    return result;
}

// Select the candidate by comparing their scores, score boni if special requirements are met
vector<Point2f> selectBestCandidate(vector<vector<Point2f>> candidates, vector<float> scores, Point inputPoint, Mat gray)
{
    for (int i = 0; i < candidates.size(); i++)
    {
        // Test in inner content has a different color average
        int left = int((candidates[i][0].x + candidates[i][1].x) / 2);
        int top = int((candidates[i][1].y + candidates[i][2].y) / 2);
        int right = int((candidates[i][2].x + candidates[i][3].x) / 2);
        int bottom = int((candidates[i][3].y + candidates[i][0].y) / 2);

        // This whole process of masking the image seems like a workaround
        Rect rect = Rect(Point2i(left, bottom), Point2i(right, top));
        Mat mask = Mat::zeros(gray.size(), CV_8U);
        rectangle(mask, rect, 1);

        double inner = mean(gray, mask)[0];
        mask = 1 - mask;
        double outer = mean(gray, mask)[0];

        if (abs(inner - outer) > COLOR_DIFF_THRESH)
        {
            scores[i] *= UPVOTE_FACTOR;
        }

        // Check if there is a door with the same top corners
        for (int j = 0; j < candidates.size(); j++)
        {
            if (j == i) continue;

            if (candidates[i][1] == candidates[j][1] && candidates[i][2] == candidates[j][2])
            {
                scores[i] = scores[i] * UPVOTE_FACTOR;
            }
        }

        // Check how close the bottom line center is to the input point
		Point2f bottomCenterPoint = (candidates[i][3] + candidates[i][0]) / 2;

		if (getDistance(bottomCenterPoint, inputPoint) < CLOSE_TO_INPUT_THRESH)
		{
			scores[i] *= UPVOTE_FACTOR;
		}
    }

    int index = max_element(scores.begin(), scores.end()) - scores.begin();
    vector<Point2f> door = candidates[index];

    return door;
}

// Get the distance between two points
float getDistance(Point2f p1, Point2f p2)
{
    return sqrt(pow((p1.x - p2.x), 2) + pow((p1.y - p2.y), 2));
}

// Get the orientation of the line consisting of two points
float getOrientation(Point2f p1, Point2f p2)
{
    if (p1.x != p2.x)
    {
        return abs((2 / M_PI) * atan(abs(p1.y - p2.y) / abs(p1.x - p2.x)));
    }
    else
    {
        return 180.0;
    }
}

// Get the angle between three points forming two lines
float getCornerAngle(Point2f p1, Point2f p2, Point2f p3)
{
    Point2f p12 = p1 - p2;
    Point2f p32 = p3 - p2;

    float angle = p12.dot(p32) / (norm(p12) * norm(p32));
    angle = abs(acos(angle) * 180 / M_PI);

    return angle;
}



// Below code is callable from Unity
extern "C" {
    bool ProcessImage(Vector2* result, Color32* rawImage, Vector2 userInput, int width, int height, bool rotation)
    {
        // Form input values to OpenCV types
        Mat image(height, width, CV_8UC4, rawImage);
        vector<Point2f> door;
        Point inputPoint = Point(userInput.x, userInput.y);

        if (rotation) {
            // NOTE: the image is already flipped in C#
            // flip(image, image, 1);
            rotate(image, image, ROTATE_90_CLOCKWISE);
            resize(image, image, Size(image.cols, image.rows), 0, 0, INTER_LINEAR);
        }

        bool success = detect(image, inputPoint, door);

        if (success)
        {
            // Write found door corners into referenced result array
            for (int i = 0; i < 4; ++i)
            {
                Vector2 &vec = result[i];
                vec.x = door[i].x;
                vec.y = door[i].y;
            }

            return true;
        }

        return false;
    }
}
